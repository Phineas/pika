namespace Pika;

public class UnregisteredPrefixError : Exception
{
    public UnregisteredPrefixError(string prefix) : base(
        $"Unregistered prefix ({prefix}) was used, but the prefix was not registered.")
    {
    }
}