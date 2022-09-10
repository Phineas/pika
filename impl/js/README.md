# pika js

Fully typed, 0 dependencies JS implementation of the full Pika specification.

## Install

Yarn:

```
yarn add pika-id
```

npm:

```
npm i pika-id
```

## Basic Usage

```ts
import Pika from "pika-id";

// Initialize Pika - do this once, then reuse the instance
const pika = new Pika(
	// Define prefix types
	// You can specify either a string or an object per prefix
	// Make sure prefixes are lowercase
	[
		"user",
		{
			prefix: "ch",
			description: "Channels",
		},
		{
			prefix: "sk",
			description: "Secret key",
			secure: true, // pika secure id
		},
	],
	{
		/**
		 * Optional initialization parameters:
		 * epoch: bigint | number - customize the epoch (millis) that IDs are derived from - by default, this is 1640995200000 (Jan 1 2022)
		 * nodeId: bigint | number - see below
		 * suppressPrefixWarnings: boolean - don't warn on undefined prefixes
		 * disableLowercase: boolean - don't require prefixes to be lowercase
		 **/
	}
);

// Generate a pika id
pika.gen("user");
// => user_Mzc5ODk1NTI4NzgxMTY4NjQ

// Generate a secure id, as registered above
pika.gen("sk");
// => sk_c19iMGI0NTM4ZjU3ZThjYTIyZThjNjNlMTgwOTg5MWMyM18zODA2NTE5MjcwNDc5NDYyNA
```

## Node IDs

By default, Node IDs are calculated by finding the MAC address of the first public network interface device, then calculating the modulo against 1024.

This works well for smaller systems, but if you have a lot of nodes generating Snowflakes, then collision is possible. In this case, you should create an internal singleton service which keeps a rolling count of the assigned node IDs - from 1 to 1023. Then, services that generate Pikas should call this service to get assigned a node ID.

You can then pass in the node ID when initializing Pika like this:

```ts
const p = new Pika([], { nodeId: customNodeId });
```

## Benchmarks

<small>See [bench/gen.js](https://github.com/hopinc/pika/blob/main/impl/js/bench/gen.js) for benchmark implementation</small>

The benchmark below was ran on a 2021 MacBook Pro 14" with an m1 Pro chip and 16gb of memory.

```
Pika#gen x 1,370,869 ops/sec ±0.19% (100 runs sampled)
Snowflake#gen x 2,015,012 ops/sec ±1.88% (97 runs sampled)
```
