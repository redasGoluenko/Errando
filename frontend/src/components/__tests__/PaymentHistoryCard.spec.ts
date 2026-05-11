import { describe, it, expect, vi, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import PaymentHistoryCard from '../PaymentHistoryCard.vue'
import { paymentService } from '../../services/paymentService'

describe('PaymentHistoryCard.vue', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads payments and displays them', async () => {
    const payments = [
      { id: 1, taskId: 10, amount: 50, currency: 'EUR', status: 'succeeded', createdAt: new Date().toISOString() },
    ]
    vi.spyOn(paymentService, 'getMyPayments').mockResolvedValue(payments)

    const wrapper = mount(PaymentHistoryCard, {
      global: {
        stubs: ['router-link'],
      },
    })

    await flushPromises()

    expect(wrapper.text()).toContain('Payment History')
    expect(wrapper.text()).toContain('Payment #1')
  })
})
