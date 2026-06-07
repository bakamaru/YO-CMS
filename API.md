# YO-Framework API Reference

**Base URL:** `/api/v1/`  
**Auth:** JWT Bearer or Cookie (via `[ApiAuthorize]` unless `[AllowAnonymous]` specified)  
**Response Envelope:**
```json
{ "Code": 200, "Message": "success", "Data": {}, "Errors": [] }
```

---

## 1. Core — File Upload

### ChunkFileUploadController
`api/v1/file` — `[AllowAnonymous]`

| Method | Route | Params | Returns | Notes |
|--------|-------|--------|---------|-------|
| POST | `/create/{userId}` | `userId` (long), `sessionParams` (form) | `HttpResponse(200)` | Create upload session |
| PUT | `/upload/user/{userId}/session/{sessionId}/` | `userId` (long), `sessionId` (string), `chunkNumber` (int, query), `file` (IFormFile) | `HttpResponse(200)` | Upload file chunk |
| GET | `/upload/{sessionId}` | `sessionId` (string) | `HttpResponse(200)` | Get upload status |
| GET | `/uploads` | — | `HttpResponse(200)` | All uploads status |
| GET | `/download/{sessionId}` | `sessionId` (string) | streams file | Download uploaded file |

### FileUploadApiController
`api/v1/kachuwa/grid` — inherits `BaseController` (not `BaseApiController`)

| Method | Route | Params | Returns | Notes |
|--------|-------|--------|---------|-------|
| POST | `/ajaxfileupload` | `Request.Form.Files`, `fdr` (form) | `{ isUploaded, savedFilePath }` | Upload file(s) |
| POST | `/ajaxupload` | `Request.Form.Files`, `fdr` (form) | `{ isUploaded, savedFilePath, w, h }` | Upload with dimensions |
| POST | `/remove` | `file` (string, body) | `{ Code, Message, Data }` | Delete file from disk |

---

## 2. YandOne.Admin — HTML Components

### HtmlComponentAPIController
`api/v1/htmlcomponent`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/check/unique` | `name`, `oldName` (string), `htmlComponentId` (int, default=0) | `SuccessResponse` |
| GET | `/all/active` | `offset` (int, d=1), `limit` (int, d=20), `query` (string) | `SuccessResponse` |
| GET | `/all` | `offset` (int, d=1), `limit` (int, d=20), `query` (string) | `SuccessResponse` |
| GET | `/{id}` | `id` (int) | `SuccessResponse` |
| POST | `/save` | `request` (HtmlComponentSaveRequest, body) | `SuccessResponse` |
| DELETE | `/{id}` | `id` (int) | `SuccessResponse` |

---

## 3. YandOne.Admin — Localization

### LocalizationApiController
`api/v1/localization`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/region/all` | `pageNo` (int, d=1), `rowsPerPage` (int, d=10), `query` (string) | `SuccessResponse` |
| GET | `/region/byid/{regionId}` | `regionId` (int) | `SuccessResponse` |
| GET | `/country/all` | — | `SuccessResponse` |
| POST | `/import` | `request` (LocalizationImportRequest, form) | `SuccessResponse` |
| POST | `/region/new` | `model` (LocaleRegion, body) | `SuccessResponse` |
| POST | `/region/update` | `model` (LocaleRegionEditViewModel, body) | `SuccessResponse` |
| GET | `/region/{localRegionId}/resource/all` | `localRegionId` (int), `pageNo` (int, d=1), `limit` (int, d=20) | `SuccessResponse` |
| GET | `/export/{localRegionId}` | `localRegionId` (int) | File (Excel .xlsx) |
| POST | `/set-default` | `request` (SetDefaultLocaleRequest, body) | `SuccessResponse` |
| POST | `/resource/save` | `model` (LocaleResource, body) | `SuccessResponse` |
| DELETE | `/region/{id}` | `id` (int) | `SuccessResponse` |
| POST | `/language` | `request` (SetLanguageRequest, body) | `SuccessResponse` |

