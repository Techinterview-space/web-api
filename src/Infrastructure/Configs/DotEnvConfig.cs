using dotenv.net;

namespace Infrastructure.Configs;

public class DotEnvConfig
{
    public static void LoadEnvFileIfExists()
    {
        var parentDir = Directory.GetParent(Directory.GetCurrentDirectory());
        var envFilePath = Path.Combine(parentDir!.FullName, ".env");

        DotEnv.Fluent()
            .WithExceptions()
            .WithEnvFiles(envFilePath)
            .WithTrimValues()
            .WithOverwriteExistingVars()
            .WithProbeForEnv(probeLevelsToSearch: 6)
            .Load();
    }
}