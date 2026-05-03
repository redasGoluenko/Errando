import apiClient from './api'

export interface AdminStats {
  totalUsers: number
  activeUsers: number
  adminCount: number
  clientCount: number
  runnerCount: number
  
  totalTasks: number
  completedTasks: number
  activeTasks: number
  taskCompletionRate: number
  
  totalComplaints: number
  resolvedComplaints: number
  unresolvedComplaints: number
  complaintResolutionRate: number
  
  totalRevenue: number
  successfulPayments: number
  pendingPayments: number
  failedPayments: number
  
  averageSystemRating: number
  usersCreatedThisMonth: number
  tasksCreatedThisMonth: number
}

export async function getAdminStats(): Promise<AdminStats> {
  try {
    const response = await apiClient.get('/Users/admin/stats')
    console.log('📊 Fetched admin stats:', response.data)
    return response.data
  } catch (error) {
    console.error('Failed to fetch admin stats:', error)
    throw error
  }
}
