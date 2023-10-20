namespace Pika.Tests;

public class PikaTests
{
    [Fact]
    public void TestPika()
    {
        var pika = new Pika(new[]
        {
            new PikaPrefix
            {
                Prefix = "user",
                Description = "User ID",
            },
            new PikaPrefix
            {
                Prefix = "sk",
                Description = "Secret key",
                Secure = true
            }
        });

        // user_MjM4NDAxNDk2MTUzODYyMTQ1
        var userId = pika.Generate("user");

        // sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg0MDE0OTYxNTgwNTY0NTA
        var secretKey = pika.Generate("sk");

        var decodedUserId = pika.Decode(userId);
        var decodedSecretKey = pika.Decode(secretKey);

        Assert.Equal("user", decodedUserId.Prefix);
        Assert.Equal("sk", decodedSecretKey.Prefix);
    }
}