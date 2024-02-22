# pika

> Elixir implementation of Pika

Combine Stripe IDs with Snowflakes you get Pika! The last ID system you'll ever need!
Combining pragmatism with functionality

## Features

- Written in pure Elixir
- Zero dependencies

## Installation

The package can be installed by adding `pika` to your list of dependencies in `mix.exs`:

```elixir
def deps do
  [
    {:pika, "~> 0.1"}
  ]
end
```

In your `config.exs`:

```elixir
config :pika,
  prefixes: [
    %{prefix: "user", description: "User IDs"},
    %{prefix: "server", description: "Server IDs", secure: true},
    # ...
  ]
```

## Example

`Pika.Snowflake` should be started under a `Supervisor` or `Application` before you start using
`Pika.gen/1` or `Pika.deconstruct/1`

```elixir
defmodule MyApp.Application do
  use Application

  def start(_type, _args) do
    children = [Pika.Snowflake]

    Supervisor.start_link(children, strategy: :one_for_one)
  end
end
```

Somewhere in your application:

```elixir
# ...
Pika.gen("user") # or Pika.gen!("user")

{:ok, "user_MjgyNDQ2NjY1OTk3MjEzNjk3"}
```
