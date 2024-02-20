namespace Pika;

public class InvalidPrefixError : Exception
{
    public InvalidPrefixError(string prefix) : base($"The prefix \"{prefix}\" is invalid.")
    {
    }
}