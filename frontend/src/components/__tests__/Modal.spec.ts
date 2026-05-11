import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Modal from '../Modal.vue'

describe('Modal.vue', () => {
  it('renders default header and emits close when the close button is clicked', async () => {
    const wrapper = mount(Modal, {
      props: {
        show: true,
        maxWidth: 'lg',
      },
    })

    expect(wrapper.text()).toContain('Modal')
    expect(wrapper.find('.max-w-lg').exists()).toBe(true)

    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('close')).toBeDefined()
  })
})
