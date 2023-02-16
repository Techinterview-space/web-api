using System;
using System.Collections.Generic;
using System.Security.Authentication;
using MG.Utils.Abstract.Exceptions;
using MG.Utils.AspNetCore.Middlewares;
using MG.Utils.Exceptions;
using MG.Utils.Helpers;
using MG.Utils.Validation.Exception;
using Microsoft.AspNetCore.Http;

namespace TechInterviewer.Setup;

public class ExceptionHttpMiddleware : ExceptionHandlerMiddlewareBase
{
    internal static readonly Dictionary<Type, int> StatusCodeTypes = new ()
    {
        { typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized },
        { typeof(AuthenticationException), StatusCodes.Status401Unauthorized },
        { typeof(NoPermissionsException), StatusCodes.Status403Forbidden },
        { typeof(ResourceNotFoundException), StatusCodes.Status404NotFound },
        { typeof(BadRequestException), StatusCodes.Status400BadRequest },
        { typeof(EntityInvalidException), StatusCodes.Status400BadRequest },
        { typeof(InputValidationException), StatusCodes.Status422UnprocessableEntity },
        { typeof(InvalidOperationException), StatusCodes.Status400BadRequest },
        { typeof(DbUpdateConcurrencyException), StatusCodes.Status409Conflict }
    };

    public ExceptionHttpMiddleware(RequestDelegate next)
        : base(next)
    {
    }

    protected override string Serialize<T>(T instance)
    {
        return instance.AsJson();
    }

    protected override Dictionary<Type, int> StatusCodeConversion => StatusCodeTypes;
}