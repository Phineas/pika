namespace Pika;

public class PikaInitializationOptions
{
    public ulong? Epoch { get; set; }

    public int? NodeId { get; set; }

    public bool DangerouslyDisablePrefixValidation { get; set; }
}