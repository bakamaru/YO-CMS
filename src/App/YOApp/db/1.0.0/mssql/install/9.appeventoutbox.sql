CREATE TABLE dbo.AppEventOutbox
(
    AppEventOutboxId bigint primary key identity(1,1) not null,

    EventKey nvarchar(200) not null,
    EventName nvarchar(256) null,
    ModuleName nvarchar(100) null,

    TenantId bigint null,
    ActorUserId bigint null,

    CorrelationId nvarchar(100) null,
    IdempotencyKey nvarchar(200) null,

    PayloadJson nvarchar(max) not null,
    HeadersJson nvarchar(max) null,

    Status nvarchar(50) not null default('Pending'),
    -- Pending, Processing, Completed, Failed, DeadLetter

    Priority int not null default(100),

    RetryCount int not null default(0),
    MaxRetryCount int not null default(5),
    NextRetryOn datetime2(3) null,

    LockedBy nvarchar(200) null,
    LockedOn datetime2(3) null,
    ProcessedOn datetime2(3) null,

    ErrorMessage nvarchar(max) null,

    IsActive bit NOT NULL DEFAULT(1),
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL,
    DeletedOn datetime2(3) NULL,
    DeletedBy bigint NULL
);

CREATE INDEX IX_AppEventOutbox_Status_NextRetryOn
ON dbo.AppEventOutbox(Status, NextRetryOn, Priority, AddedOn)
INCLUDE(EventKey, TenantId, ActorUserId, RetryCount, MaxRetryCount);

CREATE INDEX IX_AppEventOutbox_EventKey
ON dbo.AppEventOutbox(EventKey, AddedOn);

CREATE UNIQUE INDEX UX_AppEventOutbox_IdempotencyKey
ON dbo.AppEventOutbox(IdempotencyKey)
WHERE IdempotencyKey IS NOT NULL;
