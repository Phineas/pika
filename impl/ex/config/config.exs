import Config

case config_env() do
  :bench ->
    import_config "bench.exs"
  :docs ->
    :ok
  _ ->
    import_config "test.exs"
end
