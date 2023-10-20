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

        // Make sure the prefixes are correct
        Assert.Equal("user", decodedUserId.Prefix);
        Assert.Equal("sk", decodedSecretKey.Prefix);

        // Make sure sequencing is working
        Assert.Equal(1UL, decodedUserId.Seq);
        Assert.Equal(2UL, decodedSecretKey.Seq);
    }

    [Fact]
    public void TestPrefixValidation()
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

        Assert.True(pika.Validate("user_MjM4NDAxNDk2MTUzODYyMTQ1"));
        Assert.True(pika.Validate("sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg0MDE0OTYxNTgwNTY0NTA"));
        Assert.False(pika.Validate("user_MjM4NDAxNDk2MTUzODYyMTQ1", "sk"));
        Assert.False(
            pika.Validate("sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg0MDE0OTYxNTgwNTY0NTA", "user"));
    }
    
    [Fact]
    public void TestInvalidPrefix()
    {
        Assert.Throws<InvalidPrefixError>(() =>
        {
            var pika = new Pika(new[]
            {
                new PikaPrefix
                {
                    Prefix = "USER",
                    Description = "User ID",
                }
            });
        });

        Assert.Throws<InvalidPrefixError>(() =>
        {
            var pika = new Pika(new[]
            {
                new PikaPrefix
                {
                    Prefix = "user69",
                    Description = "User ID",
                }
            });
        });
    }
    
    [Fact]
    public void TestUnregisteredPrefix()
    {
        Assert.Throws<UnregisteredPrefixError>(() =>
        {
            var pika = new Pika(new[]
            {
                new PikaPrefix
                {
                    Prefix = "user",
                    Description = "User ID",
                }
            });
            
            pika.Generate("sk");
        });
    }
}