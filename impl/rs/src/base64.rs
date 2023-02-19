const BASE64_CHARS: [u8; 64] = [
    b'A', b'B', b'C', b'D', b'E', b'F', b'G', b'H', b'I', b'J', b'K', b'L', b'M', b'N', b'O', b'P',
    b'Q', b'R', b'S', b'T', b'U', b'V', b'W', b'X', b'Y', b'Z', b'a', b'b', b'c', b'd', b'e', b'f',
    b'g', b'h', b'i', b'j', b'k', b'l', b'm', b'n', b'o', b'p', b'q', b'r', b's', b't', b'u', b'v',
    b'w', b'x', b'y', b'z', b'0', b'1', b'2', b'3', b'4', b'5', b'6', b'7', b'8', b'9', b'+', b'/'
];

// Base64 encode a slice of bytes
pub fn base64_encode(input: String) -> String {
    let mut result = String::new();
    let mut buffer: u32 = 0;
    let mut bits_left = 0;

    for byte in input.into_bytes() {
        buffer = (buffer << 8) | byte as u32;
        bits_left += 8;

        while bits_left >= 6 {
            bits_left -= 6;
            let index = ((buffer >> bits_left) & 0b111111) as usize;
            result.push(BASE64_CHARS[index] as char);
        }
    }

    if bits_left > 0 {
        buffer <<= 6 - bits_left;
        let index = (buffer & 0b111111) as usize;
        result.push(BASE64_CHARS[index] as char);
    }

    while result.len() % 4 != 0 {
        result.push('=');
    }

    result
}

// Base64 decode a string into a vector of bytes
pub fn base64_decode(encoded: &str) -> Option<Vec<u8>> {
    let mut decoded = Vec::new();
    let mut padding = 0;
    let mut buffer = 0;
    let mut bits = 0;

    for c in encoded.chars() {
        let value = match BASE64_CHARS.iter().position(|&x| x == c as u8) {
            Some(v) => v as u32,
            None if c == '=' => {
                padding += 1;
                continue;
            }
            None => return None,
        };

        buffer = (buffer << 6) | value;
        bits += 6;

        if bits >= 8 {
            bits -= 8;
            decoded.push((buffer >> bits) as u8);
            buffer &= (1 << bits) - 1;
        }
    }

    if bits >= 6 || padding > 2 || (padding > 0 && bits > 0) {
        return None;
    }

    Some(decoded)
}