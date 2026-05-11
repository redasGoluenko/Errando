import { shallowMount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import App from '../App.vue'

describe('App.vue', () => {
  it('renders the app shell', () => {
    const wrapper = shallowMount(App, {
      global: {
        stubs: {
          Navbar: { template: '<nav />' },
          Footer: { template: '<footer />' },
          RouterView: { template: '<section />' },
        },
      },
    })

    expect(wrapper.find('nav').exists()).toBe(true)
    expect(wrapper.find('main').exists()).toBe(true)
    expect(wrapper.find('footer').exists()).toBe(true)
  })
})
