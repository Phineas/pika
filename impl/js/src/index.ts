import { Pika } from "./pika";

const p = new Pika([
  "a",
  {
    prefix: "sk",
    secure: true,
  },
]);
console.log(p.gen("sk"));
console.log(p.decode(p.gen("sk")));
export = Pika;
