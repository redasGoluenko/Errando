import { describe, it, expect, vi, afterEach } from 'vitest'
import apiClient from '../api'
import { reviewService } from '../reviewService'

describe('reviewService', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('creates a review', async () => {
    const review = { id: 1, taskId: 1, reviewerId: 1, revieweeId: 2, starRating: 5, finalRating: 5, createdAt: '', updatedAt: '' }
    vi.spyOn(apiClient, 'post').mockResolvedValue({ data: review } as any)
    const result = await reviewService.createReview({ taskId: 1, revieweeId: 2, starRating: 5 })
    expect(result).toEqual(review)
  })

  it('gets user reviews', async () => {
    const response = { data: { userId: 2, averageRating: 4.5, totalReviews: 3, reviews: [] } }
    vi.spyOn(apiClient, 'get').mockResolvedValue(response as any)
    const result = await reviewService.getUserReviews(2)
    expect(result).toEqual(response.data)
  })

  it('gets reviews for a task', async () => {
    const reviews = [{ id: 1, taskId: 7, reviewerId: 1, revieweeId: 2, starRating: 4, finalRating: 4, createdAt: '', updatedAt: '' }]
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: reviews } as any)
    const result = await reviewService.getTaskReviews(7)
    expect(result).toEqual(reviews)
    expect(get).toHaveBeenCalledWith('/Reviews/task/7')
  })

  it('gets a single review', async () => {
    const review = { id: 3, taskId: 7, reviewerId: 1, revieweeId: 2, starRating: 4, finalRating: 4, createdAt: '', updatedAt: '' }
    const get = vi.spyOn(apiClient, 'get').mockResolvedValue({ data: review } as any)
    const result = await reviewService.getReview(3)
    expect(result).toEqual(review)
    expect(get).toHaveBeenCalledWith('/Reviews/3')
  })
})
