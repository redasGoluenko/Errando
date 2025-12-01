import apiClient from './api'

export interface User {
  id: number
  username: string
  email: string
  role: string
}

export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
  role: string
}

export interface AuthResponse {
  token: string
  user: User
}

export const authService = {
  login(credentials: LoginRequest): Promise<AuthResponse> {
    return apiClient.post('/Users/login', credentials).then((res) => {
      localStorage.setItem('token', res.data.token)
      localStorage.setItem('user', JSON.stringify(res.data.user))
      return res.data
    })
  },

  register(data: RegisterRequest): Promise<AuthResponse> {
    return apiClient.post('/Users/register', data).then((res) => {
      localStorage.setItem('token', res.data.token)
      localStorage.setItem('user', JSON.stringify(res.data.user))
      return res.data
    })
  },

  logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token')
  },

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user')
    return userStr ? JSON.parse(userStr) : null
  },

  getToken(): string | null {
    return localStorage.getItem('token')
  },
}