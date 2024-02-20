namespace Pika.Snowflakes;

public class Snowflake
{
    private readonly ulong _epoch;
    private ulong _seq;
    private long _lastSequenceExhaustion;

    public Snowflake(ulong epoch, ulong nodeId)
    {
        _epoch = NormalizeEpoch(epoch);
        NodeId = nodeId;
    }

    public ulong NodeId { get; }

    public ulong Generate(SnowflakeGenOptions? options = null)
    {
        options ??= new SnowflakeGenOptions();
        var timestamp = options.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (timestamp < _lastSequenceExhaustion)
        {
            throw new SequenceExhaustionError();
        }

        _lastSequenceExhaustion = timestamp;
        if (timestamp == _lastSequenceExhaustion)
        {
            _seq++;
            if (_seq > PikaConstants.SequenceMask)
            {
                throw new SequenceExhaustionError();
            }
        }
        else
        {
            _seq = 0;
        }

        var result = ((timestamp - (long) _epoch) << 22) | (long) (NodeId << 12) | (long) _seq;
        return (ulong) result;
    }

    public DeconstructedSnowflake Decode(ulong id)
    {
        return new DeconstructedSnowflake
        {
            Id = id,
            Timestamp = (id >> 22) + _epoch,
            NodeId = (uint) ((id >> 12) & 0x3FF),
            Seq = (uint) (id & 0xFFF),
            Epoch = _epoch,
        };
    }

    private static ulong NormalizeEpoch(EpochResolvable epoch)
    {
        var timestamp = epoch.ToULong();
        if (timestamp < 1420070400000)
        {
            throw new Exception("Epoch cannot be before 2015-01-01T00:00:00.000Z");
        }

        return timestamp;
    }
}