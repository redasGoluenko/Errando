import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vitest/config'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    pool: 'threads',
    deps: {
      inline: ['@asamuzakjp/css-color', '@csstools/css-calc'],
    },
    coverage: {
      provider: 'istanbul',
      reporter: ['text', 'html'],
      all: true,
      include: ['src/**/*.{ts,vue}'],
      exclude: ['node_modules/', 'src/**/*.spec.ts', 'src/**/*.test.ts'],
    },
  },
})
