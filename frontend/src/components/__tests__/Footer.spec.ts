import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import Footer from '../Footer.vue'

describe('Footer.vue', () => {
  it('renders footer content and current year', () => {
    const wrapper = mount(Footer, {
      global: {
        stubs: ['router-link'],
      },
    })
    expect(wrapper.text()).toContain('Errando')
    expect(wrapper.text()).toContain(new Date().getFullYear().toString())
  })
})
