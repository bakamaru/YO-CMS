# YO-Framework (YandOne / Kachuwa) — AI Agent Guide

## Project Overview

Modular ASP.NET Core 10 CMS/framework with plugin architecture. Uses Dapper (no EF Core), custom SQL generation from POCO attributes, and auto-discovery of modules via DI scanners. Author: Binod Tamang.

---

## 1. Technology Stack

| Layer | Technology |
|-------|-----------|
| Language | C# 13, .NET 10 |
| Host | ASP.NET Core 10 (RCL + MVC) |
| ORM | Dapper + custom `QueryBuilder` (SQL gen from attributes) |
| Databases | SQL Server, MySQL, PostgreSQL, SQLite |
| DI | Built-in `IServiceCollection` + Scrutor assembly scanning |
| Auth | ASP.NET Core Identity + OpenIddict (JWT + Cookies) |
| Caching | In-memory (`YOMemoryCache`) + Redis provider |
| Real-time | SignalR (YangOne.RTC.Core) |
| Push | Firebase Cloud Messaging (FCM) |
| Email | SMTP, SendGrid, MailJet |
| SMS | Azure SMS, custom providers |
| File Storage | Local, Azure Blob |
| Bundling | Smidge (CSS/JS bundling) |
| Templating | Mustache |
| Testing | xUnit (YOApp.Test, scaffold only) |

---

## 2. Solution Structure

```
YO-Framework-Dev/
├── src/
│   ├── YO-Framework.sln
│   ├── Dockerfile
│   ├── Directory.Build.props
│   ├── App/YOApp/                    # Host web application
│   ├── Core/
│   │   ├── YangOne.Core/             # Interfaces, DI, extensions, security, caching
│   │   ├── YangOne.Data/             # Database access (Dapper + QueryBuilder)
│   │   ├── YangOne.Web/              # Web framework (controllers, models, services, tags)
│   │   ├── YangOne.Identity/         # Custom Identity stores & services
│   │   ├── YangOne.OTP/              # TOTP/HOTP generation
│   │   ├── YangOne.RTC.Core/         # SignalR real-time communication
│   │   ├── YangOne.FCM/              # (placeholder stub)
│   │   └── YangOne.Extenstions/      # (placeholder stub)
│   ├── Modules/                      # Pluggable feature modules (RCL)
│   │   ├── YandOne.Admin/            # Admin panel
│   │   ├── YangOne.BlogEngine/       # Blog engine
│   │   ├── YangOne.Identity.Web/     # Identity UI
│   │   ├── YangOne.IdentityServer/   # OpenIddict/IdentityServer UI
│   │   ├── YangOne.FCM/              # Firebase Cloud Messaging
│   │   ├── YangOne.Localization/     # Localization management
│   │   └── YangOne.Messaging/        # Internal messaging
│   ├── Extensions/
│   │   ├── YangOne.AntiVirus/        # (placeholder)
│   │   ├── YangOne.SMS.Azure/        # Azure SMS provider
│   │   └── YangOne.Storage.AzureBlob/ # Azure Blob storage
│   └── Test/YOApp.Test/              # Test project (scaffold only)
└── tools/                            # Empty
```

---

## 3. Coding Conventions

### Naming
- **PascalCase** for classes, methods, properties, namespaces, files
- **Interfaces**: `I` prefix (`IBlogService`, `IRepository`)
- **Async methods**: `Async` suffix (`GetAsync`, `SaveAsync`)
- **Private fields**: `_camelCase` (e.g., `_blogService`)
- **Files**: match class name exactly, organized in folders by concern

### Project Structure per Module
```
ModuleName/
├── {Module}ServiceRegistrar.cs       # IServiceRegistrar impl
├── {Module}.csproj
├── Model/                            # Domain models / entities
├── Service/                          # Interfaces + implementations
├── API/ or Api/                      # Web API controllers
├── Controllers/                      # MVC controllers
├── ViewModel/                        # DTOs / ViewModels
├── Areas/{Area}/Views/               # Razor views
└── TagHelpers/                       # (optional) custom tag helpers
```

