namespace Pika.Snowflakes;

public class SequenceExhaustionError : Exception
{
    public SequenceExhaustionError() : base("Sequence exhausted for this millisecond.")
    {
    }
}