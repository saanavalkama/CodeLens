namespace CodeLens.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string resource)
        : base($"{resource} not found"){}
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message)
        : base(message){}
}

public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message){}
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) 
        : base(message) { }
}

public class BadRequestException: Exception
{
    public BadRequestException(string message) : base(message)
    {
        
    }
}