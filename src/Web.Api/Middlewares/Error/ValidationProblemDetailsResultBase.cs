using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Web.Api.Middlewares.Error;

public abstract class ValidationProblemDetailsResultBase : IActionResult
{
    public async Task ExecuteResultAsync(ActionContext context)
    {
        var modelStateEntries = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToArray();

        var errors = new List<ValidationError>();

        if (modelStateEntries.Any())
        {
            foreach ((string key, ModelStateEntry value) in modelStateEntries)
            {
                errors.AddRange(value.Errors
                    .Select(modelStateError => Error(
                        key: key,
                        modelStateError: modelStateError)));
            }
        }

        await new JsonErrorResponse(
                context: context.HttpContext,
                serializedError: Serialize(new ValidationProblemDetails(errors, AppInstanceName)),
                statusCode: ValidationProblemDetails.ValidationStatusCode)
            .WriteAsync();
    }

    public abstract string AppInstanceName { get; }

    public abstract string Serialize<T>([NotNull] T instance);

    protected virtual ValidationError Error(string key, ModelError modelStateError)
    {
        return new ValidationError(
            name: key,
            description: modelStateError.ErrorMessage);
    }
}