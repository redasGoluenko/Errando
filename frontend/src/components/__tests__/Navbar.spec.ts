import { mount, flushPromises } from '@vue/test-utils'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import Navbar from '../Navbar.vue'

const mocks = vi.hoisted(() => ({
  afterEachHandler: undefined as undefined | (() => void),
  router: {
    afterEach: vi.fn((handler: () => void) => {
      mocks.afterEachHandler = handler
    }),
    replace: vi.fn(() => Promise.resolve()),
  },
  auth: {
    isAuthenticated: vi.fn(() => true),
    getUsername: vi.fn(() => 'client'),
    getRole: vi.fn(() => 'Client'),
    logout: vi.fn(),
  },
  notifications: {
    unreadCount: 3,
    initializeForCurrentUser: vi.fn(() => Promise.resolve()),
    checkForUnreadMessages: vi.fn(),
    reset: vi.fn(),
  },
}))

vi.mock('vue-router', () => ({
  useRouter: () => mocks.router,
}))

vi.mock('@/services/api', () => ({
  authService: mocks.auth,
}))

vi.mock('@/stores/notificationStore', () => ({
  useNotificationStore: () => mocks.notifications,
}))

const stubs = {
  RouterLink: {
    props: ['to'],
    template: '<a :href="to" @click="$emit(\'click\', $event)"><slot /></a>',
  },
  NotificationBell: { template: '<button>bell</button>' },
}

describe('Navbar.vue', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    mocks.afterEachHandler = undefined
    mocks.router.afterEach.mockClear()
    mocks.router.replace.mockClear()
    mocks.auth.isAuthenticated.mockReturnValue(true)
    mocks.auth.getUsername.mockReturnValue('client')
    mocks.auth.getRole.mockReturnValue('Client')
    mocks.auth.logout.mockClear()
    mocks.notifications.unreadCount = 3
    mocks.notifications.initializeForCurrentUser.mockClear()
    mocks.notifications.checkForUnreadMessages.mockClear()
    mocks.notifications.reset.mockClear()
  })

  it('renders authenticated client navigation and logs out', async () => {
    const wrapper = mount(Navbar, { global: { stubs } })
    await flushPromises()

    expect(wrapper.text()).toContain('My Tasks')
    expect(wrapper.text()).toContain('client')
    expect(mocks.notifications.initializeForCurrentUser).toHaveBeenCalled()

    await wrapper.find('button.bg-red-600').trigger('click')
    expect(mocks.auth.logout).toHaveBeenCalled()
    expect(mocks.notifications.reset).toHaveBeenCalled()
    expect(mocks.router.replace).toHaveBeenCalledWith('/login')

    vi.useRealTimers()
  })

  it('renders guest links and role-specific mobile links', async () => {
    mocks.auth.isAuthenticated.mockReturnValue(false)
    mocks.auth.getUsername.mockReturnValue(null)
    mocks.auth.getRole.mockReturnValue(null)

    const wrapper = mount(Navbar, { global: { stubs } })
    await flushPromises()
    expect(wrapper.text()).toContain('Login')
    expect(wrapper.text()).toContain('Register')

    mocks.auth.isAuthenticated.mockReturnValue(true)
    mocks.auth.getUsername.mockReturnValue('runner')
    mocks.auth.getRole.mockReturnValue('Runner')
    mocks.afterEachHandler?.()
    await wrapper.find('button[aria-label="Toggle menu"]').trigger('click')

    expect(wrapper.text()).toContain('Runner Dashboard')
    expect(wrapper.text()).toContain('Messages')
    expect(wrapper.text()).toContain('runner')

    vi.useRealTimers()
  })
})
