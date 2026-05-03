import apiClient from './api'

export interface ClientStats {
  id: number
  username: string
  rating: number
  totalReviews: number
  tasksCreated: number
  tasksCompleted: number
  activeTasks: number
  totalSpent: number
  complaintsFiled: number
  completionRate: number
}

export async function getClientStats(): Promise<ClientStats[]> {
  try {
    const response = await apiClient.get('/Users/clients/stats')
    console.log('📊 Fetched client stats:', response.data)
    return response.data
  } catch (error) {
    console.error('Failed to fetch client stats:', error)
    throw error
  }
}
