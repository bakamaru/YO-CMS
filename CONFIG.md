# YO-Framework Configuration Reference

Configuration is loaded in `Program.cs` in order (later files override earlier keys):

```csharp
config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
config.AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
config.AddJsonFile("app_data/yoconfig.json", optional: false, reloadOnChange: true);
config.AddJsonFile("app_data/securityconfig.json", optional: false, reloadOnChange: true);
config.AddJsonFile("app_data/cspconfig.json", optional: false, reloadOnChange: true);
config.AddJsonFile("app_data/optimizationconfig.json", optional: false, reloadOnChange: true);
config.AddJsonFile("app_data/fileconfig.json", optional: false, reloadOnChange: true);
config.AddJsonFile("app_data/apiconfig.json", optional: false, reloadOnChange: true);
```

All files in `App_Data/` have `reloadOnChange: true` — changes trigger `ConfigChangeEvent` which restarts the app via `IHostApplicationLifetime.StopApplication()`.

---

## 1. `appsettings.json`

**Path:** `src/App/YOApp/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=yocms;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Authority": "https://localhost:44314",
    "Audience": "aud"
  },
  "RateLimiting": {
    "TokenLimit": 20,
    "TokensPerPeriod": 20,
    "ReplenishmentSeconds": 30
  }
}
```

| Key | Type | Default | Description | Consumed By |
|-----|------|---------|-------------|-------------|
| `Logging:LogLevel:Default` | string | `"Information"` | ASP.NET default log level | ASP.NET Core logging infra |
| `Logging:LogLevel:Microsoft.AspNetCore` | string | `"Warning"` | ASP.NET framework log level | ASP.NET Core logging infra |
| `AllowedHosts` | string | `"*"` | CORS allowed hosts | ASP.NET Core host filtering |
| `ConnectionStrings:DefaultConnection` | string | — | Main DB connection string | `MsSQLFactory`, `NpgSqlFactory`, `DbBakRestoreService`, `YangOneConnectionStrings` |
| `Jwt:Authority` | string | `"https://localhost:44314"` | JWT token authority URL | `Startup.cs` JWT Bearer config |
| `Jwt:Audience` | string | `"aud"` | JWT token audience | `Startup.cs` JWT Bearer config |
| `RateLimiting:TokenLimit` | int | `20` | Max tokens in token-bucket | `RateLimitOptions`, `Startup.cs` |
| `RateLimiting:TokensPerPeriod` | int | `20` | Tokens replenished per period | `Startup.cs` rate limiter |
| `RateLimiting:ReplenishmentSeconds` | int | `30` | Seconds between replenishments | `Startup.cs` rate limiter |

**POCO:** `RateLimitOptions` (`YangOne.Web/API/RateLimitOptions.cs`)

---

## 2. `appsettings.Development.json`

**Path:** `src/App/YOApp/appsettings.Development.json` (optional)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Overrides `Logging:LogLevel:*` during development only.

---

## 3. `yoconfig.json`

**Path:** `src/App/YOApp/App_Data/yoconfig.json`

### `YangOneAppConfig` section

```json
{
  "YangOneAppConfig": {
    "AppName": "YangOne Framework",
    "SiteUrl": "https://localhost:7189",
    "IsInstalled": true,
    "DbProvider": "SQLServer",
    "Framework": "",
    "Version": "1.0.0",
    "EnableLogging": true,
    "EnableDbLogging": false,
    "Theme": "Default",
    "AdminTheme": "Admin",
    "FacebookAppId": "",
    "RequireConfirmedEmail": false,
    "PasswordLength": 8,
    "RequireNonAlphanumeric": false,
    "RequireUppercase": false,
    "DataProtectionKeyPath": "app_data",
    "CookieDomain": "localhost",
    "CookieIdentity": "kachuwa.id",
    "ApiTokenEndPoint": "https://localhost:7189/connect/token",
    "ApiEndPoint": "https://localhost:7189",
    "TokenAuthority": "https://localhost:7189",
    "SMTMConfig": {
      "Host": "",
      "UserName": "",
      "Password": "",
      "UseSSL": true,
      "Port": 587
    },
    "JobConnection": "JobConnection"
  }
}
```

**Binding:** `services.Configure<YangOneAppConfig>(configuration.GetSection("YangOneAppConfig"))` in `YangOneCoreExtensions.cs`

