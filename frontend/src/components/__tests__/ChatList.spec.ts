import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import ChatList from '../ChatList.vue'

describe('ChatList.vue', () => {
  beforeEach(() => {
    localStorage.setItem('userId', '1')
  })

  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  it('renders chat list and emits chat-selected when a chat is clicked', async () => {
    const chats = [
      {
        id: 1,
        user1Id: 1,
        user2Id: 2,
        user1: { username: 'me', role: 'Client' },
        user2: { username: 'other', role: 'Runner' },
        updatedAt: new Date().toISOString(),
        messages: [{ senderId: 2, content: 'Hello' }],
      },
    ]

    const wrapper = mount(ChatList, {
      props: {
        chats,
        selectedChatId: null,
        loading: false,
      },
      global: {
        plugins: [createPinia()],
        stubs: ['NewChatModal'],
      },
    })

    expect(wrapper.text()).toContain('Messages')
    expect(wrapper.text()).toContain('1 conversation(s)')

    const chatButtons = wrapper.findAll('div.cursor-pointer')
    expect(chatButtons.length).toBeGreaterThan(0)
    await chatButtons[0].trigger('click')

    expect(wrapper.emitted('chat-selected')).toBeDefined()
  })
})
