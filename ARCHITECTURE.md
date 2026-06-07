# YO-Framework (YandOne / Kachuwa) — Architecture

---

## 1. High-Level Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        YOApp (Host Web App)                        │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │                     ASP.NET Core 10 Host                      │  │
│  │  Program.cs → Startup.cs → Middleware Pipeline                │  │
│  └──────────┬────────────────────────────────────────┬───────────┘  │
│             │                                        │              │
│  ┌──────────▼──────────┐              ┌──────────────▼───────────┐  │
│  │   Core Libraries     │              │     Modules (RCLs)       │  │
│  │  ┌────────────────┐  │              │  ┌──────────────────┐   │  │
│  │  │  YangOne.Core   │  │              │  │  YandOne.Admin   │   │  │
│  │  │ (netstandard2.1)│  │              │  ├──────────────────┤   │  │
│  │  ├────────────────┤  │              │  │YangOne.BlogEngine │   │  │
│  │  │  YangOne.Data  │  │              │  ├──────────────────┤   │  │
│  │  │   (net10.0)    │  │              │  │  YangOne.FCM     │   │  │
│  │  ├────────────────┤  │              │  ├──────────────────┤   │  │
│  │  │  YangOne.Web   │  │              │  │YangOne.Localizatn│   │  │
│  │  │   (net10.0)    │  │              │  ├──────────────────┤   │  │
│  │  ├────────────────┤  │              │  │ YangOne.Messaging│   │  │
│  │  │ YangOne.Identity│ │              │  ├──────────────────┤   │  │
│  │  ├────────────────┤  │              │  │YangOne.Identity.W│   │  │
│  │  │  YangOne.OTP   │  │              │  ├──────────────────┤   │  │
│  │  ├────────────────┤  │              │  │YangOne.IdentitySr│   │  │
│  │  │YangOne.RTC.Core│  │              │  └──────────────────┘   │  │
│  │  └────────────────┘  │              └──────────────────────────┘  │
│  └──────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### Key Principle: Modular Monolith with Plugin Discovery

The framework is a **modular monolith** — all modules run in the same process as Razor Class Libraries (RCLs), but they are **auto-discovered at runtime** via assembly scanning, giving the appearance and extensibility of a plugin system.

---

## 2. Startup & Request Lifecycle

### 2.1 Application Bootstrap

```
Program.cs
    │
    ├── Build configuration (appsettings.json, yoconfig.json, apiconfig.json, etc.)
    │
    ├── services.AddControllersWithViews()
    │
    ├── services.TryAddSingleton<ConfigChangeEvent, YangOneConfigChangeEvent>()
    │
    ├── Startup.ConfigureServices(services, config, env)
    │       │
    │       ├── DatabaseFactories.SetFactory(Dialect.SQLServer, sp)  ← DB dialect
    │       ├── services.RegisterKachuwaCoreServices(...)            ← Core DI
    │       │       │
    │       │       └── Bootstrapper scans all assemblies for
    │       │           IServiceRegistrar implementations
    │       │           → calls Register() then Update() on each
    │       │
    │       ├── OpenIddict (Server + Validation + ASP.NET)
    │       ├── Rate Limiting
    │       ├── Authentication (Cookie + JWT Bearer + OpenIddict)
    │       ├── Authorization (policies, handlers)
    │       ├── Session + SignalR
    │       └── MVC + Razor + Areas
    │
    ├── app = builder.Build()
    │
    ├── Startup.Configure(app, serviceProvider, env)
    │       │
    │       ├── KachuwaAppBuilder scans for IAppBuilderRegistrar
    │       │   → calls Configure() on each (middleware registration)
    │       │
    │       ├── Security headers, CSP
    │       ├── Static files
    │       ├── Authentication / Authorization
    │       ├── Session, SignalR
    │       ├── MVC routing with areas
    │       └── Endpoints
    │
    ├── app.MapOpenApi()
    ├── app.MapScalarApiReference()
    └── app.Run()
```

### 2.2 Request Lifecycle