### Attributes & Data Annotations
```csharp
[Table("TableName")]     // Maps POCO to table
[Key]                    // Primary key
[Required]               // Required field (for insert SQL)
[Column("col")]          // Custom column name
[NotMapped]              // Excluded from SQL
[IgnoreInsert]           // Excluded from INSERT
[IgnoreUpdate]           // Excluded from UPDATE
[IgnoreAll]              // Excluded from ALL SQL ops
[IgnoreSelect]           // Excluded from SELECT
[ReadOnly]               // Read-only property
[Editable(true/false)]   // Editable flag
[AutoFill(...)]          // Auto-populates (CurrentDate, CurrentUserId)
[FriendlyName("...")]    // Display name
[Dependent]              // Dependent property
```

### AutoFill Pattern
```csharp
[AutoFill(AutoFillProperty.CurrentDate)]   // Auto-fills DateTime.Now
[AutoFill(AutoFillProperty.CurrentUserId)] // Auto-fills current user ID
[AutoFill(true)]                            // Auto-fills (for bool defaults)
```

### Base Entity (`BaseProps`)
Every domain model inherits from `BaseProps`:
```csharp
public class BaseProps
{
    public bool IsActive { get; set; }
    [JsonIgnore] public bool IsDeleted { get; set; }
    [JsonIgnore][IgnoreUpdate][AutoFill(AutoFillProperty.CurrentDate)] public DateTime AddedOn { get; set; }
    [JsonIgnore][IgnoreUpdate][AutoFill(AutoFillProperty.CurrentUserId)] public long AddedBy { get; set; }
    [JsonIgnore] public long DeletedBy { get; set; }
    [IgnoreInsert][JsonIgnore] public DateTime UpdatedOn { get; set; }
    [IgnoreInsert][JsonIgnore] public DateTime DeletedOn { get; set; }
    [IgnoreInsert][JsonIgnore] public long UpdatedBy { get; set; }
    [IgnoreAll] public int RowTotal { get; set; }
}
```

---

## 4. Module System & DI Auto-Discovery

### Service Registration
Each module has a registrar implementing `IServiceRegistrar` (in `YangOne.Core/DI/`):
```csharp
public interface IServiceRegistrar
{
    void Register(IServiceCollection serviceCollection, IConfiguration configuration);
    void Update(IServiceCollection serviceCollection);
}
```

**`Bootstrapper`** scans all non-Microsoft/System assemblies for `IServiceRegistrar` implementations and calls `Register()` then `Update()`.

### Module Registrar Pattern
```csharp
public class BlogServiceRegistrar : IServiceRegistrar
{
    public void Update(IServiceCollection serviceCollection) { }
    public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddTransient<IBlogService, BlogService>();
        var assp = new EmbeddedFileProvider(typeof(BlogServiceRegistrar).GetTypeInfo().Assembly);
        serviceCollection.Configure<MvcRazorRuntimeCompilationOptions>(opts =>
        {
            opts.FileProviders.Add(assp);
        });
    }
}
```

### App Builder Registration
Modules can also implement `IAppBuilderRegistrar` for middleware configuration:
```csharp
public interface IAppBuilderRegistrar
{
    void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IWebHostEnvironment env);
}
```

### Module Interface
```csharp
public interface IModule
{
    string Name { get; }
    string Version { get; }
    string Author { get; }
    Assembly Assembly { get; }
    bool IsInstalled { get; }
}
```

---

## 5. Database Access Pattern

### No Entity Framework — Dapper + Custom QueryBuilder

The framework does NOT use EF Core. It uses **Dapper** with a custom **QueryBuilder** that generates SQL from POCO attributes.

### IDatabaseFactory
```csharp
public interface IDatabaseFactory
{
    IDbConnection GetConnection();
    QueryBuilder GetQueryBuilder();
    Dialect GetDialect();
}
```

