using SharedKernel;

namespace DirectoryService.Presentation.EndpointResults
{
    public sealed class ErrorsResult : IResult
    {
        private readonly ErrorList _errors;

        public ErrorsResult(ErrorList errors)
        {
            _errors = errors;
        }

        public ErrorsResult(Error error)
        {
            _errors = error.ToErrorList();
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (_errors.Any() == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                return httpContext.Response.WriteAsJsonAsync(Envelope.Error(_errors));
            }

            var distinctTypeErrors = _errors
                .Select(e => e.Type)
                .Distinct()
                .ToList();

            int statusCode = distinctTypeErrors.Count > 1
                ? StatusCodes.Status500InternalServerError
                : GetStatusCodeForErrorType(distinctTypeErrors.First());

            var envelope = Envelope.Error(_errors);

            httpContext.Response.StatusCode = statusCode;

            return httpContext.Response.WriteAsJsonAsync(envelope);
        }

        private static int GetStatusCodeForErrorType(ErrorType errorType) =>
         errorType switch
         {
             ErrorType.VALIDATION => StatusCodes.Status400BadRequest,
             ErrorType.NOT_FOUND => StatusCodes.Status404NotFound,
             ErrorType.CONFLICT => StatusCodes.Status409Conflict,
             ErrorType.FAILURE => StatusCodes.Status500InternalServerError,
             ErrorType.NONE => StatusCodes.Status500InternalServerError,
             _ => StatusCodes.Status500InternalServerError,
         };
    }
}