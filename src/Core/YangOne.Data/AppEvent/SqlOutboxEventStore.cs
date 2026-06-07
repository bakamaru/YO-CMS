using Dapper;
using System.Data;
using YangOne.AppEvent;
using YangOne.AppEvent.Dto;

namespace YangOne.Data.AppEvent
{
    public class SqlOutboxEventStore : IOutboxEventStore
    {
        private readonly IDatabaseFactory _dbFactory;

        public SqlOutboxEventStore(IDatabaseFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        private IDbConnection GetConnection() => _dbFactory.GetConnection();

        public async Task<long> AddAsync(
            OutboxEventCreateDto dto,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
INSERT INTO dbo.AppEventOutbox
(
    EventKey,
    EventName,
    ModuleName,
    TenantId,
    ActorUserId,
    CorrelationId,
    IdempotencyKey,
    PayloadJson,
    HeadersJson,
    Priority,
    MaxRetryCount,
    Status,
    AddedOn
)
VALUES
(
    @EventKey,
    @EventName,
    @ModuleName,
    @TenantId,
    @ActorUserId,
    @CorrelationId,
    @IdempotencyKey,
    @PayloadJson,
    @HeadersJson,
    @Priority,
    @MaxRetryCount,
    'Pending',
    SYSUTCDATETIME()
);

SELECT CAST(SCOPE_IDENTITY() AS bigint);
";

            using var db = GetConnection();
            return await db.ExecuteScalarAsync<long>(sql, dto);
        }

        public async Task<IReadOnlyList<OutboxEventDto>> GetPendingAsync(
            int take,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
SELECT TOP (@Take)
    AppEventOutboxId,
    EventKey,
    EventName,
    ModuleName,
    TenantId,
    ActorUserId,
    CorrelationId,
    IdempotencyKey,
    PayloadJson,
    HeadersJson,
    Status,
    Priority,
    RetryCount,
    MaxRetryCount
FROM dbo.AppEventOutbox WITH (READPAST, UPDLOCK, ROWLOCK)
WHERE 
    IsActive = 1
    AND IsDeleted = 0
    AND Status IN ('Pending', 'Failed')
    AND RetryCount < MaxRetryCount
    AND (NextRetryOn IS NULL OR NextRetryOn <= SYSUTCDATETIME())
ORDER BY Priority ASC, AddedOn ASC;
";

            using var db = GetConnection();
            var result = await db.QueryAsync<OutboxEventDto>(sql, new { Take = take });
            return result.ToList();
        }

        public async Task MarkProcessingAsync(
            long outboxEventId,
            string lockedBy,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.AppEventOutbox
SET 
    Status = 'Processing',
    LockedBy = @LockedBy,
    LockedOn = SYSUTCDATETIME(),
    UpdatedOn = SYSUTCDATETIME()
WHERE AppEventOutboxId = @OutboxEventId;
";

            using var db = GetConnection();
            await db.ExecuteAsync(sql, new
            {
                OutboxEventId = outboxEventId,
                LockedBy = lockedBy
            });
        }

        public async Task MarkCompletedAsync(
            long outboxEventId,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.AppEventOutbox
SET 
    Status = 'Completed',
    ProcessedOn = SYSUTCDATETIME(),
    UpdatedOn = SYSUTCDATETIME(),
    LockedBy = NULL,
    LockedOn = NULL
WHERE AppEventOutboxId = @OutboxEventId;
";

            using var db = GetConnection();
            await db.ExecuteAsync(sql, new
            {
                OutboxEventId = outboxEventId
            });
        }

        public async Task MarkFailedAsync(
            long outboxEventId,
            string errorMessage,
            DateTime nextRetryOn,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
UPDATE dbo.AppEventOutbox
SET 
    RetryCount = RetryCount + 1,
    Status = CASE 
                WHEN RetryCount + 1 >= MaxRetryCount THEN 'DeadLetter'
                ELSE 'Failed'
             END,
    ErrorMessage = @ErrorMessage,
    NextRetryOn = @NextRetryOn,
    UpdatedOn = SYSUTCDATETIME(),
    LockedBy = NULL,
    LockedOn = NULL
WHERE AppEventOutboxId = @OutboxEventId;
";

            using var db = GetConnection();
            await db.ExecuteAsync(sql, new
            {
                OutboxEventId = outboxEventId,
                ErrorMessage = errorMessage,
                NextRetryOn = nextRetryOn
            });
        }
    }
}