```
HTTP Request
    │
    ├── Security Headers Middleware
    ├── CSP Middleware (Content Security Policy)
    ├── Custom Header Middleware
    ├── Force HTTPS Middleware
    ├── Header Log Middleware
    ├── Image Resizer Middleware
    ├── Theme Resource Middleware
    ├── Web Token Middleware
    │
    ├── Authentication (Cookie / JWT Bearer / OpenIddict)
    │
    ├── Authorization (Policy-based, Page Permission Handler)
    │
    ├── MVC Routing
    │       │
    │       ├── Module View Location Expander
    │       ├── Theme View Location Expander
    │       ├── Area View Location Expander
    │       │
    │       ├── API Request → BaseApiController
    │       │       ├── Action Method Calls Service
    │       │       ├── Service Uses CrudService<T> or DbFactoryProvider
    │       │       └── Returns ApiResponse<T> via SuccessResponse/HttpResponse
    │       │
    │       └── MVC Request → BaseController
    │               ├── Renders Razor View
    │               └── Views come from Module Embedded Resources or File System
    │
    └── Response
```

---

## 3. Module System Architecture

### 3.1 Discovery Mechanism

```
Application Startup
    │
    └── Bootstrapper.cs
            │
            ├── Uses DependencyContext to enumerate all loaded assemblies
            ├── Filters out Microsoft.* and System.* assemblies
            │
            ├── For each assembly:
            │       └── Find all types implementing IServiceRegistrar
            │
            ├── Instantiate each IServiceRegistrar
            │       └── Call Register(services, configuration)
            │       └── Call Update(services)
            │
            └── IAppBuilderRegistrar discovery (separate pass in middleware pipeline)
                    └── Call Configure(app, sp, env)
```

### 3.2 Module Contract

```
┌──────────────────────────────────────────────────────┐
│                   IServiceRegistrar                   │
│  Register(IServiceCollection, IConfiguration)        │
│  Update(IServiceCollection)                          │
└──────────────────────────────────────────────────────┘
         ▲                        ▲
         │                        │
┌────────┴──────────┐   ┌────────┴──────────┐
│ Module Registrar   │   │ AppBuilderRegistrar│
│ (DI Registration)  │   │ (Middleware Config)│
└───────────────────┘   └───────────────────┘
         ▲
         │
┌────────┴─────────────────────────────────────┐
│ Examples:                                     │
│  - BlogServiceRegistrar                       │
│  - AdminModuleRegistrar                       │
│  - ServiceRegistrar (FCM)                     │
│  - LocalizationRegistrar                      │
│  - MessagingRegistrar                         │
│  - IdentityWebServiceRegistrar                │
│  - IdentityServerRegistrar                    │
└──────────────────────────────────────────────┘
```

### 3.3 Module File Organization

Each module follows a consistent structure:

```
ModuleName/
├── {Module}ServiceRegistrar.cs   ← IServiceRegistrar implementation
├── {Module}.csproj               ← RCL targeting net10.0
│
├── Model/                        ← Domain entities with [Table] attributes
│   ├── Entity1.cs                ← Inherits BaseProps
│   └── Entity2.cs
│
├── Service/                      ← Business logic layer
│   ├── IEntityService.cs         ← Interface with CrudService<T> properties
│   └── EntityService.cs          ← Implementation
│
├── API/ or Api/                  ← Web API controllers
│   └── EntityApiController.cs    ← Extends BaseApiController
│
├── Controllers/                  ← MVC controllers (optional)
│   └── EntityController.cs
│
├── ViewModel/                    ← DTOs / ViewModels
│   └── EntityViewModel.cs
│
├── Areas/{Area}/Views/           ← Embedded Razor views
│   └── Entity/Index.cshtml
│
├── TagHelpers/                   ← Custom tag helpers (optional)
│   └── EntityTagHelper.cs
│
└── wwwroot/                      ← Static assets (optional)
    └── css/js
```

### 3.4 Module Registration Pattern

```csharp
// 1. Register services in DI
serviceCollection.AddTransient<IXxxService, XxxService>();

// 2. Add embedded file provider for Razor views
var embeddedAssembly = new EmbeddedFileProvider(
    typeof(XxxServiceRegistrar).GetTypeInfo().Assembly);

serviceCollection.Configure<MvcRazorRuntimeCompilationOptions>(opts =>
{
    opts.FileProviders.Add(embeddedAssembly);
});
```

---

## 4. Database Access Layer Architecture

### 4.1 Layer Stack

