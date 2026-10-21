module.exports = {
  testEnvironment: 'node',
  testMatch: ['**/*.pact.test.ts'],
  setupFilesAfterEnv: ['<rootDir>/tests/pact/setup.ts'],
  testTimeout: 30000,
};