// import { Snowflake } from "./snowflake";

import { Pika } from "./pika";

// const s = new Snowflake(1420070400000);

// console.log(s.gen());
// for (var i = 0; i < 4096; i++) {
//   console.log(s.deconstruct(s.gen()));
// }

const p = new Pika([{ prefix: "cus", description: "Customers" }], {
  suppressPrefixWarnings: false,
});
const i = p.gen("cus");
console.log(i);
console.log(p.decode(i));