Configured via:
```csharp
DatabaseFactories.SetFactory(Dialect.SQLServer, serviceProvider);
```

Accessed via singleton:
```csharp
DbFactoryProvider.GetFactory();
```

### CrudService<T> (no interface)
The direct data access class. Used everywhere:
```csharp
public class CrudService<T>
{
    // Query
    T Query(string sql, object param = null)
    IEnumerable<T> QueryList(string sql, object param = null)
    
    // Get
    T Get(object id)
    T Get(string condition, object parameters)
    IEnumerable<T> GetList(object whereConditions)
    IEnumerable<T> GetListPaged(int pageNumber, int rowsPerPage, string conditions, string orderby)
    
    // Insert
    int? Insert(object entityToInsert)
    TKey Insert<TKey>(object entityToInsert)
    
    // Update
    int Update(object entityToUpdate)
    
    // Delete
    int Delete(T entityToDelete)
    int Delete(object id)
    int Delete(int[] ids)
    
    // Soft delete
    Task UpdateAsDeleted(object id)
    Task<int> UpdateStatusAsync(object id, object status)
    Task<int> UpdateStatusByColumnName(object id, object status, string columnName)
    
    // Count
    Task<int> RecordCountAsync(string conditions = "", object parameters = null)
}
```

### CRUDHelper (Dapper extension methods)
Static extension methods on `IDbConnection` in `CRUDHelper.cs`:
- `Get<T>()`, `GetList<T>()`, `GetListPaged<T>()`, `Insert<T>()`, `Update()`, `Delete<T>()`, `RecordCount<T>()`

### QueryBuilder
Generates SQL from POCO attributes using reflection. Supports:
- `[Table]` → table name
- `[Key]` → identity/PK
- `[IgnoreInsert]`, `[IgnoreUpdate]`, `[IgnoreAll]` → column exclusion
- `[AutoFill]` → auto-value injection before SQL execution
- Dialect-specific SQL generation (MsSQL, MySQL, PostgreSQL, SQLite)

### ModelService (generic facade)
Sits above `CrudService<T>` for quick CRUD without direct dependency:
```csharp
public class ModelService
{
    Task<T> GetAsync<T>(object id)
    Task<IEnumerable<T>> GetListAsync<T>()
    Task<IEnumerable<T>> GetListPagedAsync<T>(...)
    Task<int?> InsertAsync<T>(object entityToInsert)
    Task<bool> UpdateAsync<T>(object entityToUpdate)
    Task<int> DeleteAsync<T>(object entityToDelete)
    Task<int> RecordCountAsync<T>(...)
}
```

### Raw SQL with Dapper
Services commonly use `DbFactoryProvider` directly:
```csharp
var dbFactory = DbFactoryProvider.GetFactory();
using var db = dbFactory.GetConnection();
var result = await db.QueryAsync<PostWithAuthor>(sql, new { page, limit });
```

### Stored Procedures
Supported via Dapper:
```csharp
var result = await db.QueryAsync<Post>("usp_GetRecentPosts", 
    new { Offset = offset, Limit = limit }, 
    commandType: CommandType.StoredProcedure);
```

### Supported Dialects
| Dialect | Factory | QueryBuilder | Template |
|---------|---------|-------------|----------|
| SQL Server | `MsSQLFactory` | `MsSqlQueryBuilder` | `MsSQLTemplate` |
| MySQL | `MySQLFactory` | `MySqlQueryBuilder` | `MySQLTemplate` |
| PostgreSQL | `NpgSqlFactory` | `NpgSqlQueryBuilder` | `NpgSQLTemplate` |
| SQLite | `SQLLiteFactory` | (shared) | `SQLLiteTemplate` |

---

## 6. API Creation Pattern

### API Response
```csharp
public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public string[] Errors { get; set; }
}
```

