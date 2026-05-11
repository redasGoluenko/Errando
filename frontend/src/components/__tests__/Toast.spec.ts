import { describe, it, expect, vi } from 'vitest'
import { mount } from '@vue/test-utils'
import Toast from '../Toast.vue'

describe('Toast.vue', () => {
  it('renders a toast and closes after duration', async () => {
    vi.useFakeTimers()

    const wrapper = mount(Toast, {
      props: {
        show: false,
        type: 'warning',
        message: 'Test warning',
        duration: 100,
      },
      global: {
        stubs: {
          Transition: true,
        },
      },
    })

    await wrapper.setProps({ show: true })
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Test warning')
    expect(wrapper.html()).toContain('bg-yellow-50')

    vi.advanceTimersByTime(100)
    await wrapper.vm.$nextTick()
    vi.advanceTimersByTime(300)
    await wrapper.vm.$nextTick()

    expect(wrapper.emitted('close')).toBeDefined()

    vi.useRealTimers()
  })
})
