import Config

case config_env() do
  :docs ->
    :ok

  _ ->
    import_config "#{Mix.env()}.exs"
end
