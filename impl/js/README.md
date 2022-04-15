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

// Define prefix types
// You can specify either a string or an object per prefix
const prefixes = [
  "user",
  {
    prefix: "ch",
    description: "Channels",
  },
];

// Initialize Pika - do this once, then reuse the instance
const pika = new Pika(prefixes, {
  /**
   * Optional initialization parameters:
   * epoch: bigint | number - customize the epoch that IDs are derived from
   * suppressPrefixWarnings: boolean - don't warn on undefined prefixes
   * disableLowercase: boolean - don't require prefixes to be lowercase
   **/
});

// Generate a pika id
pika.gen("user");
// => user_
```
