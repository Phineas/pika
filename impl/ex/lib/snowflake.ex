defmodule Pika.DecodedSnowflake do
  @moduledoc """
  Represents a decoded `Snowflake`
  """
  defstruct [:timestamp, :node_id, :seq, :epoch]
end

defmodule Pika.Snowflake do
  import Bitwise
  alias Pika.Utils
  use GenServer

  @moduledoc """
  `Pika.Snowflake` holds the state, generates Snowflakes, and decodes Snowflakes.

  `Pika.Snowflake` should be started under a `Supervisor` or `Application` before you start using
  `Pika.gen/1` or `Pika.deconstruct/1`

  ```elixir
  defmodule MyApp.Application do
    use Application

    def start(_type, _args) do
      children = [Pika.Snowflake]

      Supervisor.start_link(children, strategy: :one_for_one)
    end
  end
  ```

  or manually in `iex`

  ```elixir
  iex(1)> Pika.Snowflake.start_link()
  {:ok, #PID<0.190.0>}
  ```

  ## Custom epoch

  You can start `Pika.Snowflake` with a custom epoch by passing it:

  ```elixir
  Pika.Snowflake.start_link(1_650_153_600_000)
  ```
  """

  def start_link([]), do: start_link()

  def start_link(epoch) when is_integer(epoch) do
    GenServer.start_link(__MODULE__, epoch, name: __MODULE__)
  end

  def start_link do
    GenServer.start_link(__MODULE__, nil, name: __MODULE__)
  end

  def init(epoch) do
    {:ok, generate_initial_state(epoch)}
  end

  def handle_call(
        {:decode, snowflake},
        _from,
        state = {_node_id, epoch, _seq, _last_seq_exhaustion}
      ) do
    timestamp = (snowflake >>> 22) + epoch
    node_id = snowflake >>> 12 &&& 0b11_1111_1111
    seq = snowflake &&& 0b1111_1111_1111

    {:reply,
     %Pika.DecodedSnowflake{timestamp: timestamp, epoch: epoch, node_id: node_id, seq: seq},
     state}
  end

  def handle_call({:generate, timestamp}, _from, {node_id, epoch, seq, last_seq_exhaustion}) do
    if seq >= 4095 and timestamp == last_seq_exhaustion do
      block(timestamp)
    end

    snowflake = (timestamp - epoch) <<< 22 ||| node_id <<< 12 ||| seq

    seq =
      if seq >= 4095 do
        0
      else
        seq + 1
      end

    {:reply, snowflake,
     {node_id, epoch, seq, maybe_sequence_exhausted(seq, timestamp, last_seq_exhaustion)}}
  end

  @doc """
  Generates a new Snowflake
  """
  @spec generate() :: integer()
  def generate do
    GenServer.call(__MODULE__, {:generate, now_ts()})
  end

  @doc """
  Generates a new Snowflake with the given `timestamp`
  """
  @spec generate(integer()) :: integer()
  def generate(timestamp) do
    GenServer.call(__MODULE__, {:generate, timestamp})
  end

  @doc """
  Decodes a Snowflake and returns:

  - timestamp
  - epoch
  - node_id
  - seq
  """
  @spec decode(integer()) :: any()
  def decode(snowflake) when is_integer(snowflake) do
    GenServer.call(__MODULE__, {:decode, snowflake})
  end

  @doc false
  defp maybe_sequence_exhausted(4095, timestamp, _last_seq_exhaustion), do: timestamp

  defp maybe_sequence_exhausted(_sequence, _timestamp, last_seq_exhaustion),
    do: last_seq_exhaustion

  @doc false
  defp generate_initial_state(nil), do: generate_initial_state(1_640_995_200_000)
  defp generate_initial_state(epoch), do: {Utils.compute_node_id(), epoch, 0, 0}

  @doc false
  defp block(timestamp) do
    if now_ts() - timestamp < 1 do
      :timer.sleep(100)
      block(timestamp)
    end
  end

  @doc "Returns the current timestamp in milliseconds."
  def now_ts do
    System.os_time(:millisecond)
  end
end
