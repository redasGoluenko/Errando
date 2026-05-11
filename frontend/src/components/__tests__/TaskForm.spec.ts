import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TaskForm from '../TaskForm.vue'

describe('TaskForm.vue', () => {
  it('emits submit with valid create task data', async () => {
    const wrapper = mount(TaskForm, {
      props: {
        mode: 'create',
      },
    })

    await wrapper.find('input[placeholder="e.g., Weekly grocery shopping"]').setValue('New Task')
    await wrapper.find('textarea[placeholder="Describe the task in detail..."]').setValue('Description')
    await wrapper.find('input[type="datetime-local"]').setValue('2026-05-12T10:00')
    await wrapper.find('select').setValue('Vilnius')
    await wrapper.find('input[placeholder="e.g., 25.00"]').setValue('100')

    await wrapper.find('form').trigger('submit.prevent')

    expect(wrapper.emitted('submit')).toBeDefined()
    const event = wrapper.emitted('submit')?.[0]
    expect(event).toBeDefined()
    expect(event?.[0]).toMatchObject({ title: 'New Task', description: 'Description', location: 'Vilnius' })
  })
})
