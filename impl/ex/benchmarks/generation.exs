defmodule Generation do
  def id(), do: Pika.gen("user")
  def snowflake(), do:  Pika.Snowflake.generate()
end

Pika.Snowflake.start_link()

Benchee.run(
  %{
    "Generate IDs" => fn -> Generation.id() end,
    "Generate Snowflakes" => fn -> Generation.snowflake() end
  },
  time: 5
)
