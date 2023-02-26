use crate::utils::now_timestamp;

#[derive(Clone, Debug)]
pub struct Snowflake {
    epoch: u64,
    node_id: u32,
    seq: u32,
    last_sequence_exhaustion: u64,
}

#[derive(Clone, Debug)]
pub struct DecodedSnowflake {
    pub id: u64,
    pub timestamp: u64,
    pub node_id: u32,
    pub seq: u64,
    pub epoch: u64,
}

impl Snowflake {
    pub fn new_with_nodeid(epoch: u64, node_id: u32) -> Self {
        Self {
            epoch,
            node_id,
            seq: 0,
            last_sequence_exhaustion: 0,
        }
    }

    #[inline]
    pub fn gen(&mut self) -> String {
        self.gen_with_ts(now_timestamp())
    }

    pub fn gen_with_ts(&mut self, timestamp: u64) -> String {
        if self.seq >= 4095 && timestamp == self.last_sequence_exhaustion {
            while now_timestamp() - timestamp < 1 {
                continue;
            }
        }

        let sf = ((timestamp - self.epoch) << 22)
            | (u64::from(self.node_id) << 12)
            | u64::from(self.seq);

        self.seq = if self.seq >= 4095 { 0 } else { self.seq + 1 };

        if self.seq == 4095 {
            self.last_sequence_exhaustion = timestamp;
        }

        sf.to_string()
    }

    pub fn decode(&self, sf: &str) -> DecodedSnowflake {
        let sf = sf.parse::<u64>().unwrap();

        let timestamp = (sf >> 22) + self.epoch;
        let node_id = (sf >> 12) & 0b11_1111_1111;
        let seq = sf & 0b1111_1111_1111;

        DecodedSnowflake {
            id: sf,
            timestamp,
            node_id: node_id as u32,
            seq,
            epoch: self.epoch,
        }
    }
}

mod test {
    #[test]
    fn generate_snowflake() {
        // if the node_id >= 1024 it will go to 0?
        let mut sf = super::Snowflake::new_with_nodeid(650_153_600_000, 1023);
        let snowflake = sf.gen();

        let deconstruct = sf.decode(snowflake.as_str());

        assert_eq!(deconstruct.epoch, 650_153_600_000);
        assert_eq!(deconstruct.node_id, 1023);
    }

    #[test]
    fn generate_snowflakes() {
        let mut sf = super::Snowflake::new_with_nodeid(650_153_600_000, 1023);

        // when the seq is 4096, the next snowflake will be 0
        let snowflakes: Vec<String> = (0..4096).map(|_| sf.gen()).collect();
        let last_snowflake = sf.gen();

        for (sequence, snowflake) in snowflakes.iter().enumerate() {
            let deconstruct = sf.decode(snowflake.as_str());

            assert_eq!(deconstruct.seq, sequence as u64);
        }

        let deconstruct = sf.decode(last_snowflake.as_str());
        assert_eq!(deconstruct.seq, 0);
    }
}
