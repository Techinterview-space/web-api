﻿using System;
using System.Collections.Generic;
using System.Net;
using MG.Utils.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace MG.Utils.AspNetCore.Middlewares.Error
{
    public class ValidationProblemDetails : ProblemDetails
    {
        public const int ValidationStatusCode = (int)HttpStatusCode.BadRequest;

        public ValidationProblemDetails(
            ICollection<ValidationError> validationErrors, string instance)
        {
            validationErrors.ThrowIfNullOrEmpty(nameof(validationErrors));
            ValidationErrors = validationErrors;

            Status = ValidationStatusCode;
            Title = "Request Validation Error";
            Instance = instance;
        }

        public ICollection<ValidationError> ValidationErrors { get; }

        public string RequestId => Guid.NewGuid().ToString();
    }
}