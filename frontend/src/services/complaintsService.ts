import apiClient from './api'

export interface Complaint {
  id: number
  description: string
  taskId: number
  taskTitle: string
  clientId: number
  clientUsername: string
  runnerId: number
  runnerUsername: string
  createdAt: string
}

export interface CreateComplaintRequest {
  description: string
  taskId: number
}

export const complaintsService = {
  // Get all complaints (Admin only)
  async getAllComplaints(): Promise<Complaint[]> {
    console.log('📤 GET ALL COMPLAINTS')
    try {
      const response = await apiClient.get<Complaint[]>('/Complaints')
      console.log('✅ GET COMPLAINTS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET COMPLAINTS ERROR:', error.response?.data)
      throw error
    }
  },

  // Create complaint (Client only)
  async createComplaint(data: CreateComplaintRequest): Promise<Complaint> {
    console.log('📤 CREATE COMPLAINT:', data)
    try {
      const response = await apiClient.post<Complaint>('/Complaints', data)
      console.log('✅ CREATE COMPLAINT SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE COMPLAINT ERROR:', error.response?.data)
      throw error
    }
  },

  // Delete complaint (Admin only)
  async deleteComplaint(id: number): Promise<void> {
    console.log('📤 DELETE COMPLAINT:', id)
    try {
      await apiClient.delete(`/Complaints/${id}`)
      console.log('✅ DELETE COMPLAINT SUCCESS')
    } catch (error: any) {
      console.error('❌ DELETE COMPLAINT ERROR:', error.response?.data)
      throw error
    }
  }
}