### Base API Controller
```csharp
public class BaseApiController : Controller
{
    IActionResult SuccessResponse(string msg, object? data)  // 200 OK
    IActionResult ErrorResponse(int statusCode, string msg)  // Error
    IActionResult ExceptionResponse(Exception ex, object? data = null) // 500
    IActionResult ValidationResponse(List<string>? errors)  // 400 (code 600)
    IActionResult NotAuthorizedResponse(string? message = null) // 401
    IActionResult HttpResponse(int statusCode, string msg, object? data)
}
```

### API Controller Pattern
```csharp
[Route("api/v1/[controller]")]
public class XxxApiController : BaseApiController
{
    private readonly IXxxService _xxxService;
    public ILogger Logger { get; }

    public XxxApiController(IXxxService xxxService, ILogger logger)
    {
        _xxxService = xxxService;
        Logger = logger;
    }

    [HttpGet("all")]
    public async Task<dynamic> GetAll(int offset = 1, int limit = 10, string query = "")
    {
        try
        {
            var result = await _xxxService.GetListAsync(offset, limit);
            return SuccessResponse("success", result);
        }
        catch (Exception ex)
        {
            return ExceptionResponse(ex);
        }
    }

    [HttpPost("save")]
    public async Task<dynamic> Save([FromBody] SaveXxxViewModel model)
    {
        // validation logic
        var result = await _xxxService.SaveAsync(model);
        return HttpResponse(200, "Saved successfully", result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<dynamic> Delete(int id) { ... }
}
```

### Controller Route Convention
- API: `[Route("api/v1/{resource}")]` — actions named as `GetAll`, `Save`, `Delete`, `GetDetail`
- MVC Admin: Areas with `[Area("Admin")]`, routes like `/Admin/{controller}/{action}`
- Module Controllers extend `BaseApiController` or `BaseController`

---

## 7. Service Pattern

### Interface Contract
```csharp
public interface IXxxService
{
    // Expose CrudService<T> properties for direct data access
    CrudService<Entity> EntityCrudService { get; set; }

    // Business methods
    Task<ReturnType> OperationAsync(params);
}
```

### Implementation
```csharp
public class XxxService : IXxxService
{
    private readonly ILogger _logger;
    
    public CrudService<Entity> EntityCrudService { get; set; } = new CrudService<Entity>();

    public XxxService(ILogger logger, /* other deps */)
    {
        _logger = logger;
    }

    public async Task<ReturnType> OperationAsync(params)
    {
        // Use CrudService properties or DbFactoryProvider for custom SQL
        var dbFactory = DbFactoryProvider.GetFactory();
        using var db = dbFactory.GetConnection();
        // ... business logic
    }
}
```

### Key Service Patterns
- Services **expose `CrudService<T>` as public properties** on both interface and implementation
- Constructor injection for `ILogger` and other services
- `DbFactoryProvider.GetFactory()` for raw SQL / transactions
- No base service class — plain POCO classes

---

## 8. DTO / ViewModel Pattern

DTOs are `ViewModel` classes that:
- May inherit from domain models (e.g., `PostWithAuthor : Post`)
- May inherit from `SEO` base for SEO metadata
- Use `[Required]` for validation
- Use `[IgnoreAll]` for properties not mapped to DB (e.g., `IFormFile` uploads)
- Placed in a `ViewModel/` folder per module

---

## 9. Core Libraries Breakdown

