import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import apiClient, { authService } from '../api'

describe('authService', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  afterEach(() => {
    vi.restoreAllMocks()
    localStorage.clear()
  })

  it('logs in and stores auth values', async () => {
    const response = { data: { token: 'tok', username: 'user', role: 'Client', userId: 123 } }
    vi.spyOn(apiClient, 'post').mockResolvedValue(response as any)

    const result = await authService.login({ username: 'user', password: 'pwd' })

    expect(result).toEqual(response.data)
    expect(localStorage.getItem('token')).toBe('tok')
    expect(localStorage.getItem('username')).toBe('user')
    expect(localStorage.getItem('role')).toBe('Client')
    expect(localStorage.getItem('userId')).toBe('123')
  })

  it('registers a new user', async () => {
    const response = { data: { id: 1, username: 'new', email: 'new@example.com' } }
    vi.spyOn(apiClient, 'post').mockResolvedValue(response as any)

    const result = await authService.register({ username: 'new', email: 'new@example.com', password: 'pwd' })

    expect(result).toEqual(response.data)
  })

  it('logout clears localStorage and auth getters work', () => {
    localStorage.setItem('token', 'x')
    localStorage.setItem('username', 'user')
    localStorage.setItem('role', 'Client')
    localStorage.setItem('userId', '5')

    authService.logout()

    expect(authService.isAuthenticated()).toBe(false)
    expect(authService.getUsername()).toBeNull()
    expect(authService.getRole()).toBeNull()
    expect(authService.getUserId()).toBeNull()
  })
})
