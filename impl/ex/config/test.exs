import Config

config :pika,
  # epoch: 1_650_153_600_000,
  prefixes: [
    %{prefix: "user", description: "User IDs"},
    %{prefix: "server", description: "Server IDs", secure: true}
  ]
