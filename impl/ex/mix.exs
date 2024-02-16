defmodule Pika.MixProject do
  use Mix.Project

  @source_url "https://github.com/ArtieFuzzz/pika/tree/ex-impl/impl/ex"

  def project do
    [
      app: :pika,
      version: "0.1.0",
      elixir: "~> 1.16",
      start_permanent: Mix.env() == :prod,
      package: package(),
      description: """
      Elixir implementation of hop.io's Pika. Combine Stripe IDs and Snowflakes.
      """,
      docs: docs(),
      deps: deps()
    ]
  end

  # Run "mix help compile.app" to learn about applications.
  def application do
    []
  end

  def docs do
    [
      source_url: @source_url
    ]
  end

  def package do
    [
      files: ["lib", "mix.exs", "README.md"],
      licenses: ["ISC"],
      links: %{
        "GitHub" => @source_url
      }
    ]
  end

  # Run "mix help deps" to learn about dependencies.
  defp deps do
    [
      # {:dep_from_hexpm, "~> 0.3.0"},
      # {:dep_from_git, git: "https://github.com/elixir-lang/my_dep.git", tag: "0.1.0"}
    ]
  end
end