### YangOne.Core (netstandard2.1)
| Area | Files | Purpose |
|------|-------|---------|
| `DI/` | `IServiceRegistrar`, `Bootstrapper`, `IAppBuilderRegistrar`, `KachuwaAppBuilder` | Module discovery & DI |
| `Extensions/` | `StringExtensions`, `ObjectExtensions`, `PagingExtensions`, `ReflectionExtensions`, `DateTimeExtensions`, `AssemblyExensions` | Utility extensions |
| `Caching/` | `ICacheService`, `ICacheProvider`, `CacheAttribute`, `CacheMiddleware`, `CacheSetting` | Caching abstraction |
| `Configuration/` | `YangOneAppConfig`, `ConfigChangeEvent`, `SMTMConfig` | App configuration |
| `Security/` | CSP builder, security headers, `CspNonceService`, `SecurityHeadersMiddleware` | Web security |
| `Storage/` | `IStorageProvider`, `LocalStorageProvider`, chunked file upload | File storage |
| `Log/` | `ILogger` (custom), `ILogProvider`, `DefaultLogProvider`, `FileBaseLogger` | Logging |
| `Localization/` | `ILocaleResourceProvider`, `DataAnnotationLocalizer`, `SystemCultureProvider` | i18n |
| `Job/` | `IYangOneScheduler`, `IYangOneJobRunner`, `YangOneJob` | Job scheduling |
| `Messaging/` | `IYangOnePubSub`, `KachuwaPubSub`, `Subscription` | Pub/sub |
| `Plugin/` | `IPlugin`, `PluginBootStrapper`, `PluginViewProvider` | Plugin system |
| `Web/` | View location expanders, `IViewRenderService`, theme system, user-agent parser | Web utilities |
| `State/` | `SessionExtensions` | Session |
| `IO/` | `FileHelper` | File helpers |
| `Utility/` | `Bits`, `Converter`, `IPAddressRange` | Utilities |
| `AntiVirus/` | `IVirusScanner`, `ScanResult` | AV abstraction |

### YangOne.Data (net10.0)
| File | Purpose |
|------|---------|
| `IDatabaseFactory.cs` | DB connection factory interface |
| `DatabaseFactories.cs` | Static factory registry (`SetFactory`) |
| `DbFactoryProvider.cs` | Singleton accessor for current factory |
| `QueryBuilder.cs` | SQL generator from POCO attributes (951 lines) |
| `ISQLTemplate.cs` | Dialect-specific SQL templates |
| `Crud/CrudService.cs` | Generic CRUD wrapper (501 lines) |
| `Crud/CRUDHelper.cs` | Dapper extension methods |
| `Crud/Dialect.cs` | Enum: SQLServer, MySQL, PostgreSQL, SQLite |
| `Crud/Attribute/*` | 14 attribute classes for POCO mapping |
| `MsSQL/*`, `MySQL/*`, `PostgreSQL/*`, `SQLLite/*` | Dialect implementations |
| `QueryRequest.cs` | Generic query request model |

### YangOne.Web (net10.0, RCL)
| Area | Purpose |
|------|---------|
| `API/` | `BaseApiController`, `ApiResponse<T>`, file upload, chunk upload, rate limiting |
| `Controller/` | `BaseController`, `InstallerController`, `SEOController`, `LogController` |
| `Service/` | Audit, Country, Email (SMTP/SendGrid/MailJet), Export-Import, File, Permission, Setting, SMS, HtmlComponent |
| `Model/` | Audit, Country, Email, Permission, Setting, SMS, Restriction, Route, Response |
| `Module/` | Module manager, service, container, view provider, script runner |
| `TagHelpers/` | Pager, Pagination, Module, Plugin, Theme, Markdown, Nonce, SEO, Token, Component |
| `Security/` | Authorization policy provider, page permission handler, CSP, API encryption/obfuscation |
| `SEO/` | Meta tags, sitemap builder (news/video/image), SEO service |
| `Caching/` | Default cache + Redis cache service |
| `Middleware/` | Custom header, HTTPS redirect, header log, image resizer, theme resource, web token |
| `Layout/` | Bootstrap grid system, layout renderer, content management |
| `Localization/` | Locale service, resource provider, resource builder |
| `Notification/` | Notification bar, temp data wrapper |
| `Widget/` | Widget service, renderer, dashboard widget manager |
| `YOForm/` | Dynamic form builder |
| `YOGrid/` | HTML grid builder |
| `Templating/` | Mustache template engine, HTML template components |
| `Installer/` | Installation wizard models |
| `Printing/` | POS printer support (ECSPOS) |
| `Page/` | Page management service, file provider, permissions |
| `Plugin/` | Plugin service |
| `Image/` | Image utilities |
| `Optimizer/` | Smidge bundler integration |

