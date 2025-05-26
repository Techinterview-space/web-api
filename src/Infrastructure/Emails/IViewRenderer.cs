namespace Infrastructure.Emails;

public interface IViewRenderer
{
    Task<string> RenderHtmlAsync<T>(
        string view,
        T model);
}