| Key | Type | Default | Description | Consumed By |
|-----|------|---------|-------------|-------------|
| `AppName` | string | `"YangOne Framework"` | App display name | UI |
| `SiteUrl` | string | `"https://localhost:7189"` | Base site URL | SEO, sitemap, links |
| `IsInstalled` | bool | `true` | If false, shows installer page | `WebServiceRegistrar`, `PermissionService` |
| `DbProvider` | string | `"SQLServer"` | Dialect: `SQLServer`, `MySQL`, `PostgreSQL`, `SQLite` | DB factory selection |
| `Framework` | string | `""` | Framework version string | Metadata |
| `Version` | string | `"1.0.0"` | App version | UI display |
| `EnableLogging` | bool | `true` | Enable file-based logging | `YangOneAppConfig` |
| `EnableDbLogging` | bool | `false` | Enable DB query logging | `DefaultDbLoggerSetting` |
| `Theme` | string | `"Default"` | Front-end theme folder | Theme resolver |
| `AdminTheme` | string | `"Admin"` | Admin theme folder | Theme resolver |
| `FacebookAppId` | string | `""` | Facebook OAuth App ID | Social login (commented out) |
| `RequireConfirmedEmail` | bool | `false` | Require email confirmation to login | `Identity/ServiceRegistrar` |
| `PasswordLength` | int | `8` | Min password length | `Identity/ServiceRegistrar` |
| `RequireNonAlphanumeric` | bool | `false` | Require non-alphanumeric in password | `Identity/ServiceRegistrar` |
| `RequireUppercase` | bool | `false` | Require uppercase in password | `Identity/ServiceRegistrar` |
| `DataProtectionKeyPath` | string | `"app_data"` | Data Protection key directory | `Identity/ServiceRegistrar` |
| `CookieDomain` | string | `"localhost"` | Auth cookie domain | `Identity/ServiceRegistrar`, `YOWebExtensions` |
| `CookieIdentity` | string | `"kachuwa.id"` | Identity cookie name | `Identity/ServiceRegistrar` |
| `ApiTokenEndPoint` | string | `"https://localhost:7189/connect/token"` | OIDC token endpoint | `TokenGenerator` (commented out) |
| `ApiEndPoint` | string | `"https://localhost:7189"` | Base API URL | Metadata |
| `TokenAuthority` | string | `"https://localhost:7189"` | JWT authority (non-dev) | `Startup.cs` JWT Bearer |
| `SMTMConfig:Host` | string | `""` | SMTP host | `SmtpEmailSender` |
| `SMTMConfig:UserName` | string | `""` | SMTP username | `SmtpEmailSender` |
| `SMTMConfig:Password` | string | `""` | SMTP password | `SmtpEmailSender` |
| `SMTMConfig:UseSSL` | bool | `true` | SMTP SSL flag | `SmtpEmailSender` |
| `SMTMConfig:Port` | int | `587` | SMTP port | `SmtpEmailSender` |
| `JobConnection` | string | `"JobConnection"` | Job scheduler connection string name | Job scheduler |

**POCOs:** `YangOneAppConfig` (`YangOne.Core/Configuration/YangOneAppConfig.cs`), `SMTMConfig` (`YangOne.Core/Configuration/SMTMConfig.cs`)

---

### `DapperIdentityCryptography` section

```json
{
  "DapperIdentityCryptography": {
    "Key": "FrKE/VtQ5pfNhGYVnyf65Sa6j4h6ion3ItkAnqLsnBE=",
    "IV": "Ig/YU0tgUqI1u2VzWH0plQ=="
  }
}
```

**Binding:** `services.Configure<AESKeys>(configuration.GetSection("DapperIdentityCryptography"))` in `Identity/Extensions/ServiceCollectionExtensions.cs`

| Key | Type | Description | Consumed By |
|-----|------|-------------|-------------|
| `Key` | string (base64) | AES encryption key for Identity data | `EncryptionHelper` via `IOptions<AESKeys>` |
| `IV` | string (base64) | AES initialization vector | `EncryptionHelper` via `IOptions<AESKeys>` |

**POCO:** `AESKeys` (`YangOne.Identity/Cryptography/AESKeys.cs`)

---

## 4. `securityconfig.json`

**Path:** `src/App/YOApp/App_Data/securityconfig.json`

```json
{
  "RequireOTP": true,
  "RequireDeviceVerification": true,
  "SendOTPFromEmail": true,
  "SendOTPFromSMS": true,
  "OTPExpiryTimeInMinutes": 5,
  "RequireConfirmedEmail": true,
  "PasswordLength": 8,
  "RequireNonAlphanumeric": true,
  "RequireUppercase": true
}
```

Read/written directly by `AppBasicSecurityService`.

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `RequireOTP` | bool | `true` | Require OTP for login |
| `RequireDeviceVerification` | bool | `true` | Require device verification |
| `SendOTPFromEmail` | bool | `true` | Allow OTP via email |
| `SendOTPFromSMS` | bool | `true` | Allow OTP via SMS |
| `OTPExpiryTimeInMinutes` | int | `5` | OTP expiration in minutes |
| `RequireConfirmedEmail` | bool | `true` | Require confirmed email |
| `PasswordLength` | int | `8` | Min password length |
| `RequireNonAlphanumeric` | bool | `true` | Require non-alphanumeric chars |
| `RequireUppercase` | bool | `true` | Require uppercase chars |

**POCO:** `AppBasicSecurity` (`YangOne.Web/Security/Class1.cs`)

---

## 5. `cspconfig.json`

**Path:** `src/App/YOApp/App_Data/cspconfig.json`

