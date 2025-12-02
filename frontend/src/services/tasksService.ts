import apiClient from './api'

export interface Task {
  id: number
  title: string
  description: string
  scheduledTime: string
  status: string  // ← ADD THIS
  clientId: number
  clientUsername?: string
  runnerId?: number | null
  runnerUsername?: string | null  // ← ADD THIS
  runner?: {
    id: number
    username: string
    role: string
  } | null
  taskItems?: any[]
  createdAt: string  // ← ADD THIS
  updatedAt: string  // ← ADD THIS
}

export interface CreateTaskRequest {
  title: string
  description: string
  scheduledTime: string
  clientId: number
}

export interface UpdateTaskRequest {
  id: number
  title: string
  description: string
  scheduledTime: string
  clientId: number
}

export const tasksService = {
  // Get all tasks (Admin sees all, Client sees only their own)
  async getAllTasks(): Promise<Task[]> {
    console.log('📤 GET ALL TASKS')
    try {
      const response = await apiClient.get<Task[]>('/Tasks')
      console.log('✅ GET TASKS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET TASKS ERROR:', error.response?.data)
      throw error
    }
  },

  // Get single task by ID
  async getTaskById(id: number): Promise<Task> {
    console.log('📤 GET TASK BY ID:', id)
    try {
      const response = await apiClient.get<Task>(`/Tasks/${id}`)
      console.log('✅ GET TASK SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET TASK ERROR:', error.response?.data)
      throw error
    }
  },

  // Create new task
  async createTask(data: CreateTaskRequest): Promise<Task> {
    console.log('📤 CREATE TASK REQUEST:', data)
    try {
      const response = await apiClient.post<Task>('/Tasks', data)
      console.log('✅ CREATE TASK SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE TASK ERROR:', error.response?.data)
      throw error
    }
  },

  // Update task
  async updateTask(id: number, data: UpdateTaskRequest): Promise<Task> {
    console.log('📤 UPDATE TASK REQUEST:', { id, data })
    
    // Ensure ID is in payload (backend requirement)
    const payload = {
      ...data,  // ← PERKELTI ČIA (data jau turi id: number)
      // id: id, ← PAŠALINTI (duplicatas)
    }
    
    try {
      const response = await apiClient.patch<Task>(`/Tasks/${id}`, payload)
      console.log('✅ UPDATE TASK SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ UPDATE TASK ERROR:', error.response?.data)
      throw error
    }
  },

  // Delete task
  async deleteTask(id: number): Promise<void> {
    console.log('📤 DELETE TASK REQUEST:', id)
    try {
      await apiClient.delete(`/Tasks/${id}`)
      console.log('✅ DELETE TASK SUCCESS')
    } catch (error: any) {
      console.error('❌ DELETE TASK ERROR:', error.response?.data)
      throw error
    }
  },

  // Assign task to runner (Runner only)
  async assignTask(taskId: number): Promise<Task> {
    console.log('📤 ASSIGN TASK REQUEST:', taskId)
    try {
      const response = await apiClient.patch<Task>(`/Tasks/${taskId}/assign`)
      console.log('✅ ASSIGN TASK SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ ASSIGN TASK ERROR:', error.response?.data)
      throw error
    }
  },

  // Unassign task from runner (Runner/Admin)
  async unassignTask(taskId: number): Promise<Task> {
    console.log('📤 UNASSIGN TASK REQUEST:', taskId)
    try {
      const response = await apiClient.patch<Task>(`/Tasks/${taskId}/unassign`)
      console.log('✅ UNASSIGN TASK SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ UNASSIGN TASK ERROR:', error.response?.data)
      throw error
    }
  },
}