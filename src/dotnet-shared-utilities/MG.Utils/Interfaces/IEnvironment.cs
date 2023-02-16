namespace MG.Utils.Interfaces
{
    public interface IEnvironment
    {
        string BaseUrl { get; }

        string ImageBaseUrl { get; }

        string Environment { get; }

        bool IsProduction { get; }
    }
}