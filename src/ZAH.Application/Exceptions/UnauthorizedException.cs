using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message, string errorCode = ErrorCodes.UNAUTHORIZED)
        : base(message, errorCode, 401)
    {
    }
}
