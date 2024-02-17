import Config

config :benchee,
  fast_warning: false

config :pika,
  prefixes: [
    %{prefix: "user", description: "User IDs"},
    %{prefix: "server", description: "Server IDs", secure: true}
  ]
