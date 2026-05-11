import { describe, it, expect, vi, afterEach } from 'vitest'

vi.mock('axios', () => {
  const postMock = vi.fn()
  const getMock = vi.fn()
  return {
    default: {
      create: vi.fn(() => ({
        post: postMock,
        get: getMock,
        interceptors: {
          request: {
            use: vi.fn(),
          },
        },
      })),
    },
    postMock,
    getMock,
  }
})

import * as axiosMock from 'axios'
import ChatService from '../ChatService'

const postMock = (axiosMock as any).postMock as ReturnType<typeof vi.fn>
const getMock = (axiosMock as any).getMock as ReturnType<typeof vi.fn>

describe('ChatService', () => {
  afterEach(() => {
    vi.resetAllMocks()
  })

  it('gets all chats', async () => {
    getMock.mockResolvedValue({ data: [{ id: 1, messages: [] }] })
    const result = await ChatService.getChats()
    expect(result).toEqual([{ id: 1, messages: [] }])
    expect(getMock).toHaveBeenCalledWith('/chats')
  })

  it('gets one chat with messages', async () => {
    getMock.mockResolvedValue({ data: { id: 1, messages: [{ id: 1, content: 'Hello' }] } })
    const result = await ChatService.getChat(1)
    expect(result).toEqual({ id: 1, messages: [{ id: 1, content: 'Hello' }] })
    expect(getMock).toHaveBeenCalledWith('/chats/1')
  })

  it('creates or gets a chat', async () => {
    postMock.mockResolvedValue({ data: { id: 1 } })
    const result = await ChatService.createOrGetChat(2)
    expect(result).toEqual({ id: 1 })
    expect(postMock).toHaveBeenCalledWith('/chats', { otherUserId: 2, taskId: undefined })
  })

  it('sends a message', async () => {
    postMock.mockResolvedValue({ data: { id: 1, senderId: 1, content: 'Hello', sentAt: 'now' } })
    const result = await ChatService.sendMessage(1, 'Hello')
    expect(result).toEqual({ id: 1, senderId: 1, content: 'Hello', sentAt: 'now' })
    expect(postMock).toHaveBeenCalledWith('/chats/1/messages', { content: 'Hello' })
  })

  it('gets chat participants', async () => {
    getMock.mockResolvedValue({ data: [{ id: 2 }] })
    const result = await ChatService.getChatParticipants()
    expect(result).toEqual([{ id: 2 }])
    expect(getMock).toHaveBeenCalledWith('/chats/participants')
  })
})
