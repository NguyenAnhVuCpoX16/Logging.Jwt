# logging.Jwt

Blazor Web App với ASP.NET Core Identity, JWT authentication, EF Core và Swagger (chỉ truy cập khi đã đăng nhập).

## Cấu trúc

- `src/Logging.Jwt.Web` — Blazor UI, API, JWT, Swagger
- `src/Logging.Jwt.Data` — DbContext, `ApplicationUser`, migrations
- `scripts/` — CMD cho EF migrations

## Yêu cầu

- .NET 10 SDK
- SQL Server LocalDB (Windows)
- `dotnet tool install --global dotnet-ef`

## Chạy lần đầu

```powershell
cd d:\dev-linh-tinh\logging.Jwt\scripts
.\add-migration.cmd InitialCreate
.\update-database.cmd
```

```powershell
cd d:\dev-linh-tinh\logging.Jwt\src\Logging.Jwt.Web
dotnet run
```

Mở https://localhost:7145/login

## Tài khoản seed (Development)

| Email | Password |
|-------|----------|
| admin@local.dev | Admin@123 |

## Trang

| Route | Mô tả |
|-------|--------|
| `/login` | Đăng nhập, nhận JWT + cookie |
| `/` | Trang chủ (yêu cầu đăng nhập) |
| `/users/create` | Tạo user mới (yêu cầu đăng nhập) |
| `/swagger` | Swagger UI (yêu cầu cookie JWT sau login) |

## API (Controllers)

- `POST /auth/login` — Form đăng nhập (browser)
- `GET /auth/logout` — Đăng xuất (browser)
- `POST /api/auth/login` — Đăng nhập JSON
- `POST /api/auth/logout` — Đăng xuất JSON
- `POST /api/users` — Tạo user (`UsersController`, Authorize)
