export type FromEpoch = number | bigint | Date;
interface SnowflakeGenOpts {
  timestamp?: FromEpoch;
}

interface DeconstructedSnowflake {
  id: bigint;
  timestamp: bigint;
  nodeId: number;
  seq: number;
  epoch: bigint;
}

/**
 * A class for generating and deconstructing snowflakes.
 *
 * Pika has put it's own spin on Twitter snowflakes to simplify deployment
 * and setup. Instead of having a separate worker and process ID, we have
 * one node ID that takes up the 10 bits these fields would usually use.
 *
 * A node ID is computed by taking the MAC address of the first available
 * public interface on the device, then calculating the modulo against
 * 1024 (10b)
 *
 * If we have a snowflake `963584775274749952n` we can represent it as binary:
 * ```
 * 64                                          22           12          0
 *  000011010101111101011111011110000011001010  0001000101  000000000000
 *           number of ms since epoch            node id      sequence
 * ```
 */
export class Snowflake {
  /**
   * Snowflakes generated are derived from this epoch
   * @internal
   */
  #epoch: bigint;

  /**
   * Passed in node ID for this Snowflake instance
   * @internal
   */
  #nodeId: bigint;

  /**
   * Current sequence number (0-4095)
   * @internal
   */
  #seq = 0n;

  /**
   * Last timestamp of the last time the sequence was exhausted
   * @internal
   */
  #lastSequenceExhaustion: number = 0;

  /**
   * @param epoch the base epoch to use
   * @param nodeId optionally pass a static node identifier (0-1023)
   */
  constructor(epoch: FromEpoch, nodeId: number | bigint) {
    this.#epoch = this.normalizeEpoch(epoch);
    this.#nodeId = BigInt(nodeId);
  }

  public get nodeId(): number {
    return Number(this.nodeId);
  }

  public gen({ timestamp = Date.now() }: SnowflakeGenOpts = {}): string {
    const nTimestamp = this.normalizeEpoch(timestamp);

    if (this.#seq === 4095n && timestamp === this.#lastSequenceExhaustion) {
      // purposely blocking
      while (Date.now() - timestamp < 1) {
        continue;
      }
    }

    this.#seq = this.#seq >= 4095n ? 0n : this.#seq + 1n;
    if (this.#seq === 4095n) this.#lastSequenceExhaustion = Date.now();

    return (
      ((nTimestamp - this.#epoch) << 22n) | // millis since epoch
      ((this.#nodeId & 0b1111111111n) << 12n) |
      this.#seq
    ).toString();
  }

  public deconstruct(id: string | bigint): DeconstructedSnowflake {
    const bigIntId = BigInt(id);
    return {
      id: bigIntId,
      timestamp: (bigIntId >> 22n) + this.#epoch,
      nodeId: Number((bigIntId >> 12n) & 0b1111111111n),
      seq: Number(bigIntId & 0b111111111111n),
      epoch: this.#epoch,
    };
  }

  private normalizeEpoch(epoch: FromEpoch): bigint {
    return BigInt(epoch instanceof Date ? epoch.getTime() : epoch);
  }
}
