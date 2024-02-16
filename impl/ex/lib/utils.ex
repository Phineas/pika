defmodule Pika.Utils do
  def get_mac_address do
    {:ok, addresses} = :inet.getifaddrs()

    addresses
    |> Enum.filter(fn {name, _opts} -> name != "lo" end)
    |> Enum.map(fn {_name, data} -> data[:hwaddr] end)
    |> List.first()
    |> Enum.map(&Integer.to_string(&1, 16))
    |> Enum.join(":")
  end

  @spec compute_node_id() :: integer()
  def compute_node_id do
    {id, _} = get_mac_address() |> String.replace(":", "") |> Integer.parse(16)

    rem(id, 1024)
  end
end
