import { afterEach, describe, expect, it, vi } from 'vitest'
import apiClient from '../api'
import { getAdminStats } from '../adminStatsService'
import { getClientStats } from '../clientStatsService'
import { complaintsService } from '../complaintsService'
import { getRunnerStats } from '../runnerStatsService'
import { statusLogsService } from '../statusLogsService'
import { taskItemsService } from '../taskItemsService'

describe('additional API services', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('fetches admin stats', async () => {
    const stats = { totalUsers: 2, totalTasks: 3 }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: stats } as any)

    await expect(getAdminStats()).resolves.toEqual(stats)
    expect(get).toHaveBeenCalledWith('/Users/admin/stats')
  })

  it('fetches client stats', async () => {
    const stats = [{ id: 1, username: 'client', rating: 4 }]
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: stats } as any)

    await expect(getClientStats()).resolves.toEqual(stats)
    expect(get).toHaveBeenCalledWith('/Users/clients/stats')
  })

  it('fetches runner stats', async () => {
    const stats = [{ id: 2, username: 'runner', rating: 5 }]
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: stats } as any)

    await expect(getRunnerStats()).resolves.toEqual(stats)
    expect(get).toHaveBeenCalledWith('/Users/runners/stats')
  })

  it('covers complaint operations', async () => {
    const complaint = { id: 1, description: 'Missing item', isResolved: false }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: [complaint] } as any)
    const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: complaint } as any)
    const patch = vi.spyOn(apiClient, 'patch').mockResolvedValue({ data: { ...complaint, isResolved: true } } as any)
    const del = vi.spyOn(apiClient, 'delete').mockResolvedValue({} as any)

    await expect(complaintsService.getAllComplaints()).resolves.toEqual([complaint])
    await expect(complaintsService.createComplaint({ taskId: 1, description: 'Missing item' })).resolves.toEqual(complaint)
    await expect(complaintsService.resolveComplaint(1)).resolves.toEqual({ ...complaint, isResolved: true })
    await expect(complaintsService.deleteComplaint(1)).resolves.toBeUndefined()

    expect(get).toHaveBeenCalledWith('/Complaints')
    expect(post).toHaveBeenCalledWith('/Complaints', { taskId: 1, description: 'Missing item' })
    expect(patch).toHaveBeenCalledWith('/Complaints/1/resolve')
    expect(del).toHaveBeenCalledWith('/Complaints/1')
  })

  it('covers task item operations', async () => {
    const item = { id: 1, taskId: 1, description: 'Buy milk', isCompleted: false }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: [item] } as any)
    const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: item } as any)
    const put = vi.spyOn(apiClient, 'put').mockResolvedValue({ data: { ...item, isCompleted: true } } as any)
    const del = vi.spyOn(apiClient, 'delete').mockResolvedValue({} as any)

    await expect(taskItemsService.getTaskItems(1)).resolves.toEqual([item])
    await expect(taskItemsService.createTaskItem({ taskId: 1, description: 'Buy milk', isCompleted: false })).resolves.toEqual(item)
    await expect(taskItemsService.updateTaskItem(1, { description: 'Buy milk', isCompleted: true })).resolves.toEqual({ ...item, isCompleted: true })
    await expect(taskItemsService.deleteTaskItem(1)).resolves.toBeUndefined()

    expect(get).toHaveBeenCalledWith('/TaskItems?taskId=1')
    expect(post).toHaveBeenCalledWith('/TaskItems', { taskId: 1, description: 'Buy milk', isCompleted: false })
    expect(put).toHaveBeenCalledWith('/TaskItems/1', { description: 'Buy milk', isCompleted: true })
    expect(del).toHaveBeenCalledWith('/TaskItems/1')
  })

  it('covers status log operations', async () => {
    const log = { id: 1, taskItemId: 1, status: 'Done', comment: 'Finished', timestamp: '2026-05-11', runnerId: 2 }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: [log] } as any)
    const post = vi.spyOn(apiClient, 'post').mockResolvedValue({ data: log } as any)
    const put = vi.spyOn(apiClient, 'put').mockResolvedValue({ data: { ...log, status: 'Blocked' } } as any)
    const del = vi.spyOn(apiClient, 'delete').mockResolvedValue({} as any)

    await expect(statusLogsService.getStatusLogs(1)).resolves.toEqual([log])
    await expect(statusLogsService.createStatusLog({ taskItemId: 1, status: 'Done', comment: 'Finished' })).resolves.toEqual(log)
    await expect(statusLogsService.updateStatusLog(1, { status: 'Blocked' })).resolves.toEqual({ ...log, status: 'Blocked' })
    await expect(statusLogsService.deleteStatusLog(1)).resolves.toBeUndefined()

    expect(get).toHaveBeenCalledWith('/StatusLogs?taskItemId=1')
    expect(post).toHaveBeenCalledWith('/StatusLogs', { taskItemId: 1, status: 'Done', comment: 'Finished' })
    expect(put).toHaveBeenCalledWith('/StatusLogs/1', { status: 'Blocked' })
    expect(del).toHaveBeenCalledWith('/StatusLogs/1')
  })
})
