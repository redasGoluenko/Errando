import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import UserForm from '../UserForm.vue'

describe('UserForm.vue', () => {
  it('shows validation error when required fields are missing', async () => {
    const wrapper = mount(UserForm, {
      props: {
        mode: 'create',
      },
    })

    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.text()).toContain('Username, email, and role are required')
    expect(wrapper.emitted('submit')).toBeUndefined()
  })

  it('emits submit with valid create user data', async () => {
    const wrapper = mount(UserForm, {
      props: {
        mode: 'create',
      },
    })

    await wrapper.find('input[type="text"]').setValue('testuser')
    await wrapper.find('input[type="email"]').setValue('test@example.com')
    await wrapper.find('input[type="password"]').setValue('password123')
    await wrapper.find('select').setValue('Admin')

    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.emitted('submit')).toBeDefined()
    expect(wrapper.emitted('submit')?.[0]).toEqual([
      {
        username: 'testuser',
        email: 'test@example.com',
        password: 'password123',
        role: 'Admin',
      },
    ])
  })
})
