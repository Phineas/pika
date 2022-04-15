import { error, warn } from "./logger";
import { FromEpoch, Snowflake } from "./snowflake";

export interface PikaPrefixRecord<P extends string> {
  prefix: Lowercase<P>;
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

type PrefixInit<V extends string> = Lowercase<V> | PikaPrefixRecord<V>;

export class Pika<
  V extends string,
  T extends readonly [PrefixInit<V>, ...PrefixInit<V>[]]
> {
  prefixes: Record<string, PikaPrefixRecord<V>> = {};
  #snowflake: Snowflake;
  #suppressPrefixWarnings = false;

  /**
   * See
   * @param prefixes a list of PikaPrefixRecords to initialize pika with
   * @param opts misc. options to initialize pika with
   */
  constructor(prefixes: T, opts: PikaInitializationOptions = {}) {
    this.#snowflake = new Snowflake(opts.epoch || DEFAULT_EPOCH);
    this.#suppressPrefixWarnings = opts.suppressPrefixWarnings ?? false;

    this.prefixes = prefixes.reduce(
      (a, p) =>
        typeof p === "string" ? { ...a, [p]: { prefix: p } } : { ...a, p },
      {}
    );
  }

  gen(
    prefix:
      | Extract<T[number], string>
      | Extract<Exclude<T[number], string>, PikaPrefixRecord<string>>["prefix"]
  ) {
    // prefix = prefix.toLowerCase();

    if (!VALID_PREFIX.test(prefix)) {
      throw TypeError(
        `invalid prefix; prefixes must be alphanumeric (a-z0-9_) and may include underscores; received: ${prefix}`
      );
    }

    if (!this.prefixes[prefix] && !this.#suppressPrefixWarnings)
      warn(
        `Unregistered prefix (${prefix}) was used. This can cause unknown behavior - see <> for details.`
      );

    const snowflake = this.#snowflake.gen();
    return `${prefix.toLowerCase()}_${Buffer.from(snowflake).toString(
      "base64url"
    )}`;
  }

  /**
   *  Gen a Snowflake, if you really need one
   */
  public genSnowflake() {
    return this.#snowflake.gen();
  }

  public decode(id: V): DecodedPika<V> {
    try {
      const s = id.split("_");
      const tail = s[s.length - 1];
      const prefix = s.slice(0, s.length - 1).join("_");

      const sf = tail.split("_").pop();
      if (!sf)
        throw Error("attempted to decode invalid pika; tail was corrupt");

      const { id: snowflake, ...v } = this.#snowflake.deconstruct(
        Buffer.from(sf, "base64url").toString()
      );

      return {
        prefix,
        tail,
        prefix_record: this.prefixes[prefix],
        snowflake,
        version: 1,
        ...v,
      };
    } catch (e) {
      error("Failed to decode ID", id);
      throw e;
    }
  }
}
