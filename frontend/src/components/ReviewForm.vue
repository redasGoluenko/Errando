<script setup lang="ts">
import { ref, watch } from 'vue'
import type { Task } from '@/services/tasksService'
import type { CreateReviewRequest } from '@/services/reviewService'
import { authService } from '@/services/api'

interface Props {
  task: Task
  revieweeId: number
  revieweeUsername?: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  submit: [data: CreateReviewRequest]
  cancel: []
}>()

const starRating = ref(5)
const reviewText = ref('')
const error = ref('')

const currentUserId = authService.getUserId()

// Star rating options
const starOptions = [1, 2, 3, 4, 5]

function validate(): boolean {
  error.value = ''

  if (!starRating.value || starRating.value < 1 || starRating.value > 5) {
    error.value = 'Please select a star rating (1-5)'
    return false
  }

  if (reviewText.value.trim().length === 0) {
    error.value = 'Please write a review'
    return false
  }

  if (reviewText.value.length > 1000) {
    error.value = 'Review must be 1000 characters or less'
    return false
  }

  return true
}

function handleSubmit() {
  if (!validate()) {
    return
  }

  const data: CreateReviewRequest = {
    taskId: props.task.id,
    revieweeId: props.revieweeId,
    starRating: starRating.value,
    reviewText: reviewText.value.trim()
  }

  console.log('🔵 ReviewForm EMIT:', data)
  emit('submit', data)
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="space-y-4">
    <!-- Error Message -->
    <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
      {{ error }}
    </div>

    <!-- Reviewee Info -->
    <div class="bg-blue-50 border border-blue-200 p-4 rounded-lg">
      <p class="text-sm text-gray-700">
        Reviewing: <span class="font-semibold text-blue-900">{{ revieweeUsername }}</span>
      </p>
    </div>

    <!-- Star Rating -->
    <div>
      <label class="block text-sm font-medium text-gray-700 mb-3">
        Rating <span class="text-red-500">*</span>
      </label>
      <div class="flex gap-2">
        <button
          v-for="star in starOptions"
          :key="star"
          type="button"
          @click="starRating = star"
          class="group relative"
        >
          <svg
            class="w-10 h-10 transition-colors"
            :class="star <= starRating ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300 fill-gray-300 hover:text-yellow-300'"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
          </svg>
          <span class="absolute -bottom-6 left-1/2 transform -translate-x-1/2 text-xs font-semibold text-gray-700 opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap">
            {{ star }} {{ star === 1 ? 'star' : 'stars' }}
          </span>
        </button>
      </div>
      <p class="mt-6 text-sm text-gray-600">
        <span v-if="starRating" class="font-semibold">{{ starRating }}</span> / 5 stars selected
      </p>
    </div>

    <!-- Review Text -->
    <div>
      <label for="review-text" class="block text-sm font-medium text-gray-700 mb-1">
        Review <span class="text-red-500">*</span>
      </label>
      <textarea
        id="review-text"
        v-model="reviewText"
        rows="5"
        placeholder="Share your experience with this user. The system will analyze your feedback to calculate a fair rating..."
        class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        required
      ></textarea>
      <p class="text-xs text-gray-500 mt-1">
        {{ reviewText.length }} / 1000 characters
      </p>
      <p class="text-xs text-gray-500 mt-2">
        💡 Tip: Use keywords like "puikiai" (great), "vėlavo" (late), "greitai" (quickly) for sentiment analysis
      </p>
    </div>

    <!-- How Rating Works -->
    <div class="bg-gray-50 border border-gray-200 p-4 rounded-lg">
      <h4 class="text-sm font-semibold text-gray-900 mb-2">How ratings are calculated:</h4>
      <ul class="text-xs text-gray-700 space-y-1">
        <li>✓ Your star rating (1-5)</li>
        <li>✓ Keywords in review (positive/negative words)</li>
        <li>✓ Completion speed (early/late completion)</li>
        <li>✓ Average from all previous reviews</li>
      </ul>
    </div>

    <!-- Buttons -->
    <div class="flex gap-3 pt-4">
      <button
        type="submit"
        class="flex-1 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition duration-200 font-medium"
      >
        Submit Review
      </button>
      <button
        type="button"
        @click="emit('cancel')"
        class="flex-1 bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition duration-200 font-medium"
      >
        Cancel
      </button>
    </div>
  </form>
</template>
