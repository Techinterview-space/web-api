using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Web.Api.Middlewares.Error;

public class JsonErrorResponse
{
    private readonly HttpContext _context;
    private readonly string _error;
    private readonly int _statusCode;

    public JsonErrorResponse(HttpContext context, string serializedError, int statusCode)
    {
        _context = context;
        _error = serializedError;
        _statusCode = statusCode;
    }

    public Task WriteAsync()
    {
        _context.Response.ContentType = "application/json";
        _context.Response.StatusCode = _statusCode;

        return _context.Response.WriteAsync(_error);
    }
}