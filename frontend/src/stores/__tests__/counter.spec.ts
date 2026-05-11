import { setActivePinia, createPinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import { useCounterStore } from '../counter'

describe('counter store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('increments and exposes the doubled count', () => {
    const store = useCounterStore()

    expect(store.count).toBe(0)
    expect(store.doubleCount).toBe(0)

    store.increment()

    expect(store.count).toBe(1)
    expect(store.doubleCount).toBe(2)
  })
})
