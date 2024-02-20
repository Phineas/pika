namespace Pika.Tests;

public class PikaTests
{
    [Fact]
    public void Pika_GenerateAndDecode_ValidUserAndSecretKey()
    {
        var pika = new PikaGenerator(new[]
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
    public void Pika_GenerateAndDecode_ValidUserWithUnderscore()
    {
        var pika = new PikaGenerator(new[]
        {
            new PikaPrefix
            {
                Prefix = "user_id",
                Description = "User ID",
            }
        });

        // user_id_MjM4NDAxNDk2MTUzODYyMTQ1
        var userId = pika.Generate("user_id");

        var decodedUserId = pika.Decode(userId);

        // Make sure the prefixes are correct
        Assert.Equal("user_id", decodedUserId.Prefix);

        // Make sure sequencing is working
        Assert.Equal(1UL, decodedUserId.Seq);
    }

    [Fact]
    public void Pika_Uniqueness()
    {
        var pika = new PikaGenerator(new[]
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

        var ids = new HashSet<string>();

        for (var i = 0; i < 10000; i++)
        {
            var id = pika.Generate("user");
            Assert.DoesNotContain(id, ids);
            ids.Add(id);
        }

        ids.Clear();

        for (var i = 0; i < 10000; i++)
        {
            var id = pika.Generate("sk");
            Assert.DoesNotContain(id, ids);
            ids.Add(id);
        }

        ids.Clear();
    }

    [Fact]
    public void Pika_Validation_ValidUserPrefix()
    {
        var pika = new PikaGenerator(new[]
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
    }

    [Fact]
    public void Pika_Validation_ValidSecureKeyPrefix()
    {
        var pika = new PikaGenerator(new[]
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

        Assert.True(pika.Validate("sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg4MDE0OTYxNTgwNTY0NTA"));
    }

    [Fact]
    public void Pika_Validation_InvalidUserPrefix()
    {
        var pika = new PikaGenerator(new[]
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

        Assert.False(pika.Validate("user_MjM4NDAxNDk2MTUzODYyMTQ1", "sk"));
    }

    [Fact]
    public void Pika_Validation_InvalidSecureKeyPrefix()
    {
        var pika = new PikaGenerator(new[]
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

        Assert.False(
            pika.Validate("sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg4MDE0OTYxNTgwNTY0NTA", "user"));
    }

    [Fact]
    public void Pika_InvalidPrefix_UppercaseCharacters()
    {
        Assert.Throws<InvalidPrefixError>(() =>
        {
            var pika = new PikaGenerator(new[]
            {
                new PikaPrefix
                {
                    Prefix = "USER",
                    Description = "User ID",
                }
            });
        });
    }

    [Fact]
    public void Pika_ValidPrefix_UnderscoreCharacters()
    {
        var pika = new PikaGenerator(new[]
        {
            new PikaPrefix
            {
                Prefix = "user_id",
                Description = "User ID",
            }
        });
    }

    [Fact]
    public void Pika_InvalidPrefix_NumericCharacters()
    {
        Assert.Throws<InvalidPrefixError>(() =>
        {
            var pika = new PikaGenerator(new[]
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
    public void Pika_UnregisteredPrefix_Generate()
    {
        Assert.Throws<UnregisteredPrefixError>(() =>
        {
            var pika = new PikaGenerator(new[]
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