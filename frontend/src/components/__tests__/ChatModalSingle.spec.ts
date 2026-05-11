import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia } from 'pinia'
import ChatModalSingle from '../ChatModalSingle.vue'
import ChatService from '../../services/ChatService'

describe('ChatModalSingle.vue', () => {
  beforeEach(() => {
    localStorage.setItem('userId', '1')
  })

  afterEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  it('renders messages and sends a new message', async () => {
    const chat = {
      id: 1,
      user1Id: 1,
      user2Id: 2,
      user1: { username: 'me', role: 'Client' },
      user2: { username: 'other', role: 'Runner' },
      messages: [{ id: 1, senderId: 2, content: 'Hello', sentAt: new Date().toISOString() }],
    }

    const sentMessage = { id: 2, senderId: 1, content: 'Reply', sentAt: new Date().toISOString() }
    vi.spyOn(ChatService, 'sendMessage').mockResolvedValue(sentMessage)

    const wrapper = mount(ChatModalSingle, {
      props: { chat },
      global: { plugins: [createPinia()] },
    })

    await wrapper.find('input[type="text"]').setValue('Reply')
    const sendButton = wrapper.findAll('button').find((btn) => btn.text().includes('Send'))
    expect(sendButton).toBeTruthy()
    await sendButton?.trigger('click')

    await wrapper.vm.$nextTick()

    expect(ChatService.sendMessage).toHaveBeenCalledWith(1, 'Reply')
    expect(wrapper.emitted('close')).toBeUndefined()
  })
})
