import apiClient from './api'

export interface TaskItem {
  id: number
  taskId: number
  description: string
  isCompleted: boolean
}

export interface CreateTaskItemRequest {
  taskId: number
  description: string
  isCompleted: boolean
}

export interface UpdateTaskItemRequest {
  description: string
  isCompleted: boolean
  // ← DON'T include 'id' here - it's passed as a parameter to updateTaskItem()
}

export const taskItemsService = {
  async getTaskItems(taskId: number): Promise<TaskItem[]> {
    console.log('📤 GET TASK ITEMS FOR TASK:', taskId)
    const response = await apiClient.get<TaskItem[]>(`/TaskItems?taskId=${taskId}`)
    console.log('✅ GET TASK ITEMS SUCCESS:', response.data)
    return response.data
  },

  async createTaskItem(data: CreateTaskItemRequest): Promise<TaskItem> {
    console.log('📤 CREATE TASK ITEM:', data)
    const response = await apiClient.post<TaskItem>('/TaskItems', data)
    console.log('✅ CREATE TASK ITEM SUCCESS:', response.data)
    return response.data
  },

  async updateTaskItem(id: number, data: UpdateTaskItemRequest): Promise<TaskItem> {
    console.log('📤 UPDATE TASK ITEM:', id, data)
    const response = await apiClient.put<TaskItem>(`/TaskItems/${id}`, data)
    console.log('✅ UPDATE TASK ITEM SUCCESS:', response.data)
    return response.data
  },

  async deleteTaskItem(id: number): Promise<void> {
    console.log('📤 DELETE TASK ITEM:', id)
    await apiClient.delete(`/TaskItems/${id}`)
    console.log('✅ DELETE TASK ITEM SUCCESS')
  },
}