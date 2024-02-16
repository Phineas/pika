defmodule Pika do
  alias Pika.Snowflake

  @spec is_valid_prefix?(binary()) :: boolean()
  defp is_valid_prefix?(prefix) do
    # Checks if `prefix` is alphanumeric
    Regex.match?(~r/^[0-9A-Za-z]+$/, prefix)
  end

  @spec gen(binary()) :: {:error, binary()} | {:ok, binary()}
  def gen(prefix) do
    case is_valid_prefix?(prefix) do
      false ->
        {:error, "Prefix is invalid (must be Alphanumeric)"}

      true ->
        prefixes = Application.get_env(:pika, :prefixes)

        case Enum.filter(prefixes, fn m -> m.prefix == prefix end) do
          [] ->
            {:error, "Prefix is undefined"}

          [prefix_record] ->
            snowflake = Snowflake.generate() |> Integer.to_string()

            unless prefix_record[:secure] do
              {:ok, "#{prefix}_#{Base.encode64(snowflake, padding: false)}"}
            else
              bytes = :rand.bytes(16)

              tail =
                "s_#{Base.encode32(bytes, padding: false, case: :lower)}_#{Base.encode64(snowflake, padding: false)}"

              {:ok, "#{prefix}_#{Base.encode64(tail, padding: false)}"}
            end
        end
    end
  end

  def gen!(prefix) do
    {:ok, id} = gen(prefix)

    id
  end

  def gen do
    {:error, "No prefix was specified"}
  end

  def deconstruct(id) do
    prefixes = Application.get_env(:pika, :prefixes)

    fragments = id |> String.split("_")
    [prefix, tail] = fragments

    [prefix_record] = Enum.filter(prefixes, fn m -> m.prefix == prefix end)
    IO.puts tail
    decoded_tail = Base.decode64!(tail, padding: false)
    tail_fragments = decoded_tail |> String.split("_")
    snowflake = tail_fragments |> Enum.at(length(tail_fragments) - 1)

    decoded_snowflake = Snowflake.decode(snowflake)

    Map.merge(decoded_snowflake, %{prefix: prefix, tail: tail, snowflake: snowflake, prefix_record: prefix_record})
  end
end
