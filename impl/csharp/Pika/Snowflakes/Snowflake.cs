namespace Pika.Snowflakes;

public class Snowflake
{
    private readonly ulong _epoch;
    private readonly ulong _nodeId;
    private ulong _seq;
    private long _lastSequenceExhaustion;

    public Snowflake(ulong epoch, ulong nodeId)
    {
        _epoch = NormalizeEpoch(epoch);
        _nodeId = nodeId;
    }

    public ulong NodeId => _nodeId;

    public ulong Gen(SnowflakeGenOptions? options = null)
    {
        var timestamp = options?.Timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (_seq == 4095 && timestamp == _lastSequenceExhaustion)
        {
            // Purposely blocking
            while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp < 1)
            {
                // Do nothing
            }
        }

        _seq = _seq >= 4095 ? 0 : _seq + 1;
        if (_seq == 4095)
        {
            _lastSequenceExhaustion = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        var snowflake = ((timestamp - (long) _epoch) << 22) |
                        (long) ((_nodeId & 0x3FF) << 12) |
                        (long) _seq;

        return (ulong) snowflake;
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

    private ulong NormalizeEpoch(EpochResolvable epoch)
    {
        var timestamp = epoch.ToULong();
        if (timestamp < 1420070400000)
        {
            throw new Exception("Epoch cannot be before 2015-01-01T00:00:00.000Z");
        }

        return (ulong) timestamp;
    }
}