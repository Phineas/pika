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

  test "Test 4096+ ids" do
    Enum.map(0..4095, fn s ->
      snowflake = Pika.Snowflake.generate()
      %{"seq" => seq} = Pika.Snowflake.decode(snowflake)

      assert seq == s
    end)

    last_snowflake = Pika.Snowflake.decode()
    %{"seq" => seq} = Pika.Snowflake.deconstruct(last_snowflake)

    assert last_sequence == 0
  end

  test "Validate node_id" do
    id = Pika.gen!("user")
    deconstructed = Pika.deconstruct(id)

    deconstructed.node_id == Pika.Utils.compute_node_id()
  end
end
