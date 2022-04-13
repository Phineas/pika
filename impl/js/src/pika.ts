import { Snowflake } from "./snowflake";

export interface PikaPrefixRecord {
  prefix: string;
  description: string;
  secure?: boolean;
  metadata?: Record<string, any>;
}

export interface DecodedPika {
  prefix: string;
  tail: string;
  snowflake: bigint;
  nodeId: number;
  seq: number;
  version: 1;
  prefix_record: PikaPrefixRecord | undefined;
}

interface PikaInitializationOptions {
  suppressPrefixWarnings?: boolean;
}

const VALID_PREFIX = /^[a-z0-9_]+$/i;

export class Pika {
  #snowflake: Snowflake;
  prefixes: Record<string, PikaPrefixRecord> = {};
  #supressPrefixWarnings = false;

  constructor(
    prefixes: PikaPrefixRecord[] = [],
    opts: PikaInitializationOptions = {}
  ) {
    this.#snowflake = new Snowflake(1420070400000);
    this.#supressPrefixWarnings = opts.suppressPrefixWarnings ?? false;

    prefixes.forEach((p) => this.register(p));
  }

  public register(prefixRecord: PikaPrefixRecord) {
    if (!VALID_PREFIX.test(prefixRecord.prefix)) {
      console.warn(
        `[pika] an invalid prefix ${prefixRecord.prefix} was attemped to be registered; prefixes must be alphanumeric (a-z0-9_) and may include underscores; this prefix will be ignored`
      );
      return;
    }

    this.prefixes = { ...this.prefixes, [prefixRecord.prefix]: prefixRecord };
  }

  public deregister(prefix: string) {
    delete this.prefixes[prefix];
  }

  public gen(prefix: string) {
    if (!VALID_PREFIX.test(prefix))
      throw TypeError(
        `invalid prefix; prefixes must be alphanumeric (a-z0-9_) and may include underscores; received: ${prefix}`
      );

    if (!this.prefixes[prefix] && !this.#supressPrefixWarnings)
      console.warn(
        `[pika] Unregistered prefix (${prefix}) was used. This can cause unknown behavior - see <> for details.`
      );

    const snowflake = this.#snowflake.gen();
    return `${prefix.toLowerCase()}_${Buffer.from(snowflake).toString(
      "base64"
    )}`;
  }

  public decode(id: string): DecodedPika {
    try {
      const s = id.split("_");
      const tail = s[s.length - 1];
      const prefix = s.slice(0, s.length - 1).join("_");

      const sf = tail.split("_").pop();
      if (!sf)
        throw Error("attempted to decode invalid pika; tail was corrupt");

      const { id: snowflake, ...v } = this.#snowflake.deconstruct(
        Buffer.from(sf, "base64").toString()
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
      console.error("[pika] Failed to decode ID", id);
      throw e;
    }
  }
}
