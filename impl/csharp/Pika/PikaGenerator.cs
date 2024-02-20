using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Pika.Snowflakes;

namespace Pika;

public class PikaGenerator
{
    public Dictionary<string, PikaPrefix> Prefixes { get; set; } = new();

    private readonly Snowflake _snowflake;

    public PikaGenerator(IEnumerable<PikaPrefix> prefixes, PikaInitializationOptions? options = null)
    {
        options ??= new PikaInitializationOptions();
        var nodeId = options.NodeId.HasValue ? (ulong) (options.NodeId.Value % (int) 1024ul) : ComputeNodeId();
        _snowflake = new Snowflake(options.Epoch ?? PikaConstants.DefaultEpoch, nodeId);
        foreach (var definition in prefixes)
        {
            if (!options.DangerouslyDisablePrefixValidation)
            {
                if (!ValidatePrefix(definition.Prefix))
                {
                    throw new InvalidPrefixError(definition.Prefix);
                }
            }

            Prefixes[definition.Prefix] = definition;
        }
    }

    public bool Validate(string id, string? expectPrefix = default)
    {
        var s = id.Split('_');
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

    public string Generate(string prefix)
    {
        if (!Prefixes.ContainsKey(prefix))
        {
            throw new UnregisteredPrefixError(prefix);
        }

        var snowflake = _snowflake.Generate();
        var secure = Prefixes[prefix].Secure;
        var tail = secure
            ? Encoding.UTF8.GetBytes($"s_{RandomHexString(16)}_{snowflake}")
            : Encoding.UTF8.GetBytes(snowflake.ToString());

        return $"{prefix}_{Base64UrlEncoding.Encode(tail)}";
    }

    private string RandomHexString(int length)
    {
        var bytes = new byte[length];
        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes.ToHexString();
    }

    public ulong GenerateSnowflake()
    {
        return _snowflake.Generate();
    }

    public DecodedPika Decode(string id)
    {
        try
        {
            var split = id.Split('_');
            var tail = split[^1];
            var prefix = string.Join("_", split, 0, split.Length - 1);

            var decodedTail = Base64UrlEncoding.Decode(tail);
            var tailString = Encoding.UTF8.GetString(decodedTail);
            var snowflake = ulong.Parse(tailString.Split('_')[^1]);

            var deconstructed = _snowflake.Decode(snowflake);

            return new DecodedPika
            {
                Prefix = prefix,
                Tail = tail,
                PrefixRecord = Prefixes[prefix],
                Snowflake = snowflake,
                Version = 1,
                NodeId = deconstructed.NodeId,
                Seq = deconstructed.Seq,
                Timestamp = deconstructed.Timestamp
            };
        }
        catch (Exception)
        {
            Console.WriteLine("Failed to decode ID " + id);
            throw;
        }
    }

    private static bool ValidatePrefix(string prefix)
    {
        return Regex.IsMatch(prefix, "^[a-z]+(_[a-z]+)*$");
    }

    private static ulong ComputeNodeId()
    {
        try
        {
            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.GetPhysicalAddress().ToString() == "00:00:00:00:00:00") continue;
                var mac = networkInterface.GetPhysicalAddress().ToString();
                return ulong.Parse(mac, System.Globalization.NumberStyles.HexNumber) % 1024;
            }

            throw new Exception("No network interfaces found");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to compute node ID, falling back to 0. Error:\n" + e);
            return 0UL;
        }
    }
}