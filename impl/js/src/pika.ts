import { randomBytes } from "crypto";
import { error, warn } from "./logger";
import { FromEpoch, Snowflake } from "./snowflake";

export interface PikaPrefixRecord<P extends string> {
  prefix: P;
  description?: string;
  secure?: boolean;
  metadata?: Record<string, unknown>;
}

export interface DecodedPika<P extends string> {
  prefix: string;
  tail: string;
  snowflake: bigint;
  nodeId: number;
  seq: number;
  version: 1;
  prefix_record: PikaPrefixRecord<P> | undefined;
}

interface PikaInitializationOptions {
  epoch?: FromEpoch;
  suppressPrefixWarnings?: boolean;
  disableLowercase?: boolean;
}

const VALID_PREFIX = /^[a-z0-9_]+$/i;
const DEFAULT_EPOCH = 1640995200000n; // Jan 1 2022

type PrefixInit<V extends string> = V | PikaPrefixRecord<V>;
type LowercasePrefixInit<V extends string> = Lowercase<V> extends V
  ? PrefixInit<V>
  : PrefixInit<Lowercase<V>>;

export class Pika<V extends string> {
  public readonly prefixes: Record<string, PikaPrefixRecord<V>> = {};
  #snowflake: Snowflake;
  #suppressPrefixWarnings = false;

  /**
   * See
   * @param prefixes a list of PikaPrefixRecords to initialize pika with
   * @param opts misc. options to initialize pika with
   */
  constructor(
    prefixes: readonly LowercasePrefixInit<V>[],
    opts: PikaInitializationOptions = {}
  ) {
    this.#snowflake = new Snowflake(opts.epoch || DEFAULT_EPOCH);
    this.#suppressPrefixWarnings = opts.suppressPrefixWarnings ?? false;

    this.prefixes = prefixes.reduce(
      (a, p) =>
        typeof p === "string"
          ? { ...a, [p]: { prefix: p } }
          : { ...a, [p.prefix]: p },
      {}
    );
  }

  gen(prefix: V) {
    if (!VALID_PREFIX.test(prefix)) {
      throw TypeError(
        `invalid prefix; prefixes must be alphanumeric (a-z0-9_) and may include underscores; received: ${prefix}`
      );
    }

    if (!this.prefixes[prefix] && !this.#suppressPrefixWarnings)
      warn(
        `Unregistered prefix (${prefix}) was used. This can cause unknown behavior - see https://github.com/hopinc/pika/tree/main/impl/js for details.`
      );

    const snowflake = this.#snowflake.gen();
    return `${prefix.toLowerCase()}_${Buffer.from(
      (this.prefixes[prefix]?.secure
        ? `s_${randomBytes(16).toString("hex")}_`
        : "") + snowflake
    ).toString("base64url")}`;
  }

  /**
   *  Gen a Snowflake, if you really need one
   */
  public genSnowflake() {
    return this.#snowflake.gen();
  }

  public decode(id: string): DecodedPika<V> {
    try {
      const s = id.split("_");
      const tail = s[s.length - 1];
      const prefix = s.slice(0, s.length - 1).join("_");

      const decodedTail = Buffer.from(tail, "base64").toString();
      const sf = decodedTail.split("_").pop();
      if (!sf)
        throw Error("attempted to decode invalid pika; tail was corrupt");

      const { id: snowflake, ...v } = this.#snowflake.deconstruct(sf);

      return {
        prefix,
        tail,
        prefix_record: this.prefixes[prefix],
        snowflake,
        version: 1,
        ...v,
      };
    } catch (e: unknown) {
      error("Failed to decode ID", id);
      throw e;
    }
  }
}