### YangOne.Identity (net10.0)
Custom ASP.NET Core Identity implementation with Dapper:
- **Stores**: `KachuwaUserStore`, `KachuwaRoleStore` (IUserStore, IRoleStore)
- **Models**: `AppUser`, `IdentityUser`, `IdentityRole`, `IdentityUserClaim`, `UserDevice`, `UserLoginHistory`
- **Services**: `AppUserService`, `IdentityUserService`, `IdentityRoleService`, `IUserService`, `IUserDeviceService`, `ILoginHistoryService`
- **DTOs**: `NewUser`, `EditUser`, `UserStatus`, `UserRoles`, `AppUserRegisterModel`, `BasicUserDetails`, `DeviceVerificationStatus`, `UserDeviceStatus`
- **Claim Factory**: `YangOneClaimsPrincipalFactory`
- **Cryptography**: `EncryptionHelper`, `AESKeys`
- **Constants**: `YORoleNames`, `YORoles`, `ApplicationClaim`

### YangOne.OTP (net10.0)
TOTP/HOTP implementation:
- `Totp`, `Hotp`, `Otp` base, `KeyGeneration`, `KeyUrl`
- `IOTPService`, `OTPService`
- Models: `OTPSetting`, `UserOTP`, `UserSecret`
- NTP time correction

### YangOne.RTC.Core (net10.0)
SignalR real-time:
- Hubs: `BaseHub`, `YangOneUserHub`
- Services: `RTCNotificationService`, `RTCPersistentConnectionManager`, `IRTCConnectionManager`, `IRTCUserService`
- Models: `RTCUser`, `RtcUserStatus`

---

## 10. Existing Modules

### YandOne.Admin
Auto-discovered admin panel module.
- **Registrar**: `AdminModuleRegistrar`
- **API Controllers** (10): `HtmlComponentAPIController`, `LocalizationApiController`, `MediaLibraryApiController`, `MenuAPIController`, `MiscAPIController`, `ModuleApiController`, `PermissionApiController`, `SeoApiController`, `SettingAPIController`
- **MVC Controllers** (19): Audit, Dashboard, EmailServiceProvider, HtmlBuilder, LiveEditor, Localization, MediaLibrary, Menu, MenuType, Module, Page, PaymentGateway, Permission, Plugins, Role, Security, SEO, Setting, Theme, User
- **Views**: Full admin UI (Audit, Dashboard, Email, Localization, Menu, Module, Page, Permission, Plugins, Role, SEO, Setting, Security, Theme, LiveEditor)
- **TagHelpers**: `PermissionTagHelper`, `MediaLibraryTagHelper`, `SettingTagHelper`

### YangOne.BlogEngine
Blog engine module.
- **Registrar**: `BlogServiceRegistrar`
- **Module**: `BlogModule : IModule` (Name: "BlogEngine", v1.0.0.0, Author: Binod Tamang)
- **Models**: `Post` ([Table("Post")]), `PostCategory`, `PostTag`, `PostSetting`, `Blog`, `BlogCategory`, `BlogComment`
- **Services**: `IBlogService/BlogService`, `IPostService/PostService`, `IBlogCategoryService/BlogCategoryService`, `ICommentService/CommentService`, `IPostCategoryService/PostCategoryService`
- **API Controllers**: `BlogApiController` (route: `api/v1/post`), `CategoryApiController`, `PostApiController`, `CommentApiController`
- **ViewModels**: `PostWithAuthor : Post`, `SavePostViewModel : SEO`, `PostViewDetailModel`, `ArchiveViewModel`
- **Views**: Post, Category, Comment management

