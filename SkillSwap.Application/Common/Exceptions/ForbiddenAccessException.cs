namespace SkillSwap.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException() : base("You are not authorized to perform this operation.") { }
    public ForbiddenAccessException(string message) : base(message) { }
}
