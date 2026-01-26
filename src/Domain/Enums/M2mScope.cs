using System.Linq;

namespace Domain.Enums;

public static class M2mScope
{
    public const string SalariesRead = "salaries:read";
    public const string SalariesWrite = "salaries:write";

    public const string InterviewsRead = "interviews:read";
    public const string InterviewsWrite = "interviews:write";

    public const string CompaniesRead = "companies:read";
    public const string CompaniesWrite = "companies:write";

    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";

    public const string StatsRead = "stats:read";

    public const string FullAccess = "*";

    public static readonly string[] AllScopes = new[]
    {
        SalariesRead, SalariesWrite,
        InterviewsRead, InterviewsWrite,
        CompaniesRead, CompaniesWrite,
        UsersRead, UsersWrite,
        StatsRead, FullAccess,
    };

    public static bool IsValidScope(string scope) => AllScopes.Contains(scope);
}