### YangOne.FCM
Firebase Cloud Messaging module.
- **Registrar**: `ServiceRegistrar`
- **Models**: `UserFCMDevice` ([Table("UserFCMDevice")]), `FCMSetting`
- **Services**: `IFCMService/FCMService` (Google OAuth, FCM HTTP v1), `IFCMDeviceService/FCMDeviceService`
- **API Controllers**: `FCMTestApiController` (route: `api/v1/fcm`), `FCMApiController`, `FCMDeviceApiController`, `FCMTopicApiController`

### YangOne.Localization
Localization management module.
- **Registrar**: `LocalizationRegistrar`
- **Models**: `Localization`, `LocalizationResource`, `Language`, `Culture`, `LocalizationQueryModel`
- **Services**: `ILocalizationService/LocalizationService`
- **API Controllers**: `LocalizationApiController`, `LanguageApiController`, `ResourceApiController`
- **Views**: Localization management UI

### YangOne.Messaging
Internal messaging module.
- **Registrar**: `MessagingRegistrar`
- **Models**: `Message`, `MessageAttachment`, `Mailbox`, `MessageQueryModel`
- **Services**: `IMessageService/MessageService`, `IMailboxService/MailboxService`
- **API Controllers**: `MessageApiController`, `MailboxApiController`
- **Views**: Messaging UI

### YangOne.Identity.Web
Identity UI module (embedded Razor views for login/register/manage).
- **Registrar**: `IdentityWebServiceRegistrar`
- **Controllers**: `AccountController`, `ManageController`
- **ViewModels**: Full Identity UI view model set
- **Views**: Account (Login, Register, ForgotPassword, ResetPassword, ConfirmEmail), Manage (Profile, ChangePassword, 2FA, PersonalData, GDPR)

### YangOne.IdentityServer
OpenIddict/IdentityServer UI module.
- **Registrar**: `IdentityServerRegistrar` (stub — Register/Update are empty)
- **Config**: `Config.cs` (IdentityServer resources, clients, scopes)
- **Controllers**: `AccountController`, `ConsentController`, `DeviceController`, `GrantsController`, `HomeController`
- **Services**: `ExtendedProfileService`, `LoginService`, `ConsentService`
- **Views**: Full IdentityServer UI

---

## 11. Host Application (YOApp) Startup

`Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
// config: appsettings.json + app_data/yoconfig.json + apiconfig.json + securityconfig.json + ...
builder.Services.TryAddSingleton<ConfigChangeEvent, YangOneConfigChangeEvent>();
Startup.ConfigureServices(builder.Services, config, env);
var app = builder.Build();
Startup.Configure(app, serviceProvider: builder.Services.BuildServiceProvider(), env);
app.MapOpenApi();
app.MapScalarApiReference();
app.Run();
```

`Startup.cs` configures:
1. `DatabaseFactories.SetFactory(Dialect.SQLServer, ...)` — DB setup
2. `services.RegisterKachuwaCoreServices(...)` — core framework services
3. OpenIddict (server + token validation + aspnet)
4. Rate limiting
5. Authentication (Cookies + JWT Bearer + OpenIddict)
6. Authorization policies
7. Session + SignalR
8. MVC + Razor + Areas
9. App configuration files: `yoconfig.json`, `apiconfig.json`, `security.json`, `cspconfig.json`, `fileconfig.json`, `optimizationconfig.json`, `securityconfig.json`

---

## 12. Configuration Files

| File | Location | Purpose |
|------|----------|---------|
| `appsettings.json` | `YOApp/` | Standard ASP.NET config |
| `yoconfig.json` | `YOApp/App_Data/` | YO framework config (DB, app settings) |
| `apiconfig.json` | `YOApp/App_Data/` | API-specific config |
| `security.json` | `YOApp/App_Data/` | Security config |
| `cspconfig.json` | `YOApp/App_Data/` | Content Security Policy |
| `fileconfig.json` | `YOApp/App_Data/` | File storage config |
| `optimizationconfig.json` | `YOApp/App_Data/` | Bundling/optimization |
| `securityconfig.json` | `YOApp/App_Data/` | Additional security |
| `locale_resources-en-us.json` | `YOApp/Locale/` | Localization resources |