```
┌─────────────────────────────────────────────────┐
│                  Service Layer                    │
│  Uses CrudService<T> properties or raw Dapper   │
└──────────────────────┬──────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────┐
│               ModelService (facade)               │
│  Wraps CrudService<T> for quick CRUD access      │
└──────────────────────┬──────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────┐
│               CrudService<T>                      │
│  Generic CRUD wrapper with sync/async methods    │
│  Uses CRUDHelper extensions internally           │
└──────────────────────┬──────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────┐
│            CRUDHelper (Extension Methods)         │
│  Static methods on IDbConnection                 │
│  Uses QueryBuilder to generate SQL               │
└──────────────────────┬──────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────┐
│              QueryBuilder (Abstract)              │
│  Reflection-based SQL generation from POCO       │
│  Reads [Table], [Key], [Column], [Ignore*], etc. │
└──────┬───────────────┬───────────────┬───────────┘
       │               │               │
┌──────▼──────┐ ┌──────▼──────┐ ┌──────▼──────┐
│ MsSQLBuilder │ │MySqlBuilder │ │NpgSqlBuilder │
│ (SQL Server) │ │  (MySQL)   │ │(PostgreSQL)  │
└─────────────┘ └─────────────┘ └─────────────┘
       │
┌──────▼─────────────────────────────────────────┐
│           Dapper (IDbConnection)                 │
│  QueryAsync, ExecuteAsync, QueryMultipleAsync   │
└──────┬─────────────────────────────────────────┘
       │
┌──────▼─────────────────────────────────────────┐
│        IDatabaseFactory / DbFactoryProvider      │
│  Provides IDbConnection for configured dialect   │
└─────────────────────────────────────────────────┘
```

### 4.2 CrudService<T> Internals

```
CrudService<T>
    │
    ├── Property: IDatabaseFactory DbFactory
    │       ├── GetConnection() → IDbConnection
    │       └── GetQueryBuilder() → QueryBuilder
    │
    ├── Get Methods
    │       ├── Get(id) → SELECT with WHERE PK = @id
    │       └── GetList(conditions) → SELECT with optional WHERE
    │
    ├── Insert Methods
    │       ├── Insert(entity) → INSERT (excludes [IgnoreInsert] props)
    │       └── Insert<TKey>(entity) → INSERT + SELECT SCOPE_IDENTITY
    │
    ├── Update Methods
    │       ├── Update(entity) → UPDATE (excludes [IgnoreUpdate] props)
    │       └── UpdateAsync(entity) → async version
    │
    └── Delete Methods
            ├── Delete(id) → DELETE WHERE PK = @id
            ├── Delete(entity) → DELETE WHERE PK = @PK
            ├── Delete(ids[]) → DELETE WHERE PK IN @ids
            ├── UpdateAsDeleted(id) → SET IsDeleted=1, IsActive=0
            └── UpdateStatusAsync(id, status) → SET custom status column
```

### 4.3 SQL Generation Flow

```
QueryBuilder.BuildInsert<T>(entity)
    │
    ├── 1. Get [Table("Name")] from T → table name
    ├── 2. Get all public properties
    ├── 3. Filter out [IgnoreInsert], [IgnoreAll], [NotMapped]
    ├── 4. Process [AutoFill] attributes → inject values
    ├── 5. Build column list (use [Column] alias if present)
    ├── 6. Build parameter list (@PropertyName)
    ├── 7. Apply dialect-specific syntax (e.g., OUTPUT INSERTED.* for SQL Server)
    └── 8. Return SQL string + parameters
```

### 4.4 Attribute-Based ORM Mapping

```csharp
[Table("Post")]                              // Maps class to table
public class Post : BaseProps
{
    [Key]                                    // Primary key (auto-increment)
    public long PostId { get; set; }

    [Required]                               // Required for INSERT
    public string Title { get; set; }

    [Column("url_slug")]                     // Custom column name
    public string Url { get; set; }

    [IgnoreInsert]                           // Excluded from INSERT
    public DateTime UpdatedOn { get; set; }

    [IgnoreUpdate]                           // Excluded from UPDATE
    public long PostAuthorId { get; set; }

    [IgnoreAll]                              // Excluded from ALL SQL ops
    public IFormFile UploadedFile { get; set; }

    [AutoFill(AutoFillProperty.CurrentDate)] // Auto-populated on INSERT
    public DateTime AddedOn { get; set; }
}
```

### 4.5 DbFactoryProvider / DatabaseFactories

```
DatabaseFactories.SetFactory(Dialect.SQLServer, serviceProvider)
    │
    ├── Stores factory instance in static dictionary keyed by Dialect
    │
    └── During request:
            │
            DbFactoryProvider.GetFactory()
                │
                ├── Uses IHttpContextAccessor to resolve IServiceProvider
                ├── Resolves IDatabaseFactory from DI
                └── Returns factory for configured dialect
```

