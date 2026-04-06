import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const userId = ref<number | null>(authService.getUserId())
  const username = ref<string | null>(authService.getUsername())
  const role = ref<string | null>(authService.getRole())
  const isAuthenticated = ref(authService.isAuthenticated())

  const user = computed(() => {
    if (!isAuthenticated.value) return null
    return {
      id: userId.value || 0,
      username: username.value || '',
      role: role.value || ''
    }
  })

  const login = async (credentials: { username: string; password: string }) => {
    const response = await authService.login(credentials)
    userId.value = response.userId
    username.value = response.username
    role.value = response.role
    isAuthenticated.value = true
    return response
  }

  const register = async (data: { username: string; email: string; password: string }) => {
    const response = await authService.register(data)
    return response
  }

  const logout = () => {
    authService.logout()
    userId.value = null
    username.value = null
    role.value = null
    isAuthenticated.value = false
  }

  return {
    userId,
    username,
    role,
    isAuthenticated,
    user,
    login,
    register,
    logout
  }
})
