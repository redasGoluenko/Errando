import apiClient from './api'

export interface User {
  id: number
  username: string
  email: string
  role: 'Admin' | 'Client' | 'Runner'
}

export interface CreateUserRequest {
  username: string
  email: string
  password: string
  role: 'Admin' | 'Client' | 'Runner'
}

export interface UpdateUserRequest {
  username?: string
  email?: string
  password?: string
  role?: 'Admin' | 'Client' | 'Runner'
}

export const userService = {
  // Get all users (Admin only)
  async getAllUsers(): Promise<User[]> {
    const response = await apiClient.get<User[]>('/Users')
    return response.data
  },

  // Get single user by ID
  async getUserById(id: number): Promise<User> {
    const response = await apiClient.get<User>(`/Users/${id}`)
    return response.data
  },

  // Create new user (Admin only)
  async createUser(data: CreateUserRequest): Promise<User> {
    console.log('📤 CREATE USER REQUEST:', data)
    const response = await apiClient.post<User>('/Users', data)
    console.log('✅ CREATE USER RESPONSE:', response.data)
    return response.data
  },

  // Update user (Admin only)
  async updateUser(id: number, data: UpdateUserRequest): Promise<User> {
    console.log('📤 UPDATE USER REQUEST:', { id, data })
    
    // ✅ PRIDĖK ID Į PAYLOAD
    const payload = {
      id: id,  // <- Backend reikalauja Id match
      ...data
    }
    
    console.log('📤 UPDATE USER PAYLOAD:', JSON.stringify(payload, null, 2))
    
    try {
      const response = await apiClient.patch<User>(`/Users/${id}`, payload)
      console.log('✅ UPDATE USER RESPONSE:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ UPDATE USER ERROR:', {
        status: error.response?.status,
        data: error.response?.data
      })
      throw error
    }
  },

  // Delete user (Admin only)
  async deleteUser(id: number): Promise<void> {
    console.log('📤 DELETE USER REQUEST:', { id })
    await apiClient.delete(`/Users/${id}`)
    console.log('✅ DELETE USER SUCCESS')
  },
}