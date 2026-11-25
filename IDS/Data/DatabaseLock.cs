using Microsoft.EntityFrameworkCore;

namespace IDS.Data;

public static class DatabaseLock
{
    // The caller holds a transaction for the operation. SQL Server releases the
    // application lock at commit/rollback, including on connection loss.
    public static async Task AcquireAsync(ApplicationDbContext db, string resource, CancellationToken token)
    {
        if (!db.Database.IsSqlServer()) return;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive',
                @LockOwner = 'Transaction', @LockTimeout = 5000;
            IF @result < 0 THROW 51000, 'IDS operation lock unavailable.', 1;
            """, token);
    }
}
