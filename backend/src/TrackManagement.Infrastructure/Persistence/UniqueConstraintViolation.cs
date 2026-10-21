using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace TrackManagement.Infrastructure.Persistence;

/// <summary>
/// Recognizes SQL Server unique index/constraint violations so callers outside Infrastructure
/// don't need to know provider error numbers.
/// </summary>
public static class UniqueConstraintViolation
{
    // 2601: duplicate key row in object with unique index. 2627: violation of UNIQUE KEY / PRIMARY KEY constraint.
    private static readonly int[] SqlServerErrorNumbers = [2601, 2627];

    public static bool IsUniqueConstraintViolation(this DbUpdateException exception) =>
        exception.InnerException is SqlException sqlException
        && SqlServerErrorNumbers.Contains(sqlException.Number);
}
