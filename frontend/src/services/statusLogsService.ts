import apiClient from './api'

export interface StatusLog {
  id: number
  taskItemId: number
  status: string
  comment: string
  timestamp: string
  runnerId: number
  runner?: {
    id: number
    username: string
    role: string
  }
}

export interface CreateStatusLogRequest {
  taskItemId: number
  status: string
  comment?: string
}

export interface UpdateStatusLogRequest {
  status: string
  comment?: string
}

export const statusLogsService = {
  async getStatusLogs(taskItemId: number): Promise<StatusLog[]> {
    console.log('📤 GET STATUS LOGS FOR TASK ITEM:', taskItemId)
    const response = await apiClient.get<StatusLog[]>(`/StatusLogs?taskItemId=${taskItemId}`)
    console.log('✅ GET STATUS LOGS SUCCESS:', response.data)
    return response.data
  },

  async createStatusLog(data: CreateStatusLogRequest): Promise<StatusLog> {
    console.log('📤 CREATE STATUS LOG:', data)
    const response = await apiClient.post<StatusLog>('/StatusLogs', data)
    console.log('✅ CREATE STATUS LOG SUCCESS:', response.data)
    return response.data
  },

  async updateStatusLog(id: number, data: UpdateStatusLogRequest): Promise<StatusLog> {
    console.log('📤 UPDATE STATUS LOG:', id, data)
    const response = await apiClient.put<StatusLog>(`/StatusLogs/${id}`, data)
    console.log('✅ UPDATE STATUS LOG SUCCESS:', response.data)
    return response.data
  },

  async deleteStatusLog(id: number): Promise<void> {
    console.log('📤 DELETE STATUS LOG:', id)
    await apiClient.delete(`/StatusLogs/${id}`)
    console.log('✅ DELETE STATUS LOG SUCCESS')
  },
}