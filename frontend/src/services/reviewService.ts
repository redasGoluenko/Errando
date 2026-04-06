import apiClient from './api'

export interface Review {
  id: number
  taskId: number
  reviewerId: number
  reviewerUsername?: string
  revieweeId: number
  revieweeUsername?: string
  starRating: number
  reviewText?: string
  finalRating: number
  createdAt: string
  updatedAt: string
}

export interface CreateReviewRequest {
  taskId: number
  revieweeId: number
  starRating: number
  reviewText?: string
}

export interface UserReviewsResponse {
  userId: number
  username?: string
  averageRating: number
  totalReviews: number
  reviews: Review[]
}

export const reviewService = {
  // Create a new review
  async createReview(data: CreateReviewRequest): Promise<Review> {
    console.log('📤 CREATE REVIEW REQUEST:', data)
    try {
      const response = await apiClient.post<Review>('/Reviews', data)
      console.log('✅ CREATE REVIEW SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE REVIEW ERROR:', error.response?.data)
      throw error
    }
  },

  // Get reviews for a specific task
  async getTaskReviews(taskId: number): Promise<Review[]> {
    console.log('📤 GET TASK REVIEWS:', taskId)
    try {
      const response = await apiClient.get<Review[]>(`/Reviews/task/${taskId}`)
      console.log('✅ GET TASK REVIEWS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET TASK REVIEWS ERROR:', error.response?.data)
      throw error
    }
  },

  // Get reviews for a specific user (their rating and reviews)
  async getUserReviews(userId: number): Promise<UserReviewsResponse> {
    console.log('📤 GET USER REVIEWS:', userId)
    try {
      const response = await apiClient.get<UserReviewsResponse>(`/Reviews/user/${userId}`)
      console.log('✅ GET USER REVIEWS SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET USER REVIEWS ERROR:', error.response?.data)
      throw error
    }
  },

  // Get a single review
  async getReview(id: number): Promise<Review> {
    console.log('📤 GET REVIEW:', id)
    try {
      const response = await apiClient.get<Review>(`/Reviews/${id}`)
      console.log('✅ GET REVIEW SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET REVIEW ERROR:', error.response?.data)
      throw error
    }
  }
}
