namespace Core.Exceptions;

// a business rule was broken (e.g. editing a shipped order)
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }
}
