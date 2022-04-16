# pika

Combine Stripe IDs with Snowflake IDs and you get... pika! - the last ID system you'll ever need, combining pragmatism with functionality.

Example ID: `user_MTI5Njg4Njg1MDQwODg5ODYx`

## Features

- Object type prefixes
- Type prefix atomicity
- Guaranteed multi-node space-time uniqueness
- Timestamped
- Fast & simple
- Shorter than UUIDs
- Standalone
- Option to be cryptographically secure

## Disadvantages / Trade-offs vs. Snowflakes

- Unable to sequence by integer (pikas are strings)
- Slower generation (by a few nanoseconds - pika is 1m ops/sec vs snowflake 1.6m ops/sec)
- Slightly larger sizes (pikas are ~28 bytes vs Snowflake's 8 bytes)

## The ID

Pika IDs consist of 2 sections: the type prefix and the tail, separated by an underscore.

### Type Prefixes

When creating a pika ID, you must specify the prefix to be prepended - the general rule of thumb should be to use a different prefix for each object type (e.g. `user`, `team`, `post`, etc).

Type prefixes should be lowercase, short, alphanumeric strings. If you have an object type with a long name, then it's recommended to shorten it down into an acronym or similar. For example, if we had an object type called "connected account", then we'd make the type prefix `ca` - or even if we had a type called "channel", we might want to shorten it down to `ch` - it's up to you to decide what you think makes the most distinctive sense.

### Tail

The tail is the part that comes after the underscore (e.g. `MTI5Njg4Njg1MDQwODg5ODYx`). Usually, this is just a base64-encoded Snowflake ID, however, if the pika is cryptographically secure, then the base64 decoded string value will start with an `s_` prefix, followed by the Snowflake ID, then followed by another underscore and a cryptographically random string.

Example of a normal decoded tail:
`129688685040889861`

Example of a cryptographically secure decoded tail:
`s_387d0775128c383fa8fbf5fd9863b84aba216bcc6872a877_1296886850408898617`

## Type Prefix Atomicity

To guarantee that developers use the correct pre-defined prefix types for the right object types, pika requires you to "register" them before they're used to prevent warnings from being thrown. This is also where you define if a prefix type should be cryptographically secure or not.
