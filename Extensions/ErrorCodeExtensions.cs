using BookingApi.Common;

namespace BookingApi.Extensions;

public static class ErrorCodeExtensions
{
    public static IResult ToHttpResult(this ErrorCode error)
    {
        return error switch
        {
            ErrorCode.ValidationError => Results.BadRequest(),
            ErrorCode.NotFound => Results.NotFound(),
            ErrorCode.Conflict => Results.Conflict(),
            _ => Results.Problem()
        };
    }
        
}