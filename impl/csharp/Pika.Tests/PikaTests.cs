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
            }
        });

        var id = pika.Gen("user");
        var decoded = pika.Decode(id);

        Assert.Equal("user", decoded.Prefix);
        Assert.Equal(1UL, decoded.Seq);
    }
}