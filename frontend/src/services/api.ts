import axios from 'axios'

// ← CHANGE to the new URL:
const API_BASE_URL = 'https://errando-app-01-gqa9bvctezcgg0h4.swedencentral-01.azurewebsites.net/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  userId: number
  username: string
  role: string
}

export const authService = {
  async login(credentials: { username: string; password: string }) {
    const response = await apiClient.post('/Users/login', credentials) // ← FIX: /Users/login
    const { token, username, role, userId } = response.data
    localStorage.setItem('token', token)
    localStorage.setItem('username', username)
    localStorage.setItem('role', role)
    localStorage.setItem('userId', userId?.toString() || '')
    return response.data
  },

  async register(data: RegisterRequest): Promise<{ id: number; username: string; email: string }> {
    const response = await apiClient.post('/Users/register', data)
    return response.data
  },

  logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('role')
    localStorage.removeItem('userId')
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token')
  },

  getRole(): string | null {
    return localStorage.getItem('role')
  },

  getUsername(): string | null {
    return localStorage.getItem('username')
  },

  getUserId(): number | null {
    const id = localStorage.getItem('userId')
    return id ? parseInt(id) : null
  },
}

export default apiClient