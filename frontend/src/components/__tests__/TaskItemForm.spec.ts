import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TaskItemForm from '../TaskItemForm.vue'

describe('TaskItemForm.vue', () => {
  it('creates a task item and emits submit', async () => {
    const wrapper = mount(TaskItemForm, {
      props: {
        taskId: 1,
      },
    })

    await wrapper.find('textarea').setValue('Item description')
    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.emitted('submit')).toBeDefined()
  })
})
