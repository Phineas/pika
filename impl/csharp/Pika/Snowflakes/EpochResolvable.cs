namespace Pika.Snowflakes;

public class EpochResolvable
{
    private readonly object _value;

    public EpochResolvable(ulong value)
    {
        _value = value;
    }

    public EpochResolvable(DateTime value)
    {
        _value = value;
    }

    public ulong ToULong()
    {
        return _value is DateTime dateTime
            ? (ulong) new DateTimeOffset(dateTime).ToUnixTimeMilliseconds()
            : (ulong) _value;
    }

    public static implicit operator long(EpochResolvable resolvable)
    {
        return (long) resolvable.ToULong();
    }

    public static implicit operator ulong(EpochResolvable resolvable)
    {
        return resolvable.ToULong();
    }

    public static implicit operator DateTime(EpochResolvable resolvable)
    {
        return resolvable._value is DateTime dateTime
            ? dateTime
            : DateTimeOffset.FromUnixTimeMilliseconds((long) resolvable._value).DateTime;
    }
    
    public static implicit operator DateTimeOffset(EpochResolvable resolvable)
    {
        return resolvable._value is DateTime dateTime
            ? new DateTimeOffset(dateTime)
            : DateTimeOffset.FromUnixTimeMilliseconds((long) resolvable._value);
    }
    
    public static implicit operator EpochResolvable(ulong value)
    {
        return new EpochResolvable(value);
    }
}