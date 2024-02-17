defmodule PikaTest do
  use ExUnit.Case
  doctest Pika

  setup do
    Pika.Snowflake.start_link()

    :ok
  end

  test "Generate an ID" do
    id = Pika.gen!("user")

    assert String.starts_with?(id, "user")
  end

  test "Generate a secure ID" do
    id = Pika.gen!("server")

    assert String.starts_with?(id, "server")
  end

  test "Fail to generate ID" do
    {status, _message} = Pika.gen("not_found")

    assert status == :error
  end

  test "Fail to validate ID" do
    {:error, message} = Pika.gen("!!!")

    assert message == "Prefix is invalid (must be Alphanumeric)"
  end

  test "Snowflake custom timestamp" do
    timestamp = 1_708_158_291_035
    snowflake = Pika.Snowflake.generate(timestamp)
    decoded = Pika.Snowflake.decode(snowflake)

    assert decoded.timestamp == timestamp
  end

  test "Test 4096+ ids" do
    Enum.map(0..4095, fn s ->
      id = Pika.gen!("user")
      deconstructed = Pika.deconstruct(id)

      assert deconstructed.seq == s
    end)

    last_id = Pika.gen!("user")
    deconstructed = Pika.deconstruct(last_id)

    assert deconstructed.seq == 0
  end

  test "Validate node_id" do
    id = Pika.gen!("user")
    deconstructed = Pika.deconstruct(id)

    assert deconstructed.node_id == Pika.Utils.compute_node_id()
  end
end
