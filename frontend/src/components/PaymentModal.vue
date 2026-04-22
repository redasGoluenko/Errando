<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { loadStripe, type Stripe, type StripeElements, type StripePaymentElement } from '@stripe/stripe-js'
import { paymentService } from '@/services/paymentService'
import type { Task } from '@/services/tasksService'

interface Props {
  task: Task
  show: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: []
  success: []
}>()

const stripePromise = loadStripe('pk_test_51TOxZUC6hYwlVWR5KVTTj3dHl6RZZaekACQq1P0MBqNBlnppTn313eo20EGNP3fik3uebpBvM9r6aYB6zqIQXCUl00mjOWygzY')

let stripe: Stripe | null = null
let elements: StripeElements | null = null
let paymentElement: StripePaymentElement | null = null

const isLoading = ref(false)
const error = ref('')
const successMessage = ref('')
const clientSecret = ref('')
const paymentId = ref<number | null>(null)
const paymentIntentId = ref<string>('')
const paymentInitialized = ref(false)

const displayAmount = computed(() => {
  return (props.task.price || 0).toFixed(2)
})

onMounted(async () => {
  stripe = await stripePromise
})

async function initializePayment() {
  try {
    error.value = ''
    isLoading.value = true
    paymentInitialized.value = false

    // Completely clean up old elements first
    if (paymentElement) {
      try {
        paymentElement.unmount()
      } catch (e) {
        console.log('Element unmount error (expected):', e)
      }
      paymentElement = null
    }
    if (elements) {
      elements = null
    }

    // Check if already paid
    const hasPaid = await paymentService.hasPaid(props.task.id)
    if (hasPaid) {
      error.value = 'This task has already been paid'
      isLoading.value = false
      return
    }

    // Create payment intent
    const response = await paymentService.createPaymentIntent({
      taskId: props.task.id,
      amount: props.task.price || 0,
    })

    clientSecret.value = response.clientSecret
    paymentId.value = response.paymentId
    paymentIntentId.value = response.paymentIntentId

    // Initialize Stripe Elements
    if (!stripe) {
      stripe = await stripePromise
    }

    if (stripe) {
      elements = stripe.elements({
        clientSecret: response.clientSecret,
      })

      paymentElement = elements.create('payment')
      
      // Wait for DOM to be ready
      await new Promise(resolve => setTimeout(resolve, 100))
      
      const paymentElementDiv = document.getElementById('payment-element')
      if (paymentElement && paymentElementDiv) {
        paymentElement.mount(paymentElementDiv)
        paymentInitialized.value = true
      }
    }

    isLoading.value = false
  } catch (err: any) {
    error.value = err.response?.data?.error || err.message || 'Failed to initialize payment'
    console.error('Payment initialization error:', err)
    isLoading.value = false
  }
}

async function handleSubmit() {
  if (!stripe || !elements || !clientSecret.value) {
    error.value = 'Payment system not initialized'
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    // Give the element a moment to fully settle before submitting
    await new Promise(resolve => setTimeout(resolve, 200))

    const { error: submitError } = await elements.submit()
    if (submitError) {
      error.value = submitError.message || 'Payment failed'
      isLoading.value = false
      return
    }

    const { paymentIntent, error: confirmError } = await stripe.confirmPayment({
      elements,
      clientSecret: clientSecret.value,
      redirect: 'if_required',
    })

    if (confirmError) {
      error.value = confirmError.message || 'Payment confirmation failed'
      isLoading.value = false
      return
    }

    if (paymentIntent.status === 'succeeded') {
      // Confirm payment on backend
      try {
        await paymentService.confirmPayment({
          paymentId: paymentId.value!,
          paymentIntentId: paymentIntent.id,
        })
      } catch (backendError) {
        console.error('Error confirming payment on backend:', backendError)
      }
      
      successMessage.value = 'Payment successful! Thank you.'
      emit('success')
      
      // Wait a moment before closing
      setTimeout(() => {
        emit('close')
      }, 2000)
    } else if (paymentIntent.status === 'processing') {
      successMessage.value = 'Payment is being processed...'
    } else {
      error.value = 'Payment status: ' + paymentIntent.status
    }

    isLoading.value = false
  } catch (err: any) {
    console.error('Payment submission error:', err)
    error.value = err.message || 'Payment processing failed'
    isLoading.value = false
  }
}

function handleClose() {
  // Reset form
  clientSecret.value = ''
  paymentId.value = null
  paymentIntentId.value = ''
  error.value = ''
  successMessage.value = ''
  paymentInitialized.value = false
  
  // Unmount payment element
  if (paymentElement) {
    paymentElement.unmount()
    paymentElement = null
  }
  if (elements) {
    elements = null
  }

  emit('close')
}
</script>

<template>
  <Transition
    enter-active-class="transition ease-out duration-300"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition ease-in duration-200"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="show"
      class="fixed inset-0 z-50 overflow-y-auto"
      @click.self="handleClose"
    >
      <!-- Backdrop -->
      <div class="fixed inset-0 bg-black bg-opacity-50" @click="handleClose"></div>

      <!-- Modal -->
      <div class="flex min-h-screen items-center justify-center p-4">
        <Transition
          enter-active-class="transition ease-out duration-300"
          enter-from-class="opacity-0 scale-95"
          enter-to-class="opacity-100 scale-100"
          leave-active-class="transition ease-in duration-200"
          leave-from-class="opacity-100 scale-100"
          leave-to-class="opacity-0 scale-95"
        >
          <div
            v-if="show"
            class="relative bg-white rounded-lg shadow-xl w-full max-w-md"
          >
            <!-- Close button -->
            <button
              type="button"
              @click="handleClose"
              class="absolute right-4 top-4 text-gray-400 hover:text-gray-600"
            >
              <span class="sr-only">Close</span>
              <svg class="h-6 w-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>

            <!-- Header -->
            <div class="border-b border-gray-200 px-6 py-4">
              <h2 class="text-lg font-semibold text-gray-900">Payment for Task</h2>
              <p class="mt-1 text-sm text-gray-600">{{ task.title }}</p>
            </div>

            <!-- Content -->
            <div class="px-6 py-4">
              <!-- Task Info -->
              <div class="mb-6 bg-blue-50 border border-blue-200 p-4 rounded-lg">
                <div class="flex justify-between items-center">
                  <span class="text-gray-700">Amount due:</span>
                  <span class="text-2xl font-bold text-blue-900">${{ displayAmount }}</span>
                </div>
              </div>

              <!-- Success Message -->
              <div v-if="successMessage" class="mb-4 bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded">
                {{ successMessage }}
              </div>

              <!-- Error Message -->
              <div v-if="error" class="mb-4 bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
                {{ error }}
              </div>

              <!-- Payment Element -->
              <form v-if="!successMessage" @submit.prevent="handleSubmit" class="space-y-4">
                <div v-if="!clientSecret" class="flex items-center justify-center py-8">
                  <button
                    type="button"
                    @click="initializePayment"
                    :disabled="isLoading"
                    class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
                  >
                    {{ isLoading ? 'Initializing...' : 'Start Payment' }}
                  </button>
                </div>

                <div v-else>
                  <div id="payment-element" class="mb-4"></div>

                  <!-- Submit Button -->
                  <button
                    type="submit"
                    :disabled="isLoading"
                    class="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed font-medium"
                  >
                    {{ isLoading ? 'Processing...' : `Pay $${displayAmount}` }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </Transition>
</template>
