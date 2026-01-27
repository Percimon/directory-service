using System.Reflection;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using SharedService.SharedKernel;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace SharedService.Framework.EndpointResults;

public sealed class EndpointResult<TValue> : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndpointResult(Result<TValue, Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult<TValue>(result.Value)
            : new ErrorsResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult<TValue>(Result<TValue, Error> result) => new(result);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);

        ArgumentNullException.ThrowIfNull(builder);

        int[] statusCodes = new[]
        {
            200,
            400,
            401,
            403,
            404,
            409,
            500,
        };

        Array.ForEach(statusCodes, c => builder.Metadata.Add(new ProducesResponseTypeMetadata(c, typeof(Envelope<TValue>), ["application/json"])));
    }
}

public sealed class EndpointResult : IResult, IEndpointMetadataProvider
{
    private readonly IResult _result;

    public EndpointResult(UnitResult<Error> result)
    {
        _result = result.IsSuccess
            ? new SuccessResult()
            : new ErrorsResult(result.Error);
    }

    public Task ExecuteAsync(HttpContext httpContext) => _result.ExecuteAsync(httpContext);

    public static implicit operator EndpointResult(UnitResult<Error> result) => new(result);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(method);

        ArgumentNullException.ThrowIfNull(builder);

        int[] statusCodes = new[]
        {
            200,
            400,
            401,
            403,
            404,
            409,
            500,
        };

        Array.ForEach(statusCodes, c => builder.Metadata.Add(new ProducesResponseTypeMetadata(c, typeof(Envelope), ["application/json"])));
    }
}