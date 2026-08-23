using ZAH.Shared.Constants;

namespace ZAH.Application.Exceptions;

public class ValidationException : BaseException
{
    public List<string> ValidationErrors { get; }

    public ValidationException(string message, List<string> validationErrors)
        : base(message, ErrorCodes.VALIDATION_ERROR, 400)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string message)
        : base(message, ErrorCodes.VALIDATION_ERROR, 400)
    {
        ValidationErrors = new List<string> { message };
    }
}