```json
{
  "default-src": ["'self'"],
  "script-src": ["'self'", "'unsafe-inline'"],
  "style-src": ["'self'", "'unsafe-inline'", "https://fonts.googleapis.com"],
  "img-src": ["'self'", "data:"],
  "object-src": ["'none'"],
  "media-src": ["'none'"],
  "connect-src": ["'self'"],
  "form-action": ["'self'"],
  "frame-src": ["'self'"],
  "embed-src": ["'self'"],
  "font-src": ["'self'", "https://fonts.googleapis.com", "https://fonts.gstatic.com"],
  "base-uri": ["'self'"],
  "SupportNonce": true
}
```

Loaded by `CspConfigLoader.Load()` in `YangOneCoreExtensions.cs`. Applied via `SecurityHeadersMiddleware`.

| Key | Type | Description |
|-----|------|-------------|
| `{directive-name}` | string[] | CSP directive values (e.g., `default-src`, `script-src`, etc.) |
| `SupportNonce` | bool | Auto-inject nonces into `script-src` and `style-src` |

**POCO:** `CspConfig` (`YangOne.Core/Security/CspConfig.cs`) — `Dictionary<string, List<string>> Directives` + `bool SupportNonce`

---

## 6. `optimizationconfig.json`

**Path:** `src/App/YOApp/App_Data/optimizationconfig.json`

```json
{
  "EnableJsMinification": false,
  "EnableCSSMinification": false,
  "CachingDirectory": false,
  "UseImageResizer": true
}
```

Read/written by `OptimizationConfigService`.

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `EnableJsMinification` | bool | `false` | Minify JS bundles |
| `EnableCSSMinification` | bool | `false` | Minify CSS bundles |
| `CachingDirectory` | bool | `false` | Cache bundle output to disk |
| `UseImageResizer` | bool | `true` | Enable image resizing middleware |

**POCO:** `OptimizationConfig` (`YangOne.Web/Optimizer/IYoBundler.cs`)

---

## 7. `fileconfig.json`

**Path:** `src/App/YOApp/App_Data/fileconfig.json`

```json
{
  "AllowFileTypes": {
    "png": ["image/png", "image/x-citrix-png", "image/x-png"],
    "jpeg": ["image/jpeg", "image/x-citrix-jpeg"],
    "jpg": ["image/jpeg"],
    "docx": ["application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
    "doc": ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
    "txt": ["text/plain"],
    "pdf": ["application/pdf"],
    "xls": ["application/vnd.ms-excel"]
  }
}
```

Read by `DefaultFileOptions` constructor. Read/written by `FileConfigService`.

| Key | Type | Description |
|-----|------|-------------|
| `AllowFileTypes` | object | Dict of extension → allowed MIME types |
| `AllowFileTypes:{ext}` | string[] | Allowed MIME types for extension |

**POCO:** `DefaultFileOptions` (`YangOne.Core/Storage/DefaultFileOptions.cs`)

---

## 8. `apiconfig.json`

**Path:** `src/App/YOApp/App_Data/apiconfig.json`

```json
{
  "APIConfig": {
    "UseEncryption": false,
    "UseObfusication": true
  }
}
```

Read/written by `ApiConfigService`.

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `APIConfig:UseEncryption` | bool | `false` | Encrypt API responses |
| `APIConfig:UseObfusication` | bool | `true` | Obfuscate API URLs/params |

**POCO:** `ApiConfig` (`YangOne.Web/Security/API/Class1.cs`)

---

## 9. `smidge.json`

**Path:** `src/Core/YangOne.Web/smidge.json`

```json
{
  "smidge": {
    "dataFolder": "smidge",
    "version": "108"
  }
}
```

Bound via `configuration.GetSection("smidge")` in `YOWebExtensions.cs`.

| Key | Type | Description |
|-----|------|-------------|
| `smidge:dataFolder` | string | Smidge cache directory (relative to content root) |
| `smidge:version` | string | Cache-busting version (increment to invalidate) |

---

## 10. Azure Blob Storage Config

Read from `IConfiguration` (not a specific file).

| Key | Description | Consumed By |
|-----|-------------|-------------|
| `Azure:StorageConnectionString` | Azure Blob Storage connection string | `YOBlobStorageProvider` |

---

## Summary Table

| File | Path | Purpose | Optional |
|------|------|---------|----------|
| `appsettings.json` | `YOApp/` | Standard ASP.NET config (DB, JWT, rate limit) | No |
| `appsettings.{env}.json` | `YOApp/` | Environment overrides | Yes |
| `yoconfig.json` | `YOApp/App_Data/` | App settings, SMTP, Identity crypto, framework config | No |
| `securityconfig.json` | `YOApp/App_Data/` | OTP, device verification, password policy | No |
| `cspconfig.json` | `YOApp/App_Data/` | Content Security Policy directives | No |
| `optimizationconfig.json` | `YOApp/App_Data/` | JS/CSS minification, image resizer | No |
| `fileconfig.json` | `YOApp/App_Data/` | Allowed file upload types | No |
| `apiconfig.json` | `YOApp/App_Data/` | API encryption/obfuscation | No |
| `smidge.json` | `YangOne.Web/` | CSS/JS bundling cache config | No (embedded resource) |
| `locale_resources-*.json` | `YOApp/Locale/` | Localization strings | Yes |
| `dashboard/*.json` | `YOApp/App_Data/dashboard/` | Role dashboard widget config | Yes (empty files) |
