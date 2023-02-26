use std::io::Error;

use crate::base64::{base64_decode, base64_encode};
use crate::snowflake::{self, Snowflake};

#[derive(Clone, Debug, Default)]
pub struct PrefixRecord {
    pub prefix: String,
    pub description: Option<String>,
    pub secure: bool,
}

#[derive(Debug, Default)]
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

#[derive(Debug, Clone)]
pub struct Pika {
    pub prefixes: Vec<PrefixRecord>,
    pub epoch: u64,
    pub node_id: u32,
    pub disable_lowercase: Option<bool>,
    snowflake: Snowflake,
}

#[derive(Default)] // default implementation was identical to std::default::Default
pub struct InitOptions {
    pub epoch: Option<u64>,
    pub node_id: Option<u32>,
    pub disable_lowercase: Option<bool>,
}

pub const DEFAULT_EPOCH: u64 = 1_640_995_200_000;

impl Pika {
    pub fn new(prefixes: Vec<PrefixRecord>, options: &InitOptions) -> Self {
        let epoch = options.epoch.unwrap_or(DEFAULT_EPOCH);
        let node_id = options.node_id.unwrap_or_else(Self::compute_node_id);

        Self {
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

    pub fn gen(&mut self, prefix: &str) -> Result<String, Error> {
        let valid_prefix = prefix.chars().all(|c| c.is_ascii_alphanumeric())
            && prefix.len() <= 32
            && !prefix.is_empty();

        assert!(valid_prefix, "Invalid prefix: {prefix}");

        let prefix_record = self.prefixes.iter().find(|x| x.prefix == prefix);

        assert!(prefix_record.is_some(), "Prefix not found: {prefix}");

        let snowflake = self.snowflake.gen();

        let id = if prefix_record.unwrap().secure {
            let random_bytes: String = (0..16).map(|_| rand::random::<u8>() as char).collect();
            let tail = format!("s_{}_{}", random_bytes, snowflake);
            format!(
                "{}_{}",
                prefix,
                base64_encode(tail).replace('=', "")
            )
        } else {
            format!("{}_{}", prefix, base64_encode(snowflake).replace('=', ""))
        };

        Ok(id)
    }

    pub fn deconstruct(&self, id: &str) -> DecodedPika {
        let parts: Vec<&str> = id.split('_').collect();
        let prefix = parts[0];
        let tail = parts[1];

        let prefix_record = self.prefixes.iter().find(|x| x.prefix == prefix).unwrap();

        if prefix_record.secure {
            let decoded_tail = base64_decode(tail).unwrap();
            let binding = String::from_utf8_lossy(&decoded_tail).to_string();
            let decoded_tail_elements = binding.split('_').collect::<Vec<&str>>();

            let snowflake = self.snowflake.decode(decoded_tail_elements[2]);

            DecodedPika {
                prefix: prefix.to_string(),
                tail: tail.to_string(),
                snowflake: decoded_tail_elements[2].parse::<u64>().unwrap(),
                node_id: self.node_id,
                timestamp: snowflake.timestamp,
                epoch: self.epoch,
                seq: snowflake.seq,
                version: 1,
                prefix_record: prefix_record.clone(),
            }
        } else {
            let decoded_tail = base64_decode(&tail).unwrap();

            let snowflake = self
                .snowflake
                .decode(&String::from_utf8_lossy(&decoded_tail));
            let stringified_tail = String::from_utf8_lossy(&decoded_tail).to_string();
    
            DecodedPika {
                prefix: prefix.to_string(),
                tail: prefix.to_string(),
                snowflake: stringified_tail.parse::<u64>().unwrap(),
                node_id: self.node_id,
                timestamp: snowflake.timestamp,
                epoch: self.epoch,
                seq: snowflake.seq,
                version: 1,
                prefix_record: prefix_record.clone(),
            }
        }
        
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
