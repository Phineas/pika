# Pika

C# implementation for Pika

## Installation

```bash
dotnet add package Pika
```

## Usage

```csharp
var pika = new PikaGenerator(new[]
{
    new PikaPrefix
    {
        Prefix = "user",
        Description = "User ID"
    },
    new PikaPrefix
    {
        Prefix = "post",
        Description = "Post ID"
    },
    new PikaPrefix
    {
        Prefix = "sk",
        Description = "Secret Key",
        Secure = true
    }
});

var userId = pika.Generate("user");
    // -> user_MjM4NDAxNDk2MTUzODYyMTQ1

var postId = pika.Generate("post");
    // -> post_MjM4NDAxNDk2MTUzODYyMTQ1

var secretKey = pika.Generate("sk");
    // -> sk_c19FMjdGRjMyMjhGNkE0MDdDRDFFMTZEMEY1Mzk1QUVGRl8yMzg0MDE0OTYxNTgwNTY0NTA
```
