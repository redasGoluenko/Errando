import { describe, it, expect, vi, afterEach } from 'vitest'
import apiClient from '../api'
import { tasksService } from '../tasksService'

describe('tasksService', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('fetches all tasks', async () => {
    const tasks = [{ id: 1, title: 'Task', description: 'Test', scheduledTime: '', status: 'Pending', clientId: 1, isRecurring: false, isExpired: false, createdAt: '', updatedAt: '', isCompleted: false }]
    vi.spyOn(apiClient, 'get').mockResolvedValue({ data: tasks } as any)

    const result = await tasksService.getAllTasks()
    expect(result).toEqual(tasks)
  })

  it('fetches one task by id', async () => {
    const task = { id: 1, title: 'Task', description: 'Test', scheduledTime: '', status: 'Pending', clientId: 1, isRecurring: false, isExpired: false, createdAt: '', updatedAt: '', isCompleted: false }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: task } as any)

    const result = await tasksService.getTaskById(1)
    expect(result).toEqual(task)
    expect(get).toHaveBeenCalledWith('/Tasks/1')
  })

  it('creates a task', async () => {
    const created = { id: 1, title: 'Task', description: 'Test', scheduledTime: '', status: 'Pending', clientId: 1, isRecurring: false, isExpired: false, createdAt: '', updatedAt: '', isCompleted: false }
    const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: created } as any)

    const result = await tasksService.createTask({ title: 'Task', description: 'Test', scheduledTime: '', clientId: 1 })
    expect(result).toEqual(created)
    expect(post).toHaveBeenCalledWith('/Tasks', { title: 'Task', description: 'Test', scheduledTime: '', clientId: 1 })
  })

  it('updates a task', async () => {
    const updated = { id: 1, title: 'Task', description: 'Updated', scheduledTime: '', status: 'Pending', clientId: 1, isRecurring: false, isExpired: false, createdAt: '', updatedAt: '', isCompleted: false }
    vi.spyOn(apiClient, 'patch').mockResolvedValue({ data: updated } as any)

    const result = await tasksService.updateTask(1, { id: 1, title: 'Task', description: 'Updated', scheduledTime: '', clientId: 1 })
    expect(result).toEqual(updated)
  })

  it('deletes a task', async () => {
    vi.spyOn(apiClient, 'delete').mockResolvedValue({} as any)
    await expect(tasksService.deleteTask(1)).resolves.toBeUndefined()
  })

  it('assigns and unassigns a task', async () => {
    const task = { id: 1, title: 'Task', description: 'Test', scheduledTime: '', status: 'Pending', clientId: 1, isRecurring: false, isExpired: false, createdAt: '', updatedAt: '', isCompleted: false }
    const patch = vi.spyOn(apiClient, 'patch').mockResolvedValue({ data: task } as any)

    await expect(tasksService.assignTask(1)).resolves.toEqual(task)
    await expect(tasksService.unassignTask(1)).resolves.toEqual(task)
    expect(patch).toHaveBeenCalledWith('/Tasks/1/assign')
    expect(patch).toHaveBeenCalledWith('/Tasks/1/unassign')
  })
})
