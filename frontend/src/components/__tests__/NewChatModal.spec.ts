import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import NewChatModal from '../NewChatModal.vue'
import ChatService from '../../services/ChatService'

const participants = [
  { id: 2, username: 'other', role: 'Runner', email: 'other@example.com' },
]

const chats = []

describe('NewChatModal.vue', () => {
  beforeEach(() => {
    vi.spyOn(ChatService, 'getChatParticipants').mockResolvedValue(participants)
    vi.spyOn(ChatService, 'getChats').mockResolvedValue(chats)
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads participants and emits chat-created after selecting a user', async () => {
    const createSpy = vi.spyOn(ChatService, 'createOrGetChat').mockResolvedValue({ id: 1, user1Id: 1, user2Id: 2, messages: [] })

    const wrapper = mount(NewChatModal, {
      global: {
        stubs: ['Transition'],
      },
    })

    await flushPromises()

    expect(wrapper.text()).toContain('Start a New Chat')

    const buttons = wrapper.findAll('button')
    const participantButton = buttons.find((btn) => btn.text().includes('other'))
    expect(participantButton).toBeTruthy()
    await participantButton?.trigger('click')
    await wrapper.vm.$nextTick()

    const startButton = wrapper.findAll('button').find((btn) => btn.text().includes('Start Chat'))
    expect(startButton).toBeTruthy()
    await startButton?.trigger('click')
    await wrapper.vm.$nextTick()

    expect(createSpy).toHaveBeenCalled()
    expect(wrapper.emitted('chat-created')).toBeDefined()
  })
})