---

## 4. YandOne.Admin — Media Library

### MediaLibraryApiController
`api/v1/media-library`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| POST | `/directory/save` | `model` (DirectoryViewModel, body) | `SuccessResponse` |
| POST | `/file/rename` | `request` (RenameFileRequest, body) | `SuccessResponse` |
| GET | `/content/all` | `currentDir` (string, d="/") | `SuccessResponse` |
| GET | `/directory/all` | `currentDir` (string, d="/") | `SuccessResponse` |
| POST | `/file/upload` | `request` (UploadFileRequest, form) `[RequestSizeLimit(long.MaxValue)]` | `SuccessResponse` |
| POST | `/file/copy` | `request` (FileTransferRequest, body) | `SuccessResponse` |
| POST | `/file/move` | `request` (FileTransferRequest, body) | `SuccessResponse` |
| POST | `/file/delete` | `request` (DeleteFilesRequest, body) | `SuccessResponse` |

---

## 5. YandOne.Admin — Menu

### MenuAPIController
`api/v1/menu`  
Endpoints marked 🔒 require `[Authorize(Roles = "Admin,SuperAdmin")]`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/mainnavigation` | — | `SuccessResponse` |
| GET | `/group` | `groupId` (int, d=0), `offset` (int, d=1), `limit` (int, d=50), `query` (string) | `SuccessResponse` |
| GET | `/backend` | `offset` (int, d=0), `limit` (int, d=50), `query` (string) | `SuccessResponse` |
| GET | `/frontend` | `offset` (int, d=0), `limit` (int, d=50), `query` (string) | `SuccessResponse` |
| GET 🔒 | `/{id}` | `id` (int) | `SuccessResponse` |
| POST 🔒 | `/save` | `model` (MenuViewModel, body) | `SuccessResponse` |
| DELETE 🔒 | `/{id}` | `id` (int) | `SuccessResponse` |
| POST 🔒 | `/order/save` | `orders` (List\<MenuOrderViewModel\>, body) | `SuccessResponse` |
| GET 🔒 | `/roles` | — | `SuccessResponse` |
| GET 🔒 | `/groups` | — | `SuccessResponse` |
| POST 🔒 | `/groups/save` | `model` (MenuGroup, body) | `SuccessResponse` |
| DELETE 🔒 | `/groups/delete/{id}` | `id` (int) | `SuccessResponse` |

---

## 6. YandOne.Admin — Misc

### MiscAPIController
`api/v1/misc`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/country/all` | — | `SuccessResponse` |

---

## 7. YandOne.Admin — Module Management

### ModuleApiController
`api/v1/module` — `[Authorize(Roles = "Admin,SuperAdmin")]`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/all` | `pageNo` (int, d=1), `pageSize` (int, d=8), `status` (int, d=1), `query` (string) | `SuccessResponse` |
| POST | `/install` | `request` (body) | `SuccessResponse` |
| POST | `/uninstall` | `request` (body) | `SuccessResponse` |

---

## 8. YandOne.Admin — Permissions

### PermissionApiController
`api/v1/permission` — `[Authorize(Roles = "Admin,SuperAdmin")]`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/role/{roleId}` | `roleId` (int) | `SuccessResponse` |
| POST | `/role/{roleId}/save` | `roleId` (int), `rolePermissions` (RolePermissionViewModel, body) | `SuccessResponse` |
| GET | `/role/all` | — | `SuccessResponse` |

---

## 9. YandOne.Admin — SEO

