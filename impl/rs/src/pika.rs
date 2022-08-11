use std::io::Error;

use regex::Regex;

use crate::snowflake::{self, Snowflake};

#[derive(Clone, Debug)]
pub struct PrefixRecord {
    pub prefix: String,
    pub description: Option<String>,
    pub secure: bool,
}

#[derive(Debug)]
pub struct DecodedPika {
    pub prefix: String,
    pub tail: String,
    pub snowflake: u64,
    pub node_id: u32,
    pub timestamp: u64,
    pub epoch: u64,
    pub seq: u64,
    pub version: i8,
    pub prefix_record: PrefixRecord,
}

#[derive(Clone)]
pub struct Pika {
    pub prefixes: Vec<PrefixRecord>,
    pub epoch: u64,
    pub node_id: u32,
    pub disable_lowercase: Option<bool>,
    snowflake: Snowflake,
}

pub struct InitOptions {
    pub epoch: Option<u64>,
    pub node_id: Option<u32>,
    pub disable_lowercase: Option<bool>,
}

pub const DEFAULT_EPOCH: u64 = 1_640_995_200_000;

impl Pika {
    pub fn new(prefixes: Vec<PrefixRecord>, options: &InitOptions) -> Pika {
        let epoch = match options.epoch {
            Some(epoch) => epoch,
            None => DEFAULT_EPOCH,
        };

        let node_id = match options.node_id {
            Some(node_id) => node_id,
            None => Self::compute_node_id(),
        };

        Pika {
            prefixes,
            epoch,
            node_id,
            disable_lowercase: options.disable_lowercase,
            snowflake: snowflake::Snowflake::new_with_nodeid(epoch, node_id),
        }
    }

    #[allow(clippy::cast_possible_truncation)]
    fn compute_node_id() -> u32 {
        let res = mac_address::get_mac_address().unwrap();
        let first_mac = res.unwrap().to_string();

        let first_mac = u64::from_str_radix(&first_mac.replace(':', ""), 16).unwrap();

        // should lower the chance of collisions
        (first_mac % 1024) as u32
    }

    pub fn deconstruct(&self, id: &str) -> DecodedPika {
        let s = id.split('_').collect::<Vec<&str>>();
        let prefix = s[0].to_string();
        let tail = s[1].to_string();

        let prefix_record = self.prefixes.iter().find(|x| x.prefix == prefix);
        let decoded_tail = base64::decode(&tail).unwrap();

        let snowflake = self
            .snowflake
            .decode(&String::from_utf8_lossy(&decoded_tail).to_string());
        let stringified_tail = String::from_utf8_lossy(&decoded_tail).to_string();

        DecodedPika {
            prefix,
            tail,
            snowflake: stringified_tail.parse::<u64>().unwrap(),
            node_id: self.node_id,
            timestamp: snowflake.timestamp,
            epoch: self.epoch,
            seq: snowflake.seq,
            version: 1,
            prefix_record: prefix_record.unwrap().clone(),
        }
    }

    pub fn gen(&mut self, prefix: &str) -> Result<String, Error> {
        let valid_prefix: Regex = Regex::new(r"^[a-zA-Z0-9]{1,32}$").unwrap();

        assert!(valid_prefix.is_match(prefix), "Invalid prefix: {}", prefix);

        let prefix_record = self.prefixes.iter().find(|x| x.prefix == prefix);

        assert!(prefix_record.is_some(), "Prefix not found: {}", prefix);

        let snowflake = self.snowflake.gen();

        let id = if prefix_record.unwrap().secure {
            let random_bytes: String = (0..16).map(|_| rand::random::<u8>() as char).collect();

            format!(
                "{}_s_{}",
                prefix,
                base64::encode(random_bytes + &snowflake).replace('=', "")
            )
        } else {
            format!("{}_{}", prefix, base64::encode(snowflake).replace('=', ""))
        };

        Ok(id)
    }
}

#[cfg(test)]
mod tests {
    use super::{InitOptions, Pika, PrefixRecord};

    #[test]
    fn init_pika() {
        let prefixes = [PrefixRecord {
            prefix: "test".to_string(),
            description: Some("test".to_string()),
            secure: false,
        }];

        let mut pika = Pika::new(
            prefixes.to_vec(),
            &InitOptions {
                epoch: Some(1_650_153_600_000),
                node_id: None,
                disable_lowercase: Some(true),
            },
        );

        let id = pika.gen("test").unwrap();
        let deconstructed = pika.deconstruct(&id);

        // cant statically check because mac address is per device
        assert_eq!(deconstructed.node_id, Pika::compute_node_id());
        assert_eq!(deconstructed.seq, 0);
        assert_eq!(deconstructed.version, 1);
        assert_eq!(deconstructed.epoch, 1_650_153_600_000);
    }

    #[test]
    fn init_pika_with_nodeid() {
        let prefixes = [PrefixRecord {
            prefix: "test".to_string(),
            description: Some("test".to_string()),
            secure: false,
        }];

        let mut pika = Pika::new(
            prefixes.to_vec(),
            &InitOptions {
                epoch: Some(1_650_153_600_000),
                node_id: Some(622),
                disable_lowercase: Some(true),
            },
        );

        let id = pika.gen("test").unwrap();
        let deconstructed = pika.deconstruct(&id);

        assert_eq!(deconstructed.node_id, 622);
        assert_eq!(deconstructed.seq, 0);
        assert_eq!(deconstructed.version, 1);
        assert_eq!(deconstructed.epoch, 1_650_153_600_000);
    }
}
