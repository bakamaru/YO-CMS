-- Notification Events
CREATE TABLE dbo.NotificationEvent
(
    NotificationEventId bigint primary key identity(1,1) not null,
    EventKey nvarchar(200) not null,
    Name nvarchar(256) not null,
    ModuleName nvarchar(100) null,
    Description nvarchar(max) null,
    SamplePayloadJson nvarchar(max) null,
    IsActive bit NOT NULL DEFAULT(1),
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE UNIQUE INDEX UX_NotificationEvent_EventKey ON dbo.NotificationEvent(EventKey) WHERE IsDeleted = 0;
CREATE INDEX IX_NotificationEvent_ModuleName ON dbo.NotificationEvent(ModuleName) WHERE IsDeleted = 0;

-- Notification Templates
CREATE TABLE dbo.NotificationTemplate
(
    NotificationTemplateId bigint primary key identity(1,1) not null,
    TemplateKey nvarchar(200) not null,
    Name nvarchar(256) not null,
    Channel nvarchar(50) not null,
    LanguageCode nvarchar(10) not null default('en'),
    SubjectTemplate nvarchar(max) null,
    BodyTemplate nvarchar(max) null,
    SamplePayloadJson nvarchar(max) null,
    IsDefault bit NOT NULL DEFAULT(0),
    IsActive bit NOT NULL DEFAULT(1),
    IsDeleted bit NOT NULL DEFAULT(0),
    Version int NOT NULL DEFAULT(1),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE INDEX IX_NotificationTemplate_Channel ON dbo.NotificationTemplate(Channel) WHERE IsDeleted = 0;
CREATE INDEX IX_NotificationTemplate_TemplateKey ON dbo.NotificationTemplate(TemplateKey) WHERE IsDeleted = 0;

-- Notification Rules
CREATE TABLE dbo.NotificationRule
(
    NotificationRuleId bigint primary key identity(1,1) not null,
    NotificationEventId bigint not null,
    Name nvarchar(256) not null,
    Priority int NOT NULL DEFAULT(100),
    IsEnabled bit NOT NULL DEFAULT(1),
    ConditionJson nvarchar(max) null,
    DelaySeconds int NOT NULL DEFAULT(0),
    MaxSendPerUserPerDay int NOT NULL DEFAULT(5),
    StopProcessingAfterMatch bit NOT NULL DEFAULT(0),
    QuietHoursEnabled bit NOT NULL DEFAULT(0),
    QuietHoursStart time(3) NULL,
    QuietHoursEnd time(3) NULL,
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE INDEX IX_NotificationRule_NotificationEventId ON dbo.NotificationRule(NotificationEventId) WHERE IsDeleted = 0;

-- Notification Rule Recipients
CREATE TABLE dbo.NotificationRuleRecipient
(
    NotificationRuleRecipientId bigint primary key identity(1,1) not null,
    NotificationRuleId bigint not null,
    RecipientType nvarchar(50) not null,
    RecipientValue nvarchar(max) null,
    SortOrder int NOT NULL DEFAULT(0),
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE INDEX IX_NotificationRuleRecipient_NotificationRuleId ON dbo.NotificationRuleRecipient(NotificationRuleId) WHERE IsDeleted = 0;

-- Notification Rule Channels
CREATE TABLE dbo.NotificationRuleChannel
(
    NotificationRuleChannelId bigint primary key identity(1,1) not null,
    NotificationRuleId bigint not null,
    Channel nvarchar(50) not null,
    NotificationTemplateId bigint not null,
    IsRequired bit NOT NULL DEFAULT(1),
    SortOrder int NOT NULL DEFAULT(0),
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE INDEX IX_NotificationRuleChannel_NotificationRuleId ON dbo.NotificationRuleChannel(NotificationRuleId) WHERE IsDeleted = 0;

-- Notification Send Logs
CREATE TABLE dbo.NotificationSendLog
(
    NotificationSendLogId bigint primary key identity(1,1) not null,
    NotificationEventId bigint null,
    NotificationRuleId bigint null,
    NotificationTemplateId bigint null,
    UserId bigint not null,
    EventKey nvarchar(200) not null,
    Channel nvarchar(50) not null,
    TemplateKey nvarchar(256) null,
    Provider nvarchar(100) null,
    Status nvarchar(50) not null,
    Receiver nvarchar(max) null,
    Subject nvarchar(max) null,
    Message nvarchar(max) null,
    ErrorMessage nvarchar(max) null,
    PayloadJson nvarchar(max) null,
    RenderedSubject nvarchar(max) null,
    RenderedBody nvarchar(max) null,
    RetryCount int NOT NULL DEFAULT(0),
    SentOn datetime2(3) NULL,
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0)
);

CREATE INDEX IX_NotificationSendLog_UserId ON dbo.NotificationSendLog(UserId);
CREATE INDEX IX_NotificationSendLog_EventKey ON dbo.NotificationSendLog(EventKey);
CREATE INDEX IX_NotificationSendLog_Status ON dbo.NotificationSendLog(Status);
CREATE INDEX IX_NotificationSendLog_Channel ON dbo.NotificationSendLog(Channel);
CREATE INDEX IX_NotificationSendLog_AddedOn ON dbo.NotificationSendLog(AddedOn);

-- Notification Preferences
CREATE TABLE dbo.NotificationPreference
(
    NotificationPreferenceId bigint primary key identity(1,1) not null,
    UserId bigint not null,
    EmailEnabled bit NOT NULL DEFAULT(1),
    SmsEnabled bit NOT NULL DEFAULT(1),
    PushEnabled bit NOT NULL DEFAULT(1),
    InAppEnabled bit NOT NULL DEFAULT(1),
    QuietHoursEnabled bit NOT NULL DEFAULT(0),
    QuietHoursStart time(3) NULL,
    QuietHoursEnd time(3) NULL,
    MutedCategoriesJson nvarchar(max) null,
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0),
    UpdatedOn datetime2(3) NULL,
    UpdatedBy bigint NULL
);

CREATE UNIQUE INDEX UX_NotificationPreference_UserId ON dbo.NotificationPreference(UserId) WHERE IsDeleted = 0;

-- Notification Inbox
CREATE TABLE dbo.NotificationInbox
(
    NotificationInboxId bigint primary key identity(1,1) not null,
    UserId bigint not null,
    NotificationEventId bigint null,
    NotificationRuleId bigint null,
    NotificationTemplateId bigint null,
    EventKey nvarchar(200) not null,
    Channel nvarchar(50) not null,
    Title nvarchar(256) not null,
    Message nvarchar(max) not null,
    Severity nvarchar(50) not null default('Info'),
    Url nvarchar(max) null,
    PayloadJson nvarchar(max) null,
    IsRead bit NOT NULL DEFAULT(0),
    ReadOn datetime2(3) NULL,
    IsDeleted bit NOT NULL DEFAULT(0),
    AddedOn datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
    AddedBy bigint NOT NULL DEFAULT(0)
);

CREATE INDEX IX_NotificationInbox_UserId ON dbo.NotificationInbox(UserId) WHERE IsDeleted = 0;
CREATE INDEX IX_NotificationInbox_IsRead ON dbo.NotificationInbox(IsRead) WHERE IsDeleted = 0;
CREATE INDEX IX_NotificationInbox_EventKey ON dbo.NotificationInbox(EventKey) WHERE IsDeleted = 0;
CREATE INDEX IX_NotificationInbox_AddedOn ON dbo.NotificationInbox(AddedOn) WHERE IsDeleted = 0;
