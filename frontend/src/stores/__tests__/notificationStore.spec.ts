import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useNotificationStore } from '../notificationStore'
import ChatService from '../../services/ChatService'
import { authService } from '../../services/api'

describe('notificationStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    localStorage.clear()
    vi.spyOn(authService, 'getUserId').mockReturnValue(1)
  })

  afterEach(() => {
    vi.restoreAllMocks()
    localStorage.clear()
  })

  it('checks unread messages and saves to storage', async () => {
    vi.spyOn(ChatService, 'getChats').mockResolvedValue([
      { id: 1, messages: [{ sentAt: '2026-01-01T00:00:00.000Z' }] },
    ] as any)

    const store = useNotificationStore()
    await store.checkForUnreadMessages()

    expect(store.unreadCount).toBe(1)
    expect(localStorage.getItem('errando_last_seen_1')).toContain('2026-01-01T00:00:00.000Z')
  })

  it('marks all messages as read', async () => {
    vi.spyOn(ChatService, 'getChats').mockResolvedValue([
      { id: 1, messages: [{ sentAt: '2026-01-01T00:00:00.000Z' }] },
    ] as any)

    const store = useNotificationStore()
    await store.markAllAsRead()

    expect(store.unreadCount).toBe(0)
  })
})
