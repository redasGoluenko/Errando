import apiClient from './api'

export interface RunnerStats {
  id: number
  username: string
  rating: number
  totalReviews: number
  tasksCompleted: number
  activeTasks: number
  moneyEarned: number
  taskAcceptanceRate: number
  totalTasksAssigned: number
}

export async function getRunnerStats(): Promise<RunnerStats[]> {
  try {
    const response = await apiClient.get('/Users/runners/stats')
    console.log('📊 Fetched runner stats:', response.data)
    return response.data
  } catch (error) {
    console.error('Failed to fetch runner stats:', error)
    throw error
  }
}