---

## 13. Database Schema (SQL Server)

SQL install scripts at `YOApp/db/1.0.0/mssql/install/`:
1. `1.install.sql` — Core schema
2. `2.data.sql` — Seed data
3. `2.menudata.sql` — Menu data
4. `3.kachuwareactdata.sql` — React data
5. `4.extra.sql` — Extras
6. `5.OTPSchema.sql` — OTP tables
7. `6.UserDevice.sql` — User device tables
8. `7.permision.sql` — Permissions
9. `8.openidschema.sql` — OpenIddict schema
10. `8.22.timezone.sql` — Timezone data
11. `banner.sql` — Banner tables
12. `htmlbuilder.sql` — HTML builder tables
13. `fx/udf_Split.sql` — Split function
14. `storedprocedures/usp_RolePermission_*.sql` — Stored procs

---

## 14. Soft Delete Pattern

All entities inherit `BaseProps` with `IsDeleted` flag:
```csharp
await new CrudService<T>().UpdateAsDeleted(id);
// Sets IsDeleted = true, IsActive = false
await new CrudService<T>().UpdateStatusAsync(id, status);
// Sets custom status column
await new CrudService<T>().UpdateStatusByColumnName(id, status, "ColumnName");
// Sets specific column to status value
```

---

## 15. Testing

- Project: `src/Test/YOApp.Test/` (xUnit, net10.0)
- Only `BaseTest.cs` exists — no actual tests written yet

---

## 16. Known Gaps / Placeholders

| Item | File | Status |
|------|------|--------|
| IdentityServer Registrar | `YangOne.IdentityServer/IdentityServerRegistrar.cs` | Empty stub |
| Class1.cs | Multiple locations (`YangOne.Extenstions`, `YangOne.FCM`, `Admin/API`, `YangOne.Widget`, `YangOne.AntiVirus`) | Placeholder |
| Tests | `YOApp.Test/` | Only BaseTest.cs |
| Tools | `tools/` | Empty |
| FCM Core | `src/Core/YangOne.FCM/` | Only Class1.cs + csproj |

---

## 17. Quick Reference — Creating a New Module

1. Create RCL project in `src/Modules/YourModule/`
2. Create `YourServiceRegistrar.cs` implementing `IServiceRegistrar`
3. Create domain models in `Model/` inheriting from `BaseProps` with `[Table("...")]`
4. Create service interface + implementation in `Service/`
5. Create API controller in `API/` extending `BaseApiController`
6. (Optional) Create MVC controller + views in `Areas/Admin/Views/`
7. Register services in the registrar with `AddTransient` / `AddScoped`
8. Add embedded file provider for Razor views
9. Reference module from `YOApp.csproj`

---

## 18. Key Files Reference

| File | What it does |
|------|-------------|
| `YOApp/Program.cs` | App entry point |
| `YOApp/Startup.cs` | Full DI + middleware setup (474 lines) |
| `YangOne.Core/DI/Bootstrapper.cs` | Scans & registers all modules |
| `YangOne.Data/Crud/CrudService.cs` | Generic CRUD (501 lines) |
| `YangOne.Data/QueryBuilder.cs` | SQL generator from POCO (951 lines) |
| `YangOne.Data/Crud/CRUDHelper.cs` | Dapper extensions (399 lines) |
| `YangOne.Web/API/BaseApiController.cs` | Base for all API controllers |
| `YangOne.Web/ModelService.cs` | Generic CRUD facade |
| `YangOne.Web/Module/ModuleManager.cs` | Module lifecycle management |
| `YangOne.Web/KachuwaWebServiceRegistrar.cs` | Web framework DI setup |
| `YangOne.Identity/Stores/KachuwaUserStore.cs` | Custom Identity user store |
| `YangOne.Identity/Stores/KachuwaRoleStore.cs` | Custom Identity role store |
