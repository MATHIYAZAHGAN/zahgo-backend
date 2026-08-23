using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class ConflictException : BaseException
{
    public ConflictException(string message, string errorCode = ErrorCodes.BAD_REQUEST)
        : base(message, errorCode, 409)
    {
    }
}
