const PREFIX = `[pika]`;

export const warn = (...args: unknown[]) => {
  console.warn(`${PREFIX}`, ...args);
};

export const error = (...args: unknown[]) => {
  console.error(`${PREFIX}`, ...args);
};
