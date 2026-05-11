import { describe, it, expect, vi, afterEach } from 'vitest'
import apiClient from '../api'
import { paymentService } from '../paymentService'

describe('paymentService', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('creates a payment intent', async () => {
    const data = { paymentId: 1, clientSecret: 'secret', paymentIntentId: 'pi_123', amount: 100, currency: 'EUR' }
    vi.spyOn(apiClient, 'post').mockResolvedValue({ data } as any)
    const result = await paymentService.createPaymentIntent({ taskId: 1, amount: 100 })
    expect(result).toEqual(data)
  })

  it('confirms a payment', async () => {
    const payment = { id: 1, taskId: 1, clientId: 1, amount: 100, currency: 'EUR', status: 'succeeded', createdAt: '', updatedAt: '' }
    vi.spyOn(apiClient, 'post').mockResolvedValue({ data: payment } as any)
    const result = await paymentService.confirmPayment({ paymentId: 1, paymentIntentId: 'pi_123' })
    expect(result).toEqual(payment)
  })

  it('gets my payments', async () => {
    const payments = [{ id: 1, taskId: 1, clientId: 1, amount: 100, currency: 'EUR', status: 'succeeded', createdAt: '', updatedAt: '' }]
    vi.spyOn(apiClient, 'get').mockResolvedValue({ data: payments } as any)
    const result = await paymentService.getMyPayments()
    expect(result).toEqual(payments)
  })

  it('gets payment history for a task', async () => {
    const payments = [{ id: 1, taskId: 7, clientId: 1, amount: 100, currency: 'EUR', status: 'succeeded', createdAt: '', updatedAt: '' }]
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: payments } as any)
    const result = await paymentService.getPaymentHistory(7)
    expect(result).toEqual(payments)
    expect(get).toHaveBeenCalledWith('/Payments/task/7')
  })

  it('checks whether a task has been paid', async () => {
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: { hasPaid: true } } as any)
    const result = await paymentService.hasPaid(7)
    expect(result).toBe(true)
    expect(get).toHaveBeenCalledWith('/Payments/7/paid')
  })
})
