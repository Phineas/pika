defmodule Pika do
  alias Pika.Snowflake
  import Pika.Utils, only: [filter_prefixes: 2]

  @moduledoc nil

  @spec valid_prefix?(binary()) :: boolean()
  defp valid_prefix?(prefix) do
    # Checks if `prefix` is alphanumeric
    Regex.match?(~r/^[0-9a-z_]+$/, prefix)
  end

  @doc false
  defp _gen(prefix, snowflake, nil) do
    {:ok, "#{prefix}_#{Base.encode64(snowflake, padding: false)}"}
  end

  @doc false
  defp _gen(prefix, snowflake, false) do
    {:ok, "#{prefix}_#{Base.encode64(snowflake, padding: false)}"}
  end

  @doc false
  defp _gen(prefix, snowflake, true) do
    bytes = :crypto.strong_rand_bytes(16)

    tail =
      "_s_#{Base.encode32(bytes, padding: false, case: :lower)}_#{snowflake}"

    {:ok, "#{prefix}_#{Base.encode64(tail, padding: false)}"}
  end

  @spec gen(binary()) :: {:error, binary()} | {:ok, binary()}
  @doc """
  Generates an ID given a prefix (which should be configured).

  This function will return an `{:error, binary()}` if one of the follow conditions are met:

  1. The prefix isn't valid
  2. The prefix isn't configured
  """
  def gen(prefix) do
    case valid_prefix?(prefix) do
      true ->
        prefixes = Application.get_env(:pika, :prefixes)

        case filter_prefixes(prefix, prefixes) do
          [prefix_record] ->
            snowflake = Snowflake.generate() |> Integer.to_string()

            _gen(prefix, snowflake, prefix_record[:secure])

          _ ->
            {:error, "Prefix is undefined"}
        end

      _ ->
        {:error, "Prefix is invalid (must be Alphanumeric)"}
    end
  end

  @spec gen!(binary()) :: binary()
  def gen!(prefix) do
    {:ok, id} = gen(prefix)

    id
  end

  @doc """
  Deconstructs a Pika ID and returns it's metadata:

  - prefix
  - tail
  - snowflake
  - timestamp
  - prefix_record
  - epoch
  - node_id
  - seq
  """
  def deconstruct(id) do
    prefixes = Application.get_env(:pika, :prefixes)
    [prefix, tail] = id |> String.split("_")
    [prefix_record] = Enum.filter(prefixes, fn m -> m.prefix == prefix end)

    snowflake =
      Base.decode64!(tail, padding: false)
      |> String.split("_")
      |> List.last()
      |> String.to_integer()

    decoded_snowflake = Snowflake.decode(snowflake)

    Map.merge(decoded_snowflake, %{
      prefix: prefix,
      tail: tail,
      snowflake: snowflake,
      prefix_record: prefix_record
    })
  end
end
