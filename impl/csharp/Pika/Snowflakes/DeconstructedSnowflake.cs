namespace Pika.Snowflakes;

public class DeconstructedSnowflake
{
    public ulong Id { get; set; }
    
    public ulong Timestamp { get; set; }
    
    public uint NodeId { get; set; }
    
    public ulong Seq { get; set; }
    
    public ulong Epoch { get; set; }
}