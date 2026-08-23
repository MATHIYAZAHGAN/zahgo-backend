using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class ForbiddenException : BaseException
{
    public ForbiddenException(string message, string errorCode = ErrorCodes.FORBIDDEN)
        : base(message, errorCode, 403)
    {
    }
}
