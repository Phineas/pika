namespace Pika;

public class PikaPrefix
{
    public required string Prefix { get; init; }
    
    public string? Description { get; set; }
    
    public bool Secure { get; init; }
}