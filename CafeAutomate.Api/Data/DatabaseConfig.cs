using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CafeAutomate.Api.Data;

/// <summary>
/// Resolves which database the API runs against.
///
/// PostgreSQL is preferred. If it is unreachable at startup the API degrades to
/// a local SQLite file instead of failing to boot, so the cafe can keep taking
/// orders while the Postgres server is down.
/// </summary>
public static class DatabaseConfig
{
    /// <summary>Seconds to wait for Postgres before giving up and using SQLite.</summary>
    private const int ProbeTimeoutSeconds = 5;

    public static void AddAppDatabase(this WebApplicationBuilder builder)
    {
        var primaryUrl = builder.Configuration["DATABASE_URL"];
        // docker-compose still supplies a plain ADO string under this key.
        var legacyAdo = builder.Configuration.GetConnectionString("Postgres");

        string? postgres = null;
        if (!string.IsNullOrWhiteSpace(primaryUrl))
            postgres = BuildNpgsqlConnectionString(primaryUrl!);
        else if (!string.IsNullOrWhiteSpace(legacyAdo))
            postgres = legacyAdo;

        if (postgres is null)
        {
            Console.WriteLine("[db] No PostgreSQL configured. Using SQLite.");
        }
        else if (TryConnect(postgres, out var failure))
        {
            Console.WriteLine("[db] Using PostgreSQL.");
            builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(postgres));
            return;
        }
        else
        {
            Console.WriteLine($"[db] PostgreSQL unavailable ({failure}). Falling back to SQLite.");
        }

        var sqlite = BuildSqliteConnectionString(
            builder.Configuration["DATABASE_FALLBACK_URL"], builder.Environment.ContentRootPath);

        builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(sqlite));
    }

    /// <summary>
    /// Accepts either a URL (postgresql://user:pass@host:port/db) or an ADO
    /// connection string, and returns an ADO connection string.
    /// </summary>
    public static string BuildNpgsqlConnectionString(string value)
    {
        if (!LooksLikeUrl(value, "postgresql", "postgres"))
        {
            // Already an ADO string — just make sure the probe cannot hang.
            var passthrough = new NpgsqlConnectionStringBuilder(value);
            if (passthrough.Timeout > ProbeTimeoutSeconds) passthrough.Timeout = ProbeTimeoutSeconds;
            return passthrough.ConnectionString;
        }

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Timeout = ProbeTimeoutSeconds
        };

        if (userInfo.Length > 0 && userInfo[0].Length > 0)
            csb.Username = Uri.UnescapeDataString(userInfo[0]);
        if (userInfo.Length > 1)
            csb.Password = Uri.UnescapeDataString(userInfo[1]);

        // Support ?sslmode=require style query parameters.
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2) csb[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
        }

        return csb.ConnectionString;
    }

    /// <summary>
    /// Accepts sqlite:///relative/path.db, sqlite:////absolute/path.db, a bare
    /// path, or null (which uses the default location).
    /// </summary>
    public static string BuildSqliteConnectionString(string? value, string contentRoot)
    {
        var path = string.IsNullOrWhiteSpace(value)
            ? Path.Combine("Data", "cafe.db")
            : StripSqliteScheme(value!);

        if (!Path.IsPathRooted(path))
            path = Path.GetFullPath(path, contentRoot);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return $"Data Source={path}";
    }

    private static string StripSqliteScheme(string value)
    {
        if (!LooksLikeUrl(value, "sqlite")) return value;

        // sqlite:///rel/path -> rel/path      (three slashes = relative)
        // sqlite:////abs/path -> /abs/path    (four slashes = absolute)
        var rest = value[("sqlite://".Length)..];
        if (rest.StartsWith('/')) rest = rest[1..];
        return rest;
    }

    private static bool LooksLikeUrl(string value, params string[] schemes) =>
        schemes.Any(s => value.StartsWith(s + "://", StringComparison.OrdinalIgnoreCase));

    private static bool TryConnect(string connectionString, out string error)
    {
        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            error = string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message.Split('\n')[0].Trim();
            return false;
        }
    }
}