---

## 5. API Layer Architecture

### 5.1 API Request Flow

```
Client (Browser/Mobile)
    │
    ├── HTTP GET/POST/PUT/DELETE
    ├── Route: /api/v1/{controller}/{action}
    ├── Headers: Authorization (Bearer/Cookie), Content-Type
    │
    ▼
BaseApiController (base class)
    │
    ├── Inherits from Controller (not ControllerBase)
    ├── No [ApiController] attribute (manual validation)
    │
    ├── Action executes (e.g., GetAll, Save, Delete)
    │       │
    │       ├── Calls injected service method
    │       │       │
    │       │       └── Service uses CrudService<T> or raw Dapper
    │       │
    │       └── Returns response via helper:
    │               ├── SuccessResponse(msg, data)        → 200 + ApiResponse
    │               ├── HttpResponse(code, msg, data)     → Custom status
    │               ├── ErrorResponse(code, msg)          → Error response
    │               ├── ExceptionResponse(ex, data)       → 500
    │               ├── ValidationResponse(errors)        → 400 (code 600)
    │               └── NotAuthorizedResponse(msg)        → 401
    │
    ▼
ApiResponse<T> (response envelope)
    {
        "code": 200,
        "message": "success",
        "data": { ... },           // T
        "errors": []               // string[]
    }
```

### 5.2 API Controller Pattern

```
[Route("api/v1/[controller]")]
public class PostApiController : BaseApiController
{
    // 1. Constructor injection
    private readonly IPostService _postService;
    public ILogger Logger { get; }

    // 2. GET list with pagination
    [HttpGet("all")]
    public async Task<dynamic> GetAll(int offset, int limit)

    // 3. GET single by ID
    [HttpGet("detail/{id}")]
    public async Task<dynamic> GetDetail(int id)

    // 4. POST create/update
    [HttpPost("save")]
    public async Task<dynamic> Save([FromBody] ViewModel model)

    // 5. DELETE
    [HttpDelete("delete/{id}")]
    public async Task<dynamic> Delete(int id)
}
```

---

## 6. Service Layer Architecture

### 6.1 Service Pattern

```
┌──────────────────────┐
│   IXxxService        │  ← Interface
│   ┌────────────────┐ │
│   │ CrudService<T> │ │  ← Public property (direct data access)
│   │ Property1      │ │
│   │ Property2      │ │
│   ├────────────────┤ │
│   │ Operation1()   │ │  ← Business methods
│   │ Operation2()   │ │
│   └────────────────┘ │
└──────────────────────┘
           ▲
           │ implements
┌──────────┴───────────┐
│   XxxService          │  ← Implementation
│   ┌────────────────┐  │
│   │ _logger        │  │  ← Private ILogger
│   │ _otherServices │  │  ← Injected dependencies
│   ├────────────────┤  │
│   │ CrudService<T> │  │  ← Exposed as public property
│   ├────────────────┤  │
│   │ Operation1()   │  │  ← Uses CrudService or DbFactoryProvider
│   │ {              │  │
│   │   // Crud path  │  │
│   │   EntityCrud   │  │
│   │   .Get(id)     │  │
│   │                │  │
│   │   // Raw SQL   │  │
│   │   var db =     │  │
│   │   DbFactoryProv│  │
│   │   .GetFactory()│  │
│   │   .GetConnect()│  │
│   │   db.Query<>() │  │
│   │ }              │  │
│   └────────────────┘  │
└───────────────────────┘
```

### 6.2 CrudService<T> Exposure Pattern

Services expose `CrudService<T>` instances as public properties so that controllers or other services can perform basic CRUD without adding wrapper methods:

```csharp
public interface IBlogService
{
    // Direct data access properties
    CrudService<Post> PostCrudService { get; set; }
    CrudService<PostCategory> CategoryService { get; set; }
    CrudService<PostSetting> SettingCrudService { get; set; }

    // Business methods
    Task<long> SavePost(SavePostViewModel model);
    Task<IEnumerable<PostWithAuthor>> GetPostWithAuthor(int page, int limit);
}
```

---

## 7. Authentication & Authorization Architecture

### 7.1 Authentication Providers

```
Authentication (Configured in Startup.cs)
    │
    ├── Cookie Authentication
    │       └── Default scheme for browser requests
    │
    ├── JWT Bearer Token
    │       └── For API clients / mobile
    │
    └── OpenIddict (IdentityServer)
            ├── Server mode
            ├── Token validation
            └── ASP.NET Core integration
```

