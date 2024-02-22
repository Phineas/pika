defmodule Generation do
  def id(), do: Pika.gen("user")
  def id_secure(), do: Pika.gen("server")
  def snowflake(), do:  Pika.Snowflake.generate()
end

Pika.Snowflake.start_link()

Benchee.run(
  %{
    "Generate IDs" => fn -> Generation.id() end,
    "Generate Secure IDs" => fn -> Generation.id_secure() end,
    "Generate Snowflakes" => fn -> Generation.snowflake() end
  },
  time: 5
)
