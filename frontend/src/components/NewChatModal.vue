<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg shadow-lg max-w-md w-full mx-4">
      <!-- Header -->
      <div class="bg-indigo-600 text-white p-6 rounded-t-lg">
        <h2 class="text-xl font-semibold">Start a New Chat</h2>
      </div>

      <!-- Content -->
      <div class="p-6">
        <p class="text-gray-600 mb-4">Select a user to chat with</p>

        <!-- Loading State -->
        <div v-if="loading" class="flex items-center justify-center py-6">
          <p class="text-gray-600">Loading participants...</p>
        </div>

        <!-- Error State -->
        <div v-else-if="error" class="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
          <p>{{ error }}</p>
        </div>

        <!-- Participant List -->
        <div v-else-if="participants.length === 0" class="text-center py-6">
          <p class="text-gray-500">No available participants</p>
          <p class="text-sm text-gray-400 mt-2">
            You need to have task assignments to chat with people
          </p>
        </div>

        <div v-else class="space-y-2 max-h-96 overflow-y-auto">
          <button
            v-for="user in participants"
            :key="user.id"
            @click="selectUser(user)"
            class="w-full text-left p-3 border border-gray-200 rounded-lg hover:bg-indigo-50 hover:border-indigo-300 transition group"
          >
            <div class="flex items-center justify-between">
              <div>
                <p class="font-semibold text-gray-900 group-hover:text-indigo-600">
                  {{ user.username }}
                </p>
                <p class="text-sm text-gray-600">{{ user.role }}</p>
                <p class="text-xs text-gray-400">{{ user.email }}</p>
              </div>
              <div v-if="selectedUser?.id === user.id" class="text-indigo-600">
                ✓
              </div>
            </div>
          </button>
        </div>
      </div>

      <!-- Footer -->
      <div class="bg-gray-50 p-6 rounded-b-lg flex gap-3 justify-end border-t">
        <button
          @click="closeModal"
          class="px-4 py-2 text-gray-700 border border-gray-300 rounded-lg hover:bg-gray-100 transition"
        >
          Cancel
        </button>
        <button
          @click="createChat"
          :disabled="!selectedUser || isCreating"
          class="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white rounded-lg transition"
        >
          <span v-if="!isCreating">Start Chat</span>
          <span v-else>Creating...</span>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import ChatService, { type Chat, type User } from '../services/ChatService';

const emit = defineEmits<{
  'close': [];
  'chat-created': [chat: Chat];
}>();

const participants = ref<User[]>([]);
const selectedUser = ref<User | null>(null);
const loading = ref(false);
const isCreating = ref(false);
const error = ref<string | null>(null);

/**
 * Load available chat participants
 */
const loadParticipants = async () => {
  try {
    loading.value = true;
    error.value = null;
    
    // Get all potential participants
    const allParticipants = await ChatService.getChatParticipants();
    
    // Get existing chats to filter out users already chatting with
    const existingChats = await ChatService.getChats();
    const existingChatUserIds = new Set<number>();
    
    existingChats.forEach(chat => {
      if (chat.user1Id !== null) existingChatUserIds.add(chat.user1Id);
      if (chat.user2Id !== null) existingChatUserIds.add(chat.user2Id);
    });
    
    // Filter out users we already have chats with
    participants.value = allParticipants.filter(
      user => !existingChatUserIds.has(user.id)
    );
  } catch (err) {
    error.value = 'Failed to load participants. Please try again.';
    console.error('Failed to load participants:', err);
  } finally {
    loading.value = false;
  }
};

/**
 * Select a user
 */
const selectUser = (user: User) => {
  selectedUser.value = selectedUser.value?.id === user.id ? null : user;
};

/**
 * Create a new chat with the selected user
 */
const createChat = async () => {
  if (!selectedUser.value) return;

  try {
    isCreating.value = true;
    const chat = await ChatService.createOrGetChat(selectedUser.value.id);
    emit('chat-created', chat);
  } catch (err) {
    error.value = 'Failed to create chat. Please try again.';
    console.error('Failed to create chat:', err);
  } finally {
    isCreating.value = false;
  }
};

/**
 * Close the modal
 */
const closeModal = () => {
  emit('close');
};

onMounted(() => {
  loadParticipants();
});
</script>
