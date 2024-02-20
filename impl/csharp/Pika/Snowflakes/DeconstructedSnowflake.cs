namespace Pika.Snowflakes;

public class DeconstructedSnowflake
{
    public ulong Id { get; init; }
    
    public ulong Timestamp { get; init; }
    
    public uint NodeId { get; init; }
    
    public ulong Seq { get; init; }
    
    public ulong Epoch { get; init; }
}