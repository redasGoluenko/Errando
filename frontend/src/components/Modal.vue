<script setup lang="ts">
interface Props {
  show: boolean
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl'
}

const props = withDefaults(defineProps<Props>(), {
  maxWidth: 'md',
})

const emit = defineEmits<{
  close: []
}>()
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
      @click.self="emit('close')"
    >
      <!-- Backdrop -->
      <div class="fixed inset-0 bg-black bg-opacity-50" @click="emit('close')"></div>

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
            :class="[
              'relative bg-white rounded-lg shadow-xl w-full',
              {
                'max-w-sm': maxWidth === 'sm',
                'max-w-md': maxWidth === 'md',
                'max-w-lg': maxWidth === 'lg',
                'max-w-xl': maxWidth === 'xl',
              },
            ]"
            @click.stop
          >
            <!-- Header -->
            <div class="px-6 py-4 border-b border-gray-200">
              <div class="flex items-center justify-between">
                <slot name="header">
                  <!-- Default header if no slot provided -->
                  <h3 class="text-xl font-semibold text-gray-900">Modal</h3>
                </slot>
                <button
                  @click="emit('close')"
                  class="text-gray-400 hover:text-gray-600 transition"
                >
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Body -->
            <div class="px-6 py-4">
              <slot name="body"></slot>
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </Transition>
</template>