### SeoApiController
`api/v1/seo`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/all` | `offset` (int, d=1), `limit` (int, d=20), `query` (string) | `SuccessResponse` |
| GET | `/by-url` | `url` (string), `type` (string) | `SuccessResponse` |
| GET | `/meta` | `url` (string), `type` (string) | `SuccessResponse` |
| GET | `/by-id/{seoId}` | `seoId` (int) | `SuccessResponse` |
| GET | `/by-seotype` | `seoType` (string), `id` (int) | `SuccessResponse` |
| GET | `/by-product` | `productId` (int), `type` (string) | `SuccessResponse` |
| POST | `/check-url` | `request` (CheckSeoUrlRequest, body) | `SuccessResponse` |
| POST | `/new` | `model` (SEO, form) | `SuccessResponse` |
| POST | `/update` | `model` (SEO, form) | `SuccessResponse` |
| DELETE | `/{seoId}` | `seoId` (int) | `SuccessResponse` |
| GET | `/jsonld/website` | — | `SuccessResponse` |
| GET | `/jsonld/page` | — | `SuccessResponse` |
| GET | `/jsonld/page/by-product` | `page` (string), `productId` (int), `type` (string) | `SuccessResponse` |
| GET | `/meta/generate` | — | `SuccessResponse` |

---

## 10. YandOne.Admin — Settings

### SettingAPIController
`api/v1/setting`  
Endpoint marked 🔒 requires `[Authorize(Roles = "SuperUser")]`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/web` | — | `SuccessResponse` |
| POST | `/web/save` | `model` (Setting, form) | `SuccessResponse` |
| POST 🔒 | `/app/restart` | — | `SuccessResponse` |
| GET | `/csp` | — | `SuccessResponse` |
| POST | `/csp/save` | `config` (CspConfig, body) | `SuccessResponse` |
| GET | `/api` | — | `SuccessResponse` |
| POST | `/api/save` | `model` (ApiConfig, body) | `SuccessResponse` |
| GET | `/optimization` | — | `SuccessResponse` |
| POST | `/optimization/save` | `model` (OptimizationConfig, body) | `SuccessResponse` |
| GET | `/file` | — | `SuccessResponse` |
| POST | `/file/save` | `config` (FileConfig, body) | `SuccessResponse` |
| GET | `/basic` | — | `SuccessResponse` |
| POST | `/basic/save` | `model` (AppBasicSecurity, body) | `SuccessResponse` |

---

## 11. YangOne.BlogEngine — Posts

### BlogApiController
`api/v1/post`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/all` | `offset` (int, d=1), `limit` (int, d=10), `query` (string) | `SuccessResponse` |
| GET | `/latest` | `offset` (int, d=1), `limit` (int, d=12) | `SuccessResponse` |
| GET | `/detail/{url}` | `url` (string) | `SuccessResponse` |
| GET | `/setting` | — | `SuccessResponse` |
| GET | `/related/all` | `page` (int, d=1), `limit` (int, d=10), `postId` (int, d=0) | `SuccessResponse` |
| GET | `/category/all` | `page` (int, d=1), `limit` (int, d=10) | `SuccessResponse` |
| POST | `/category/save` | `model` (PostCategory, body) | `SuccessResponse` |
| POST | `/save` | `model` (SavePostViewModel, body) | `SuccessResponse` |
| POST | `/setting/save` | `model` (PostSetting, body) | `SuccessResponse` |
| GET | `/popular` | — | `HttpResponse(200)` |
| GET | `/byid` | `id` (int) | `SuccessResponse` |
| DELETE | `/delete/{id}` | `id` (int) | `SuccessResponse` |

---

## 12. YangOne.FCM — Firebase Push

### FCMTestApiController
`api/v1/fcm` — `[AllowAnonymous]`

| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/test` | `usertoken`, `title`, `message`, `clickUrl`, `imageUrl`, `key1`, `key2`, `key3` (string, query) | `HttpResponse(200)` |

---

## 13. YangOne.IdentityServer — OpenIddict Admin

### OpenIddictAdminApiController
`api/v1/openiddict`

#### Dashboard
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/dashboard/stat` | — | `SuccessResponse` |

#### Lookups
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/consenttype/all` | — | `SuccessResponse` |
| GET | `/clienttype/all` | — | `SuccessResponse` |
| GET | `/permission/all` | — | `SuccessResponse` |

