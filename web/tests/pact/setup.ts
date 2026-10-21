const { PactV4 } = require('@pact-foundation/pact');
const path = require('path');

const provider = new PactV4({
  consumer: 'stock-analysis-web',
  provider: 'stock-analysis-api',
  port: 1234,
  dir: path.resolve(process.cwd(), 'pacts'),
  logLevel: 'warn',
});

module.exports = { provider };