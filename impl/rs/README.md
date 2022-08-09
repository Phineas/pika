# pika rs

Rust implementation for Pika

## Install

```toml
[dependencies]
pika = "0.1.0"
```

## Basic Usage

```rs
let prefixes = [
    PrefixRecord {
        prefix: "user".to_string(),
        description: Some("User ID".to_string()),
        secure: false,
    },
    PrefixRecord {
        prefix: "sk".to_string(),
        description: Some("Secret Key".to_string()),
        secure: true,
    }
];

let mut pika = Pika::new(
    prefixes.to_vec(),
    InitOptions {
        epoch: Some(1_650_153_600_000),
        node_id: None,
        disable_lowercase: Some(true),
    },
);

pika.gen("user").unwrap();
    // => user_Mzc5ODk1NTI4NzgxMTY4NjQ

pika.gen("sk").unwrap()
    // => sk_c19iMGI0NTM4ZjU3ZThjYTIyZThjNjNlMTgwOTg5MWMyM18zODA2NTE5MjcwNDc5NDYyNA
```

## Node IDs

By default, Node IDs are calculated by finding the MAC address of the first public network interface device, then calculating the modulo against 1024.

This works well for smaller systems, but if you have a lot of nodes generating Snowflakes, then collision is possible. In this case, you should create an internal singleton service which keeps a rolling count of the assigned node IDs - from 1 to 1023. Then, services that generate Pikas should call this service to get assigned a node ID.

You can then pass in the node ID when initializing Pika like this:

```rs
let mut pika = Pika::new(
    prefixes.to_vec(),
    InitOptions {
        epoch: Some(1_650_153_600_000),
        node_id: custom_node_id,
        disable_lowercase: Some(true),
    },
);
```
