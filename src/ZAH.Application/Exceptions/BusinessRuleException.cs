using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class BusinessRuleException : BaseException
{
    public BusinessRuleException(string message, string errorCode = ErrorCodes.BAD_REQUEST)
        : base(message, errorCode, 422)
    {
    }
}