#### Applications
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/application/all` | `offset` (int, d=1), `limit` (int, d=20), `search` (string) | `SuccessResponse` |
| GET | `/application/detail/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/application/save` | `model` (CreateOrUpdateApplicationRequest, body) | `SuccessResponse` |
| DELETE | `/application/delete/{id}` | `id` (string) | `SuccessResponse` |

#### API Resources
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/apiresource/all` | `offset` (int, d=1), `limit` (int, d=20), `search` (string) | `SuccessResponse` |
| GET | `/apiresource/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/apiresource/save` | `model` (ApiResourceDto, body) | `SuccessResponse` |
| POST | `/apiresource/update` | `model` (ApiResourceDto, body) | `SuccessResponse` |
| DELETE | `/apiresource/{id}` | `id` (string) | `SuccessResponse` |

#### Identity Resources
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/identityresource/all` | `offset` (int, d=1), `limit` (int, d=20), `search` (string) | `SuccessResponse` |
| GET | `/identityresource/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/identityresource/save` | `model` (IdentityResourceDto, body) | `SuccessResponse` |
| POST | `/identityresource/update` | `model` (IdentityResourceDto, body) | `SuccessResponse` |
| DELETE | `/identityresource/{id}` | `id` (string) | `SuccessResponse` |

#### API Scopes
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/apiscope/all` | `offset` (int, d=1), `limit` (int, d=20), `search` (string) | `SuccessResponse` |
| GET | `/apiscope/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/apiscope/save` | `model` (ApiScopeDto, body) | `SuccessResponse` |
| POST | `/apiscope/update` | `model` (ApiScopeDto, body) | `SuccessResponse` |
| DELETE | `/apiscope/{id}` | `id` (string) | `SuccessResponse` |

#### Grants / Authorizations
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/grant/all` | `offset` (int, d=1), `limit` (int, d=20), `search` (string) | `SuccessResponse` |
| GET | `/grant/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/grant/save` | `model` (CreateGrantRequest, body) | `SuccessResponse` |
| POST | `/grant/update` | `model` (UpdateGrantRequest, body) | `SuccessResponse` |
| DELETE | `/grant/{id}` | `id` (string) | `SuccessResponse` |
| POST | `/grant/revoke` | `id` (string, query) | `SuccessResponse` |

---

## 14. JSON MVC Endpoints (Non-API Controllers)

### InstallerController
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/install/ping` | — | `"pong"` |
| POST | `/install` | `model` (InstallerViewModel, body) | `JsonResult` |
| POST | `/install/checkconnection` | `model` (InstallerViewModel, body) | `JsonResult` |
| POST | `/install/setupadmin` | `model` (InstallerUserViewModel, body) | `JsonResult` |

### AccountController (YangOne.Identity.Web)
| Method | Route | Params | Returns |
|--------|-------|--------|---------|
| GET | `/account/verify/sms` | `idx` (encrypted email, query) | `JsonResult` |
| GET | `/account/verify/email` | `idx` (encrypted email, query) | `JsonResult` |

---

## Summary

| Module | Controller | Endpoints |
|--------|-----------|-----------|
| Core | ChunkFileUploadController | 5 |
| Core | FileUploadApiController | 3 |
| Admin | HtmlComponentAPIController | 6 |
| Admin | LocalizationApiController | 12 |
| Admin | MediaLibraryApiController | 8 |
| Admin | MenuAPIController | 12 |
| Admin | MiscAPIController | 1 |
| Admin | ModuleApiController | 3 |
| Admin | PermissionApiController | 3 |
| Admin | SeoApiController | 14 |
| Admin | SettingAPIController | 13 |
| BlogEngine | BlogApiController | 12 |
| FCM | FCMTestApiController | 1 |
| IdentityServer | OpenIddictAdminApiController | 29 |
| — | InstallerController (MVC) | 4 |
| — | AccountController (MVC) | 2 |
| **Total** | | **128** |
