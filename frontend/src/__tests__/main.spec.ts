import { describe, expect, it, vi } from 'vitest'

const mocks = vi.hoisted(() => ({
  app: {
    use: vi.fn(() => mocks.app),
    mount: vi.fn(),
  },
  createApp: vi.fn(() => mocks.app),
  createPinia: vi.fn(() => ({ pinia: true })),
}))

vi.mock('vue', () => ({
  createApp: mocks.createApp,
}))

vi.mock('pinia', () => ({
  createPinia: mocks.createPinia,
}))

vi.mock('../App.vue', () => ({
  default: { name: 'AppStub' },
}))

vi.mock('../router', () => ({
  default: { router: true },
}))

describe('main.ts', () => {
  it('creates and mounts the Vue app', async () => {
    await import('../main')

    expect(mocks.createApp).toHaveBeenCalled()
    expect(mocks.createPinia).toHaveBeenCalled()
    expect(mocks.app.use).toHaveBeenCalledTimes(2)
    expect(mocks.app.mount).toHaveBeenCalledWith('#app')
  })
})
