defmodule Pika.Utils do
  @moduledoc false

  defp validate_address(address) do
    case address do
      nil -> :error
      [0, 0, 0, 0, 0, 0] -> :error
      [_, _, _, _, _, _] = addr -> {:ok, addr}
      _ -> :error
    end
  end

  def get_mac_address do
    {:ok, addresses} = :inet.getifaddrs()

    {_if_name, if_mac} =
      Enum.reduce(addresses, [], fn {if_name, if_data}, acc ->
        case Keyword.get(if_data, :hwaddr) |> validate_address do
          {:ok, address} -> [{to_string(if_name), address} | acc]
          _ -> acc
        end
      end)
      |> List.first()

    if_mac
    |> Enum.map_join(":", fn i -> Integer.to_string(i, 16) |> String.pad_leading(2, "0") end)
  end

  @spec compute_node_id() :: integer()
  def compute_node_id do
    {id, _} = get_mac_address() |> String.replace(":", "") |> Integer.parse(16)

    rem(id, 1024)
  end

  def filter_prefixes(prefix, prefixes) do
    Enum.filter(prefixes, fn record -> record.prefix == prefix end)
  end
end
