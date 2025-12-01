import apiClient from './api'

export interface TaskItem {
  id: number
  description: string
  isCompleted: boolean
  taskId: number
}

export interface CreateTaskItemRequest {
  description: string
  isCompleted: boolean
  taskId: number
}

export interface UpdateTaskItemRequest {
  id: number
  description: string
  isCompleted: boolean
  taskId: number
}

export const taskItemsService = {
  // Get all task items for a specific task
  async getTaskItemsByTaskId(taskId: number): Promise<TaskItem[]> {
    console.log('📤 GET TASK ITEMS FOR TASK:', taskId)
    try {
      const response = await apiClient.get<TaskItem[]>(`/TaskItems?taskId=${taskId}`)
      console.log('✅ GET TASK ITEMS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET TASK ITEMS ERROR:', error.response?.data)
      throw error
    }
  },

  // Get single task item by ID
  async getTaskItemById(id: number): Promise<TaskItem> {
    console.log('📤 GET TASK ITEM BY ID:', id)
    try {
      const response = await apiClient.get<TaskItem>(`/TaskItems/${id}`)
      console.log('✅ GET TASK ITEM SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET TASK ITEM ERROR:', error.response?.data)
      throw error
    }
  },

  // Create new task item
  async createTaskItem(data: CreateTaskItemRequest): Promise<TaskItem> {
    console.log('📤 CREATE TASK ITEM REQUEST:', data)
    try {
      // Add id: 0 to satisfy backend validation
      const payload = {
        id: 0, // ← ADD THIS
        ...data,
      }
      const response = await apiClient.post<TaskItem>('/TaskItems', payload)
      console.log('✅ CREATE TASK ITEM SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE TASK ITEM ERROR:', error.response?.data)
      throw error
    }
  },

  // Update task item
  async updateTaskItem(id: number, data: UpdateTaskItemRequest): Promise<TaskItem> {
    console.log('📤 UPDATE TASK ITEM REQUEST:', { id, data })
    try {
      const response = await apiClient.patch<TaskItem>(`/TaskItems/${id}`, data)
      console.log('✅ UPDATE TASK ITEM SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ UPDATE TASK ITEM ERROR:', error.response?.data)
      throw error
    }
  },

  // Delete task item
  async deleteTaskItem(id: number): Promise<void> {
    console.log('📤 DELETE TASK ITEM REQUEST:', id)
    try {
      await apiClient.delete(`/TaskItems/${id}`)
      console.log('✅ DELETE TASK ITEM SUCCESS')
    } catch (error: any) {
      console.error('❌ DELETE TASK ITEM ERROR:', error.response?.data)
      throw error
    }
  },

  // Toggle completion status
  async toggleComplete(id: number, isCompleted: boolean): Promise<TaskItem> {
    console.log('📤 TOGGLE TASK ITEM COMPLETE:', { id, isCompleted })
    try {
      const endpoint = isCompleted
        ? `/TaskItems/${id}/complete`
        : `/TaskItems/${id}/incomplete`
      const response = await apiClient.patch<TaskItem>(endpoint)
      console.log('✅ TOGGLE COMPLETE SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ TOGGLE COMPLETE ERROR:', error.response?.data)
      throw error
    }
  },
}