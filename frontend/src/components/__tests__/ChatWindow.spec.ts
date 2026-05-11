import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import ChatWindow from '../ChatWindow.vue'
import ChatService from '../../services/ChatService'

describe('ChatWindow.vue', () => {
  beforeEach(() => {
    localStorage.setItem('userId', '1')
  })

  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  it('sends a message and emits message-sent', async () => {
    const chat = {
      id: 1,
      user1Id: 1,
      user2Id: 2,
      user1: { username: 'me', role: 'Client' },
      user2: { username: 'other', role: 'Runner' },
      messages: [],
    }

    const message = { id: 1, senderId: 1, content: 'Hi', sentAt: new Date().toISOString() }
    vi.spyOn(ChatService, 'sendMessage').mockResolvedValue(message)

    const wrapper = mount(ChatWindow, {
      props: {
        chat,
        loading: false,
      },
      global: {
        plugins: [createPinia()],
      },
    })

    await wrapper.find('textarea').setValue('Hello')
    const sendButton = wrapper.findAll('button').find((btn) => btn.text().includes('Send'))
    expect(sendButton).toBeTruthy()
    await sendButton?.trigger('click')
    await wrapper.vm.$nextTick()

    expect(ChatService.sendMessage).toHaveBeenCalledWith(1, 'Hello')
    expect(wrapper.emitted('message-sent')).toBeDefined()
  })
})
