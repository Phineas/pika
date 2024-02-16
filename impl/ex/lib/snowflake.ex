defmodule Pika.Snowflake do
  import Bitwise
  alias Pika.Utils
  use GenServer

  def start_link(%{"epoch" => epoch}) when is_integer(epoch) do
    GenServer.start_link(__MODULE__, {Utils.compute_node_id(), epoch, 0, 0}, name: __MODULE__)
  end

  def start_link() do
    # State: {node_id, epoch, seq, last_sequence_exhaustion}
    GenServer.start_link(__MODULE__, {Utils.compute_node_id(), 1_640_995_200_000, 0, 0},
      name: __MODULE__
    )
  end

  def init(state) do
    {:ok, state}
  end

  def generate do
    GenServer.call(__MODULE__, :generate)
  end

  def decode(snowflake) do
    GenServer.call(__MODULE__, {:decode, snowflake})
  end

  def handle_call(
        {:decode, snowflake},
        _from,
        state = {_node_id, epoch, _seq, _last_seq_exhaustion}
      ) do
    snowflake = snowflake |> String.to_integer()

    timestamp = (snowflake >>> 22) + epoch
    node_id = snowflake >>> 12 &&& 0b11_1111_1111
    seq = snowflake &&& 0b1111_1111_1111

    {:reply, %{timestamp: timestamp, epoch: epoch, node_id: node_id, seq: seq}, state}
  end

  def handle_call(:generate, _from, {node_id, epoch, seq, last_seq_exhaustion}) do
    now = now_ts()

    if seq >= 4095 and now == last_seq_exhaustion do
      :timer.sleep(1)
    end

    snowflake = (now - epoch) <<< 22 ||| node_id <<< 12 ||| seq

    seq =
      if seq >= 4095 do
        0
      else
        seq + 1
      end

    if now === last_seq_exhaustion do
      {:reply, snowflake, {node_id, epoch, seq, now}}
    else
      {:reply, snowflake, {node_id, epoch, seq, now_ts()}}
    end
  end

  def now_ts do
    System.os_time(:millisecond)
  end
end
