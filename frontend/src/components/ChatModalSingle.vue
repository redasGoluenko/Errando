<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div class="bg-white rounded-lg shadow-lg w-full max-w-2xl h-[600px] flex flex-col">
      <!-- Header -->
      <div class="bg-gradient-to-r from-indigo-600 to-indigo-700 text-white p-4 rounded-t-lg flex items-center justify-between">
        <div>
          <h2 class="text-lg font-semibold">{{ getOtherUserName() }}</h2>
          <p class="text-xs text-indigo-100">{{ getOtherUserRole() }}</p>
        </div>
        <button
          @click="$emit('close')"
          class="text-white hover:bg-indigo-600 p-2 rounded"
        >
          ✕
        </button>
      </div>

      <!-- Messages -->
      <div
        ref="messagesContainer"
        class="flex-1 overflow-y-auto p-6 space-y-3 bg-gray-50"
      >
        <div v-if="chat.messages.length === 0" class="flex items-center justify-center h-full">
          <p class="text-gray-500">No messages yet. Start the conversation!</p>
        </div>

        <div v-else class="flex flex-col gap-3">
          <div
            v-for="message in chat.messages"
            :key="message.id"
            class="flex"
            :class="{ 'justify-end': isOwnMessage(message), 'justify-start': !isOwnMessage(message) }"
          >
            <div
              class="max-w-xs px-4 py-3 rounded-2xl shadow-sm"
              :class="{
                'bg-blue-500 text-white': isOwnMessage(message),
                'bg-gray-300 text-gray-900': !isOwnMessage(message)
              }"
            >
              <p class="text-sm break-words">{{ message.content }}</p>
              <p class="text-xs mt-2 opacity-75">
                {{ formatMessageTime(message.sentAt) }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Input -->
      <div class="bg-white border-t p-4 rounded-b-lg">
        <div class="flex gap-2">
          <input
            v-model="messageContent"
            @keydown.enter="sendMessage"
            type="text"
            placeholder="Type a message..."
            class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
          />
          <button
            @click="sendMessage"
            :disabled="!messageContent.trim() || isSending"
            class="bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white font-semibold py-2 px-6 rounded-lg transition"
          >
            <span v-if="!isSending">Send</span>
            <span v-else>...</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue';
import ChatService, { type Chat, type ChatMessage } from '../services/ChatService';
import { useAuthStore } from '../stores/authStore';

const props = defineProps<{
  chat: Chat;
}>();

const emit = defineEmits<{
  'close': [];
}>();

const authStore = useAuthStore();
const messageContent = ref('');
const messagesContainer = ref<HTMLElement | null>(null);
const isSending = ref(false);

const isOwnMessage = (message: ChatMessage): boolean => {
  const currentId = authStore.user?.id;
  return currentId !== null && currentId !== undefined && message.senderId === Number(currentId);
};

const getOtherUserName = (): string => {
  if (!authStore.user?.id || !props.chat) return 'Unknown';
  
  const currentId = Number(authStore.user.id);
  const user1Id = Number(props.chat.user1Id);
  const user2Id = Number(props.chat.user2Id);
  
  if (user1Id === currentId && props.chat.user2?.username) {
    return props.chat.user2.username;
  } else if (user2Id === currentId && props.chat.user1?.username) {
    return props.chat.user1.username;
  }
  
  return 'Unknown';
};

const getOtherUserRole = (): string => {
  if (!authStore.user?.id || !props.chat) return 'Unknown';
  
  const currentId = Number(authStore.user.id);
  const user1Id = Number(props.chat.user1Id);
  const user2Id = Number(props.chat.user2Id);
  
  if (user1Id === currentId && props.chat.user2?.role) {
    return props.chat.user2.role;
  } else if (user2Id === currentId && props.chat.user1?.role) {
    return props.chat.user1.role;
  }
  
  return 'Unknown';
};

const formatMessageTime = (isoTime: string): string => {
  const date = new Date(isoTime);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

const sendMessage = async () => {
  const content = messageContent.value.trim();
  if (!content || isSending.value) return;

  try {
    isSending.value = true;
    const message = await ChatService.sendMessage(props.chat.id, content);
    props.chat.messages.push(message);
    messageContent.value = '';
    
    await nextTick();
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
    }
  } catch (error) {
    console.error('Failed to send message:', error);
    alert('Failed to send message. Please try again.');
  } finally {
    isSending.value = false;
  }
};

watch(
  () => props.chat.messages.length,
  async () => {
    await nextTick();
    if (messagesContainer.value) {
      messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
    }
  }
);
</script>
