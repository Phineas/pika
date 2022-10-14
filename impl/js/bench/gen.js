const Benchmark = require('benchmark');
const Pika = require('../dist');

const suite = new Benchmark.Suite();

const p = new Pika(['test']);

suite
	.add('Pika#gen', () => {
		p.gen('test');
	})
	.add('Snowflake#gen', () => {
		p.genSnowflake();
	})
	.on('cycle', function (event) {
		console.log(String(event.target));
	})
	.run({async: true});
