using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message, string errorCode = ErrorCodes.NOT_FOUND)
        : base(message, errorCode, 404)
    {
    }
}