### 7.2 Authorization System

```
Authorization
    │
    ├── Policy-based Authorization
    │       └── Custom AuthorizationPolicyProvider
    │
    ├── Page Permission Handler
    │       └── Checks user claims against page permissions
    │
    └── Role-based (YORoles)
            ├── SuperAdmin
            └── Custom roles via IdentityRole
```

### 7.3 Custom Identity Stores (Dapper-based)

```
ASP.NET Core Identity
    │
    ├── KachuwaUserStore : IUserStore<IdentityUser>
    │       ├── IUserPasswordStore
    │       ├── IUserEmailStore
    │       ├── IUserPhoneNumberStore
    │       ├── IUserTwoFactorStore
    │       ├── IUserLockoutStore
    │       └── IUserRoleStore
    │
    └── KachuwaRoleStore : IRoleStore<IdentityRole>
```

---

## 8. Caching Architecture

```
┌──────────────────────────────────────────────┐
│              ICacheService                     │
│  GetAsync<T>(key), SetAsync(key, value, ttl)  │
└─────────────────────┬────────────────────────┘
                      │
          ┌───────────┴───────────┐
          │                       │
┌─────────▼─────────┐  ┌─────────▼─────────┐
│ DefaultCacheService │  │  RedisCacheService │
│  (In-Memory)        │  │  (Distributed)     │
│  YOMemoryCache      │  │  StackExchange.Redis│
└───────────────────┘  └───────────────────┘
```

### Cache Attributes
- `[Cache]` attribute on controller actions for automatic caching
- `CacheMiddleware` for response caching
- Configurable via `CacheSetting` and `ICacheConfig`

---

## 9. Real-Time Communication (SignalR)

```
┌──────────────────────────────────────────────┐
│              YangOne.RTC.Core                  │
│                                                │
│  Hubs:                                         │
│  ├── BaseHub (abstract base)                   │
│  └── YangOneUserHub (user-specific)            │
│                                                │
│  Services:                                     │
│  ├── RTCNotificationService                    │
│  ├── RTCPersistentConnectionManager             │
│  ├── IRTCConnectionManager                     │
│  └── IRTCUserService                          │
└──────────────────────────────────────────────┘
```

---

## 10. Configuration Architecture

### 10.1 Configuration Sources

```
IConfiguration (Builder pattern in Program.cs)
    │
    ├── appsettings.json                     ← Standard ASP.NET
    ├── appsettings.{env}.json               ← Environment-specific
    ├── App_Data/yoconfig.json               ← YO framework settings
    ├── App_Data/apiconfig.json              ← API settings
    ├── App_Data/security.json               ← Security settings
    ├── App_Data/cspconfig.json              ← Content Security Policy
    ├── App_Data/fileconfig.json             ← File storage settings
    ├── App_Data/optimizationconfig.json     ← Bundling settings
    └── App_Data/securityconfig.json         ← Additional security
```

### 10.2 Configuration Change Events

```
ConfigChangeEvent (Singleton)
    │
    ├── Fired when config files change on disk
    ├── IConfigChangeListner implementations are notified
    └── Used to reload caches, reconfigure services at runtime
```

---

## 11. File Storage Architecture

```
┌──────────────────────────────────────────────┐
│              IStorageProvider                  │
│  Save(file, path), Get(path), Delete(path)     │
└─────────────────────┬────────────────────────┘
                      │
          ┌───────────┴───────────┐
          │                       │
┌─────────▼─────────┐  ┌─────────▼──────────────┐
│ LocalStorageProvdr │  │ YOBlobStorageProvider   │
│  (Local filesystem)│  │  (Azure Blob Storage)   │
└───────────────────┘  └────────────────────────┘
```

### Chunked File Upload
- `ChunkedFileStream` - handles large file uploads in chunks
- `FileSession` / `CreateSessionParams` - session-based upload tracking
- `ChunkFileUploadController` - API endpoint for chunked uploads

---

## 12. Security Architecture

### 12.1 Content Security Policy (CSP)
- `CspConfig` loaded from `cspconfig.json`
- `CspNonceService` generates per-request nonces
- `ContentSecurityPolicyBuiilder` builds CSP headers
- `SecurityHeadersMiddleware` applies headers

