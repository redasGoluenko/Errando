import { describe, expect, it, vi } from 'vitest'

const mocks = vi.hoisted(() => ({
  routerOptions: undefined as any,
  beforeEachGuard: undefined as any,
  auth: {
    isAuthenticated: vi.fn(() => false),
    getRole: vi.fn(() => 'Client'),
  },
}))

vi.mock('vue-router', () => ({
  createWebHistory: vi.fn(() => ({ history: true })),
  createRouter: vi.fn((options) => {
    mocks.routerOptions = options
    return {
      beforeEach: vi.fn((guard) => {
        mocks.beforeEachGuard = guard
      }),
    }
  }),
}))

vi.mock('@/services/api', () => ({
  authService: mocks.auth,
}))

describe('router', () => {
  it('configures auth redirects and route guards', async () => {
    await import('../index')

    const root = mocks.routerOptions.routes.find((route: any) => route.path === '/')
    expect(root.redirect()).toBe('/login')

    mocks.auth.isAuthenticated.mockReturnValue(true)
    expect(root.redirect()).toBe('/dashboard')

    const login = mocks.routerOptions.routes.find((route: any) => route.path === '/login')
    const next = vi.fn()
    login.beforeEnter({}, {}, next)
    expect(next).toHaveBeenCalledWith('/dashboard')

    mocks.auth.isAuthenticated.mockReturnValue(false)
    next.mockClear()
    login.beforeEnter({}, {}, next)
    expect(next).toHaveBeenCalledWith()
  })

  it('redirects unauthenticated and unauthorized navigation', async () => {
    await import('../index')

    const next = vi.fn()
    mocks.auth.isAuthenticated.mockReturnValue(false)
    mocks.beforeEachGuard({ path: '/tasks', matched: [{ meta: { requiresAuth: true } }], meta: {} }, {}, next)
    expect(next).toHaveBeenCalledWith('/login')

    next.mockClear()
    mocks.auth.isAuthenticated.mockReturnValue(true)
    mocks.auth.getRole.mockReturnValue('Client')
    mocks.beforeEachGuard({ path: '/admin/users', matched: [{ meta: { requiresAuth: true } }], meta: { role: 'Admin' } }, {}, next)
    expect(next).toHaveBeenCalledWith('/dashboard')

    next.mockClear()
    mocks.auth.getRole.mockReturnValue('Admin')
    mocks.beforeEachGuard({ path: '/admin/users', matched: [{ meta: { requiresAuth: true } }], meta: { role: 'Admin' } }, {}, next)
    expect(next).toHaveBeenCalledWith()
  })
})
