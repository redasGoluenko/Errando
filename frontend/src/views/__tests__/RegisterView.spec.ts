import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import RegisterView from '../RegisterView.vue'
import { authService } from '../../services/api'

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

describe('RegisterView.vue', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('shows validation error for missing fields', async () => {
    const wrapper = mount(RegisterView, {
      global: { stubs: ['router-link'] },
    })
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.text()).toContain('All fields are required')
  })

  it('shows password mismatch error', async () => {
    const wrapper = mount(RegisterView, {
      global: { stubs: ['router-link'] },
    })
    await wrapper.find('#username').setValue('test')
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('pass123')
    await wrapper.find('#confirmPassword').setValue('pass321')
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.text()).toContain('Passwords do not match')
  })

  it('calls authService.register on valid submit', async () => {
    const registerSpy = vi.spyOn(authService, 'register').mockResolvedValue({ id: 1, username: 'test', email: 'test@example.com' } as any)
    const wrapper = mount(RegisterView, {
      global: { stubs: ['router-link'] },
    })
    await wrapper.find('#username').setValue('test')
    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#password').setValue('pass123')
    await wrapper.find('#confirmPassword').setValue('pass123')
    await wrapper.find('form').trigger('submit.prevent')
    expect(registerSpy).toHaveBeenCalled()
  })
})
