using System;
using System.Net.Http;
using MG.Utils.Abstract;

namespace MG.Utils.AspNetCore.WebResponse
{
    public class WebResponse<T>
    {
        private readonly HttpRequestException _exception;

        private WebResponse(
            T result,
            bool successful,
            FailReason? failReason,
            string failReasonDescription,
            HttpRequestException httpException = null)
        {
            Result = result;
            Successful = successful;
            FailReason = failReason;
            FailReasonDescription = failReasonDescription;
            _exception = httpException;
        }

        public T Result { get; }

        public bool Successful { get; }

        public FailReason? FailReason { get; }

        public string FailReasonDescription { get; }

        public string HttpExceptionMessage => _exception?.Message;

        public static WebResponse<T> Success(T result)
        {
            result.ThrowIfNull(nameof(result));
            return new WebResponse<T>(
                result: result,
                successful: true,
                failReason: null,
                failReasonDescription: null,
                httpException: null);
        }

        public WebResponse<TOther> CopyAs<TOther>()
        {
            if (Successful)
            {
                throw new InvalidOperationException("Could not create failed response from successful one");
            }

            return WebResponse<TOther>.Fail(FailReason!.Value, _exception);
        }

        public static WebResponse<T> Fail(FailReason failReason, HttpRequestException exception = null)
        {
            if (failReason == default)
            {
                throw new ArgumentException("You should not pass default value");
            }

            return new WebResponse<T>(
                result: default,
                successful: false,
                failReason: failReason,
                failReasonDescription: failReason.ToString(),
                httpException: exception);
        }
    }
}