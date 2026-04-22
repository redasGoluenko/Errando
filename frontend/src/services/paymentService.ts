import apiClient from './api'

export interface Payment {
  id: number
  taskId: number
  clientId: number
  amount: number
  currency: string
  status: string
  stripePaymentIntentId?: string
  createdAt: string
  updatedAt?: string
}

export interface CreatePaymentRequest {
  taskId: number
  amount: number
}

export interface PaymentIntentResponse {
  paymentId: number
  clientSecret: string
  paymentIntentId: string
  amount: number
  currency: string
}

export interface ConfirmPaymentRequest {
  paymentId: number
  paymentIntentId: string
}

export const paymentService = {
  // Create a payment intent for a task
  async createPaymentIntent(data: CreatePaymentRequest): Promise<PaymentIntentResponse> {
    console.log('📤 CREATE PAYMENT INTENT:', data)
    try {
      const response = await apiClient.post<PaymentIntentResponse>('/Payments/create-intent', data)
      console.log('✅ CREATE PAYMENT INTENT SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CREATE PAYMENT INTENT ERROR:', error.response?.data)
      throw error
    }
  },

  // Confirm payment after Stripe processing
  async confirmPayment(data: ConfirmPaymentRequest): Promise<Payment> {
    console.log('📤 CONFIRM PAYMENT:', data)
    try {
      const response = await apiClient.post<Payment>('/Payments/confirm', data)
      console.log('✅ CONFIRM PAYMENT SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ CONFIRM PAYMENT ERROR:', error.response?.data)
      throw error
    }
  },

  // Get payment history for a task
  async getPaymentHistory(taskId: number): Promise<Payment[]> {
    console.log('📤 GET PAYMENT HISTORY:', taskId)
    try {
      const response = await apiClient.get<Payment[]>(`/Payments/task/${taskId}`)
      console.log('✅ GET PAYMENT HISTORY SUCCESS:', response.data)
      return response.data
    } catch (error: any) {
      console.error('❌ GET PAYMENT HISTORY ERROR:', error.response?.data)
      throw error
    }
  },

  // Check if a task has been paid
  async hasPaid(taskId: number): Promise<boolean> {
    console.log('📤 CHECK PAYMENT STATUS:', taskId)
    try {
      const response = await apiClient.get<{ hasPaid: boolean }>(`/Payments/${taskId}/paid`)
      console.log('✅ CHECK PAYMENT STATUS SUCCESS:', response.data)
      return response.data.hasPaid
    } catch (error: any) {
      console.error('❌ CHECK PAYMENT STATUS ERROR:', error.response?.data)
      throw error
    }
  },
}
