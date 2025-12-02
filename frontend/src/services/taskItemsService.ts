import apiClient from './api'

export interface TaskItem {
  id: number
  description: string
  status: string
  isCompleted: boolean
  taskId: number
  createdAt: string
  updatedAt: string
}

export interface CreateTaskItemRequest {
  description: string
  taskId: number
}

export interface UpdateTaskItemRequest {
  description: string
  isCompleted: boolean
  taskId: number
}

export const taskItemsService = {
  async getTaskItemsByTaskId(taskId: number): Promise<TaskItem[]> {
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
    console.log('📤 UPDATE TASK ITEM REQUEST:', id, data)

    // Backend expects full TaskItem object with Id
    const fullItem = {
      id: id,
      description: data.description,
      isCompleted: data.isCompleted,
      taskId: data.taskId,
      status: 'Pending', // Default status
    }

    const response = await apiClient.patch<TaskItem>(`/TaskItems/${id}`, fullItem)
    console.log('✅ UPDATE TASK ITEM SUCCESS:', response.data)
    return response.data
  },

  async deleteTaskItem(id: number): Promise<void> {
    console.log('📤 DELETE TASK ITEM:', id)
    await apiClient.delete(`/TaskItems/${id}`)
    console.log('✅ DELETE TASK ITEM SUCCESS')
  },

  async toggleComplete(id: number, isCompleted: boolean): Promise<void> {
    console.log('📤 TOGGLE TASK ITEM COMPLETE:', id, isCompleted)
    const endpoint = isCompleted ? `/TaskItems/${id}/complete` : `/TaskItems/${id}/incomplete`
    await apiClient.patch(endpoint)
    console.log('✅ TOGGLE COMPLETE SUCCESS')
  },
}