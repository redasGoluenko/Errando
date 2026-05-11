import { describe, it, expect, vi, afterEach } from 'vitest'
import apiClient from '../api'
import { userService } from '../userService'

describe('userService', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('fetches all users', async () => {
    const users = [{ id: 1, username: 'test', email: 't@t.com', role: 'Client' }]
    vi.spyOn(apiClient, 'get').mockResolvedValue({ data: users } as any)
    const result = await userService.getAllUsers()
    expect(result).toEqual(users)
  })

  it('creates a user', async () => {
    const user = { id: 1, username: 'test', email: 't@t.com', role: 'Admin' }
    vi.spyOn(apiClient, 'post').mockResolvedValue({ data: user } as any)
    const result = await userService.createUser({ username: 'test', email: 't@t.com', password: 'pwd', role: 'Admin' })
    expect(result).toEqual(user)
  })

  it('updates a user with payload id', async () => {
    const user = { id: 1, username: 'updated', email: 't@t.com', role: 'Admin' }
    vi.spyOn(apiClient, 'patch').mockResolvedValue({ data: user } as any)
    const result = await userService.updateUser(1, { username: 'updated' })
    expect(result).toEqual(user)
    expect(apiClient.patch).toHaveBeenCalledWith('/Users/1', { id: 1, username: 'updated' })
  })
})
