namespace Pika;

public class Base64UrlEncoding
{
    public static byte[] Decode(string input)
    {
        var base64 = input.Replace("-", "+").Replace("_", "/");
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }
        return Convert.FromBase64String(base64);
    }

    public static string Encode(byte[] input)
    {
        return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}