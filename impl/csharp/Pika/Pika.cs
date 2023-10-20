using System.Security.Cryptography;
using System.Text;
using Pika.Snowflakes;

namespace Pika;

public class Pika
{
    public Dictionary<string, PikaPrefix> Prefixes { get; set; } = new();

    private readonly Snowflake _snowflake;
    private readonly bool _suppressPrefixWarnings;

    public Pika(IEnumerable<PikaPrefix> prefixes, PikaInitializationOptions? options = null)
    {
        options ??= new PikaInitializationOptions();
        var nodeId = options.NodeId.HasValue ? (ulong) (options.NodeId.Value % (int) 1024ul) : ComputeNodeId();
        _snowflake = new Snowflake(options.Epoch ?? PikaConstants.DefaultEpoch, nodeId);
        _suppressPrefixWarnings = options.SuppressPrefixWarnings;
        foreach (var definition in prefixes)
        {
            if (!PikaConstants.ValidPrefixRegex().IsMatch(definition.Prefix))
            {
                throw new InvalidPrefixError(definition.Prefix);
            }

            Prefixes[definition.Prefix] = definition;
        }
    }

    public bool Validate(string? maybeId, string? expectPrefix = default)
    {
        if (maybeId == null)
        {
            return false;
        }

        var s = maybeId.Split('_');
        var tail = s[^1];
        var prefix = string.Join("_", s, 0, s.Length - 1);
        if (string.IsNullOrEmpty(tail))
        {
            return false;
        }

        if (expectPrefix != null && prefix != expectPrefix)
        {
            return false;
        }

        if (expectPrefix != null)
        {
            return prefix == expectPrefix;
        }

        return Prefixes.ContainsKey(prefix);
    }

    public string Gen(string prefix)
    {
        if (!PikaConstants.ValidPrefixRegex().IsMatch(prefix))
        {
            throw new InvalidPrefixError(prefix);
        }

        if (!Prefixes.ContainsKey(prefix) && !_suppressPrefixWarnings)
        {
            Console.WriteLine(
                $"Unregistered prefix ({prefix}) was used. This can cause unknown behavior - see https://github.com/hopinc/pika/tree/main/impl/js for details.");
        }

        var snowflake = _snowflake.Gen();
        var securePrefix = Prefixes[prefix].Secure ? $"s_{RandomHexString(16)}_" : "";
        return
            $"{prefix.ToLower()}_{Convert.ToBase64String(Encoding.UTF8.GetBytes(securePrefix + snowflake))}";
    }

    private string RandomHexString(int length)
    {
        var bytes = new byte[length];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes.ToHexString();
    }

    public ulong GenSnowflake()
    {
        return _snowflake.Gen();
    }

    public DecodedPika Decode(string id)
    {
        try
        {
            var s = id.Split('_');
            var tail = s[^1];
            var prefix = string.Join("_", s, 0, s.Length - 1);
            var decodedTail = Encoding.UTF8.GetString(Convert.FromBase64String(tail));
            var sfParts = decodedTail.Split('_');
            if (sfParts.Length == 0)
            {
                throw new Exception("attempted to decode invalid pika; tail was corrupt");
            }

            var snowflake = ulong.Parse(sfParts[^1]);
            var deconstructed = _snowflake.Decode(snowflake);
            return new DecodedPika
            {
                Prefix = prefix,
                Tail = tail,
                PrefixRecord = Prefixes[prefix],
                Snowflake = snowflake,
                Version = 1,
                NodeId = deconstructed.NodeId,
                Seq = deconstructed.Seq
            };
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to decode ID " + id);
            throw;
        }
    }

    private ulong ComputeNodeId()
    {
        try
        {
            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.GetPhysicalAddress().ToString() == "00:00:00:00:00:00") continue;
                var mac = networkInterface.GetPhysicalAddress().ToString();
                return (ulong) (ulong.Parse(mac, System.Globalization.NumberStyles.HexNumber) % 1024);
            }

            throw new Exception("no valid mac address found");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to compute node ID, falling back to 0. Error:\n" + e);
            return 0ul;
        }
    }
}