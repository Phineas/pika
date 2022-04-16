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
