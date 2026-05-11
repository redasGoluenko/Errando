import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import ReviewForm from '../ReviewForm.vue'

describe('ReviewForm.vue', () => {
  it('emits submit when valid data is entered', async () => {
    const wrapper = mount(ReviewForm, {
      props: {
        task: { id: 1 } as any,
        revieweeId: 2,
        revieweeUsername: 'other',
      },
    })

    await wrapper.find('textarea').setValue('Great work')
    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.emitted('submit')).toBeDefined()
  })
})
