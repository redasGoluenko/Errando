import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'

vi.mock('@/stores/notificationStore', () => ({
  useNotificationStore: () => ({
    unreadCount: 5,
    markAllAsRead: vi.fn().mockResolvedValue(undefined),
  }),
}))
vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn().mockResolvedValue(undefined) }),
}))

import NotificationBell from '../NotificationBell.vue'

describe('NotificationBell.vue', () => {
  it('renders unread badge and navigates on click', async () => {
    const wrapper = mount(NotificationBell)
    expect(wrapper.text()).toContain('5')

    await wrapper.find('button').trigger('click')
    expect(wrapper.html()).toContain('aria-label="Messages"')
  })
})
