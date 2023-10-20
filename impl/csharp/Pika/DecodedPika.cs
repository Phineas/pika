namespace Pika;

public class DecodedPika
{
    public required string Prefix { get; set; }

    public required string Tail { get; set; }

    public ulong Snowflake { get; set; }

    public uint NodeId { get; set; }

    public ulong Seq { get; set; }

    public byte Version { get; set; }
    
    public ulong Timestamp { get; set; }

    public required PikaPrefix PrefixRecord { get; set; }
}