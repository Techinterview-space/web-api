using System.Threading.Tasks;

namespace MG.Utils.AspNetCore.Views
{
    public interface IView
    {
        Task<string> RenderAsync<T>(string view, T model);
    }
}