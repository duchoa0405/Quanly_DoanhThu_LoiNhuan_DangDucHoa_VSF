namespace FashionWeb.Business.Exceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException(string message) : base(message) { }
    protected BusinessException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : ArgumentException
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : KeyNotFoundException
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : InvalidOperationException
{
    public ConflictException(string message) : base(message) { }
}

public class BusinessRuleException : InvalidOperationException
{
    public BusinessRuleException(string message) : base(message) { }
}