### 12.2 Security Headers
```csharp
SecurityHeadersMiddleware
    ├── X-Content-Type-Options: nosniff
    ├── X-Frame-Options: DENY
    ├── X-XSS-Protection: 1; mode=block
    ├── Referrer-Policy: strict-origin-when-cross-origin
    └── Content-Security-Policy: {from config}
```

### 12.3 API Security
- `ApiAuthorizeAttribute` - custom authorization filter
- `ObfuscationMiddleware` / `ObfuscationService` - API response obfuscation
- `EncryptedJsonFormatter` / `EncryptionService` - encrypted API responses
- Rate limiting via `RateLimitOptions`

---

## 13. Templating & Rendering Architecture

### 13.1 Template Engine (Mustache)
```
MustacheTemplateEngine : ITemplateEngine
    ├── Render(template, data) → string
    └── Used for email templates, dynamic content
```

### 13.2 HTML Template Components
```
ITemplateComponent / ITemplateComponentBuilder
    ├── Invoice1TemplateDataSource
    ├── TemplateSetting / TemplateTypes
    └── TemplateDataSourceManager
```

### 13.3 View Location Expansion
```
Request → MVC
    │
    ├── ThemeLocationExpander      ← Checks active theme
    ├── ModuleViewLocationExpander ← Checks module embedded views
    ├── AreaViewLocationExpander   ← Checks area structure
    └── ViewOverrideLocationExpander ← Checks override directory
```

---

## 14. Plugin System Architecture

```
┌──────────────────────────────────────────────┐
│              IPlugin                           │
│  Name, Version, Author, Assembly, IsInstalled  │
└─────────────────────┬────────────────────────┘
                      │
┌─────────────────────▼────────────────────────┐
│            PluginBootStrapper                  │
│  Discovers plugins from assemblies            │
└─────────────────────┬────────────────────────┘
                      │
┌─────────────────────▼────────────────────────┐
│            PluginViewProvider                  │
│  Resolves views from plugin embedded resources│
└──────────────────────────────────────────────┘
```

---

## 15. Widget System Architecture

```
┌──────────────────────────────────────────────┐
│              IWidget                           │
│  Render(), Name, Zone                         │
└─────────────────────┬────────────────────────┘
                      │
┌─────────────────────▼────────────────────────┐
│            WidgetService / WidgetRenderer      │
│  Manages widget instances, renders to HTML    │
└──────────────────────────────────────────────┘
                      │
┌─────────────────────▼────────────────────────┐
│          DashboardWidgetManager                │
│  Manages dashboard-specific widgets           │
└──────────────────────────────────────────────┘
```

---

## 16. Data Flow Examples

### 16.1 API Read Flow (Get Posts)

```
Client: GET /api/v1/post/all?offset=1&limit=10
    │
    ▼
PostApiController.GetAll(offset, limit)
    │
    ├── Calls _blogService.GetPostWithAuthor(page, limit)
    │       │
    │       ├── BlogService
    │       │       │
    │       │       ├── DbFactoryProvider.GetFactory()
    │       │       ├── db.GetConnection()
    │       │       │
    │       │       └── db.QueryAsync<PostWithAuthor>(sql, new { offset, limit })
    │       │               │
    │       │               └── Dapper → SQL Server → Results
    │       │
    │       └── Returns IEnumerable<PostWithAuthor>
    │
    └── Returns SuccessResponse("success", posts)
            │
            └── JSON: { code: 200, message: "success", data: [...], errors: [] }
```

### 16.2 API Write Flow (Save Post)

```
Client: POST /api/v1/post/save (JSON body)
    │
    ▼
PostApiController.Save(model)
    │
    ├── Manual validation checks
    │
    ├── Calls _blogService.SavePost(model)
    │       │
    │       ├── BlogService
    │       │       │
    │       │       ├── DbFactoryProvider.GetFactory()
    │       │       ├── db.GetConnection()
    │       │       ├── db.Open()
    │       │       ├── transaction = db.BeginTransaction()
    │       │       │
    │       │       ├── PostCrudService.Insert(postEntity)  ← SQL generated by QueryBuilder
    │       │       │       │
    │       │       │       └── QueryBuilder.BuildInsert<Post>()
    │       │       │               ├── [Table("Post")]
    │       │       │               ├── Exclude [IgnoreInsert] props
    │       │       │               ├── Process [AutoFill] (AddedOn, AddedBy)
    │       │       │               └── INSERT INTO Post (...) VALUES (...)
    │       │       │
    │       │       ├── If SEO: SeoService.Save(seoData)
    │       │       │
    │       │       ├── transaction.Commit()
    │       │       └── Returns postId
    │       │
    │       └── Returns postId
    │
    └── Returns HttpResponse(200, "Saved successfully", new { id = postId })
```

