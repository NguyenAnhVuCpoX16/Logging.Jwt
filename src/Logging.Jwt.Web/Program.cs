using System.Text;
using Microsoft.AspNetCore.Components;
using Logging.Jwt.Data;
using Logging.Jwt.Data.Entities;
using Logging.Jwt.Web.Components;
using Logging.Jwt.Web.Constants;
using Logging.Jwt.Web.Middleware;
using Logging.Jwt.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings are not configured.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token)
                && context.Request.Cookies.TryGetValue(AuthConstants.AccessTokenCookieName, out var cookieToken))
            {
                context.Token = cookieToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TokenStorage>();
builder.Services.AddScoped<AuthCookieService>();
builder.Services.AddScoped<TokenInitializer>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["App:BaseUrl"] ?? "https://localhost:5001/");
})
.AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp =>
{
    var navigation = sp.GetRequiredService<NavigationManager>();
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("Api");
    client.BaseAddress = new Uri(navigation.BaseUri);
    return client;
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Logging.Jwt API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

await SeedAdminUserAsync(app);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SwaggerAuthMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Logging.Jwt API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAntiforgery();
app.MapStaticAssets();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static async Task SeedAdminUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    const string adminEmail = "admin@local.dev";

    if (await userManager.FindByEmailAsync(adminEmail) is not null)
    {
        return;
    }

    var admin = new ApplicationUser
    {
        UserName = adminEmail,
        Email = adminEmail,
        DisplayName = "Administrator",
        EmailConfirmed = true
    };

    await userManager.CreateAsync(admin, "Admin@123");
}
