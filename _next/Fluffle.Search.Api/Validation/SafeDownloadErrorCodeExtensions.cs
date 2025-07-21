using Fluffle.Search.Api.SearchByUrl;
using Microsoft.AspNetCore.Mvc;

namespace Fluffle.Search.Api.Validation;

public static class SafeDownloadErrorCodeExtensions
{
    public static ObjectResult AsResult(this SafeDownloadErrorCode? code)
    {
        if (code is SafeDownloadErrorCode.Unparsable
            or SafeDownloadErrorCode.InvalidScheme)
        {
            return Error.Create(
                statusCode: 400,
                code: null,
                message: "The provided URL isn't formatted correctly."
            );
        }

        if (code is SafeDownloadErrorCode.HostNotFound
            or SafeDownloadErrorCode.NoIpAddresses
            or SafeDownloadErrorCode.NoPublicIpAddresses)
        {
            return Error.Create(
                statusCode: 400,
                code: null,
                message: "The domain specified in the URL is invalid."
            );
        }

        if (code is SafeDownloadErrorCode.NonSuccessStatusCode)
        {
            return Error.Create(
                statusCode: 400,
                code: null,
                message: "The remote server responded with an error."
            );
        }

        if (code is SafeDownloadErrorCode.FileTooBig)
        {
            return Error.Create(
                statusCode: 400,
                code: null,
                message: "The file is over Fluffle's 4 MiB limit."
            );
        }

        throw new ArgumentException("Cannot map error code to response.", nameof(code));
    }
}