### 16.3 Module Loading Flow

```
Application Start
    │
    ├── Bootstrapper loads all assemblies
    │
    ├── Scans for IServiceRegistrar
    │
    ├── Found: BlogServiceRegistrar
    │       ├── services.AddTransient<IBlogService, BlogService>()
    │       └── Adds EmbeddedFileProvider for BlogEngine views
    │
    ├── Found: AdminModuleRegistrar
    │       ├── services.AddTransient<IMenuService, MenuService>()
    │       └── Adds EmbeddedFileProvider for Admin views
    │
    └── ... all other modules registered
```

---

## 17. Design Patterns Used

| Pattern | Where |
|---------|-------|
| **Plugin/Modular Architecture** | `IServiceRegistrar`, `Bootstrapper` - auto-discovery of modules |
| **Strategy Pattern** | Dialect-specific SQL templates (MsSQL, MySQL, PostgreSQL, SQLite) |
| **Factory Pattern** | `IDatabaseFactory`, `DatabaseFactories.SetFactory()` |
| **Singleton** | `DbFactoryProvider`, `ConfigChangeEvent` |
| **Facade** | `ModelService` wrapping `CrudService<T>` |
| **Repository (simplified)** | `CrudService<T>` as generic data access |
| **Result Pattern** | `ApiResponse<T>` for standardized API responses |
| **Auto-value Injection** | `[AutoFill]` attribute processed before SQL execution |
| **Embedded Resources** | Razor views embedded in RCL modules |
| **Location Expand** | View location expanders for themes, modules, areas |
| **Observer/Event** | `ConfigChangeEvent`, `IYangOnePubSub/KachuwaPubSub` |
| **Provider Pattern** | `IStorageProvider`, `ICacheProvider`, `ILogProvider` |
| **Chain of Responsibility** | Middleware pipeline |

---

## 18. Directory Layout Reference

