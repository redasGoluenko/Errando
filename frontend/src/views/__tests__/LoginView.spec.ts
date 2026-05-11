import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import LoginView from '../LoginView.vue'
import { authService } from '../../services/api'

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
}))

describe('LoginView.vue', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('shows validation error when fields are empty', async () => {
    const wrapper = mount(LoginView, {
      global: { stubs: ['router-link'] },
    })
    await wrapper.find('form').trigger('submit.prevent')
    expect(wrapper.text()).toContain('Username and password are required')
  })

  it('calls authService.login on valid submit', async () => {
    const loginSpy = vi.spyOn(authService, 'login').mockResolvedValue({ token: 't', username: 'test', role: 'Client', userId: 1 } as any)
    const wrapper = mount(LoginView, {
      global: { stubs: ['router-link'] },
    })
    await wrapper.find('#username').setValue('test')
    await wrapper.find('#password').setValue('pass')
    await wrapper.find('form').trigger('submit.prevent')
    expect(loginSpy).toHaveBeenCalledWith({ username: 'test', password: 'pass' })
  })
})
