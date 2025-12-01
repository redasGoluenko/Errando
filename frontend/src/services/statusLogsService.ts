import apiClient from './api' // ← CHANGE FROM { apiClient }

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

export const statusLogsService = {
  async getStatusLogs(taskItemId: number): Promise<StatusLog[]> {
    console.log('📤 GET STATUS LOGS FOR TASK ITEM:', taskItemId)
    try {
      const response = await apiClient.get<StatusLog[]>(`/StatusLogs?taskItemId=${taskItemId}`)
      console.log('✅ GET STATUS LOGS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET STATUS LOGS ERROR:', error.response?.data)
      throw error
    }
  },

  async createStatusLog(data: CreateStatusLogRequest): Promise<StatusLog> {
    console.log('📤 CREATE STATUS LOG REQUEST:', data)
    try {
      const response = await apiClient.post<StatusLog>('/StatusLogs', data)
      console.log('✅ CREATE STATUS LOG SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE STATUS LOG ERROR:', error.response?.data)
      throw error
    }
  },
}