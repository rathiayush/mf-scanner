// Ensure the environment variable is defined
if (!import.meta.env.VITE_API_BASE_URL) {
  console.warn('VITE_API_BASE_URL is not set. Using default localhost URL.');
}

const env = {
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5247/api',
  },
} as const;

export default env;
