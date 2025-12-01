import axios from 'axios'

// Azure backend URL arba local
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5064/api'

// Axios instance
const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
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
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/Users/login', data)
    // Save to localStorage
    localStorage.setItem('token', response.data.token)
    localStorage.setItem('userId', response.data.userId.toString())
    localStorage.setItem('username', response.data.username)
    localStorage.setItem('role', response.data.role)
    return response.data
  },

  async register(data: RegisterRequest): Promise<{ id: number; username: string; email: string }> {
    const response = await apiClient.post('/Users/register', data)
    return response.data
  },

  logout() {
    localStorage.removeItem('token')
    localStorage.removeItem('userId')
    localStorage.removeItem('username')
    localStorage.removeItem('role')
  },

  getToken(): string | null {
    return localStorage.getItem('token')
  },

  getUserId(): number | null {
    const id = localStorage.getItem('userId')
    return id ? parseInt(id) : null
  },

  getRole(): string | null {
    return localStorage.getItem('role')
  },

  getUsername(): string | null {
    return localStorage.getItem('username')
  },

  isAuthenticated(): boolean {
    return !!this.getToken()
  },
}

// Add JWT token to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export default apiClient