```
src/
├── App/YOApp/                          # Host web application
│   ├── Program.cs                      # Entry point
│   ├── Startup.cs                      # DI + middleware (474 lines)
│   ├── App_Data/                       # JSON config files
│   ├── db/1.0.0/mssql/                 # SQL install scripts
│   ├── Locale/                         # Localization resources
│   └── Views/                          # Shared views
│
├── Core/
│   ├── YangOne.Core/                   # Interfaces, DI, security, caching (netstandard2.1)
│   │   ├── DI/                         # IServiceRegistrar, Bootstrapper
│   │   ├── Extensions/                 # String, Object, Paging, Reflection extensions
│   │   ├── Caching/                    # ICacheService, ICacheProvider, CacheAttribute
│   │   ├── Configuration/              # YangOneAppConfig, ConfigChangeEvent
│   │   ├── Security/                   # CSP, security headers, nonce service
│   │   ├── Storage/                    # IStorageProvider, LocalStorageProvider, ChunkFile
│   │   ├── Log/                        # ILogger (custom), ILogProvider, FileBaseLogger
│   │   ├── Localization/               # ILocaleResourceProvider, DataAnnotationLocalizer
│   │   ├── Job/                        # IYangOneScheduler, IYangOneJobRunner
│   │   ├── Messaging/                  # IYangOnePubSub, KachuwaPubSub
│   │   ├── Plugin/                     # IPlugin, PluginBootStrapper
│   │   ├── Web/                        # Theme system, view location expanders, user-agent
│   │   ├── State/                      # SessionExtensions
│   │   ├── IO/                         # FileHelper
│   │   ├── Utility/                    # Bits, Converter, IPAddressRange
│   │   └── AntiVirus/                  # IVirusScanner, ScanResult
│   │
│   ├── YangOne.Data/                   # Dapper data access (net10.0)
│   │   ├── IDatabaseFactory.cs         # Factory interface
│   │   ├── DatabaseFactories.cs        # Static registry
│   │   ├── DbFactoryProvider.cs        # Singleton accessor
│   │   ├── QueryBuilder.cs             # SQL generator from POCO (951 lines)
│   │   ├── ISQLTemplate.cs             # Dialect templates
│   │   ├── Crud/
│   │   │   ├── CrudService.cs          # Generic CRUD (501 lines)
│   │   │   ├── CRUDHelper.cs           # Dapper extensions (399 lines)
│   │   │   ├── Dialect.cs              # DB enum
│   │   │   └── Attribute/              # 14 attribute classes
│   │   ├── MsSQL/                      # SQL Server dialect
│   │   ├── MySQL/                      # MySQL dialect
│   │   ├── PostgreSQL/                 # PostgreSQL dialect
│   │   └── SQLLite/                    # SQLite dialect
│   │
│   ├── YangOne.Web/                    # Web framework RCL (net10.0)
│   │   ├── API/                        # BaseApiController, ApiResponse<T>, FileUpload
│   │   ├── Controller/                 # BaseController, InstallerController
│   │   ├── Service/                    # Audit, Email, Permission, SMS, Settings, etc.
│   │   ├── Model/                      # Domain entities (Audit, Country, Email, etc.)
│   │   ├── Module/                     # ModuleManager, ModuleService, ModuleContainer
│   │   ├── TagHelpers/                 # Pager, Module, Theme, Nonce, SEO, etc.
│   │   ├── Security/                   # Authorization, CSP, API obfuscation
│   │   ├── SEO/                        # Meta tags, sitemap builder
│   │   ├── Caching/                    # DefaultCacheService, RedisCacheService
│   │   ├── Middleware/                 # Custom headers, HTTPS, Image resizer, etc.
│   │   ├── Layout/                     # Bootstrap grid, layout renderer
│   │   ├── Localization/               # LocaleService, ResourceProvider
│   │   ├── Notification/               # Notification bar, temp data
│   │   ├── Widget/                     # WidgetService, DashboardWidgetManager
│   │   ├── YOForm/                     # Dynamic form builder
│   │   ├── YOGrid/                     # HTML grid builder
│   │   ├── Templating/                 # Mustache engine, HTML template components
│   │   ├── Page/                       # Page management, file provider
│   │   ├── Plugin/                     # Plugin service
│   │   ├── Printing/                   # POS printer (ECSPOS)
│   │   ├── Image/                      # Image utilities
│   │   ├── Optimizer/                  # Smidge bundler integration
│   │   └── Views/                      # Shared Razor views
│   │
│   ├── YangOne.Identity/               # Custom Identity with Dapper (net10.0)
│   │   ├── Stores/                     # KachuwaUserStore, KachuwaRoleStore
│   │   ├── Service/                    # UserService, RoleService, LoginHistoryService
│   │   ├── Model/                      # AppUser, IdentityUser, IdentityRole, UserDevice
│   │   ├── Dto/                        # NewUser, EditUser, UserStatus, etc.
│   │   ├── ClaimFactory/               # YangOneClaimsPrincipalFactory
│   │   ├── Cryptography/               # EncryptionHelper, AESKeys
│   │   └── Constant/                   # YORoleNames, YORoles, ApplicationClaim
│   │
│   ├── YangOne.OTP/                    # TOTP/HOTP (net10.0)
│   │   ├── Totp.cs, Hotp.cs, Otp.cs
│   │   ├── Services/                   # IOTPService, OTPService
│   │   └── Model/                      # OTPSetting, UserOTP, UserSecret
│   │
│   └── YangOne.RTC.Core/              # SignalR real-time (net10.0)
│       ├── Hubs/                       # BaseHub, YangOneUserHub
│       ├── Service/                    # RTCNotificationService, ConnectionManager
│       └── Model/                      # RTCUser, RtcUserStatus
│
├── Modules/                            # Feature modules (RCL, net10.0)
│   ├── YandOne.Admin/                  # Admin panel (10 API + 19 MVC controllers)
│   ├── YangOne.BlogEngine/             # Blog engine (BlogModule : IModule)
│   ├── YangOne.FCM/                    # Firebase Cloud Messaging
│   ├── YangOne.Localization/           # Localization management
│   ├── YangOne.Messaging/              # Internal messaging
│   ├── YangOne.Identity.Web/           # Identity UI (Login, Register, Manage)
│   └── YangOne.IdentityServer/         # OpenIddict/IdentityServer UI
│
├── Extensions/                         # Extension providers
│   ├── YangOne.SMS.Azure/              # Azure SMS provider
│   └── YangOne.Storage.AzureBlob/      # Azure Blob storage
│
└── Test/YOApp.Test/                    # xUnit test project (scaffold)
```
