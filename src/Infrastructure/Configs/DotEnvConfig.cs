using dotenv.net;

namespace Infrastructure.Configs;

public class DotEnvConfig
{
    /// <summary>
    /// Loads environment variables from .env file if it exists.
    /// Searches in the following locations (first found wins):
    /// 1. Current directory (.env)
    /// 2. Parent directory (../. env) - for running from src/Web.Api
    /// 3. Two levels up (../../.env) - for running from src/Web.Api/bin/Debug
    /// </summary>
    public static void LoadEnvFileIfExists()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", ".env"),
        };

        foreach (var envFilePath in possiblePaths)
        {
            var fullPath = Path.GetFullPath(envFilePath);
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"Loading environment variables from: {fullPath}");
                DotEnv.Load(options: new DotEnvOptions(
                    ignoreExceptions: true,
                    envFilePaths: new[] { fullPath },
                    overwriteExistingVars: true));
                return;
            }
        }

        Console.WriteLine("No .env file found, using default configuration");
    }
}