using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Fluffle.Search.Api.Validation;

public static class Error
{
    public static ObjectResult Create(int statusCode, string? code, string message)
    {
        return Create(statusCode, [
            new ErrorModel
            {
                Code = code,
                Message = message
            }
        ]);
    }

    public static ObjectResult Create(int statusCode, ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .Select(x => new ErrorModel
            {
                Code = null,
                Message = x.ErrorMessage
            }).ToList();

        return Create(statusCode, errors);
    }

    public static ObjectResult Create(int statusCode, ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value != null)
            .SelectMany(x => x.Value!.Errors.Select(y => new ErrorModel
            {
                Code = null,
                Message = y.ErrorMessage
            })).ToList();

        return Create(statusCode, errors);
    }

    public static ObjectResult Create(int statusCode, ICollection<ErrorModel> errors)
    {
        var model = new ErrorResponseModel
        {
            Errors = errors
        };

        return new ObjectResult(model)
        {
            StatusCode = statusCode
        };
    }
}
