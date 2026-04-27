<template>
  <div class="bg-white rounded-lg shadow h-full flex flex-col">
    <!-- Chat Header -->
    <div class="bg-gradient-to-r from-indigo-600 to-indigo-700 text-white p-4 rounded-t-lg">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-xl font-semibold">{{ getOtherUserName() }}</h2>
          <p class="text-sm text-indigo-100">{{ getOtherUserRole() }}</p>
        </div>
        <div class="flex items-center gap-3">
          <div class="text-right text-sm text-indigo-100">
            <p>Task ID: {{ chat.taskId ? `#${chat.taskId}` : 'N/A' }}</p>
          </div>
          <button
            @click="deleteChat"
            class="bg-red-500 hover:bg-red-600 text-white px-3 py-1 rounded text-sm transition"
          >
            Delete
          </button>
        </div>
      </div>
    </div>

    <!-- Messages Container -->
    <div 
      ref="messagesContainer"
      class="flex-1 overflow-y-auto p-6 bg-gray-50"
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
            class="max-w-xs lg:max-w-md px-4 py-3 rounded-2xl shadow-sm"
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

    <!-- Message Input -->
    <div class="bg-white border-t p-4 rounded-b-lg">
      <div class="flex gap-2">
        <textarea
          v-model="messageContent"
          @keydown.enter.ctrl="sendMessage"
          @keydown.enter.exact="handleEnter"
          placeholder="Type your message... (Ctrl+Enter to send)"
          class="flex-1 px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none max-h-24"
          rows="2"
        ></textarea>
        <button
          @click="sendMessage"
          :disabled="!messageContent.trim() || loading"
          class="bg-indigo-600 hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed text-white font-semibold py-2 px-6 rounded-lg transition"
        >
          <span v-if="!loading">Send</span>
          <span v-else>...</span>
        </button>
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
  loading: boolean;
}>();

const emit = defineEmits<{
  'message-sent': [message: ChatMessage];
}>();

const authStore = useAuthStore();
const messageContent = ref('');
const messagesContainer = ref<HTMLElement | null>(null);
const isSending = ref(false);

/**
 * Check if a message is from the current user
 */
const isOwnMessage = (message: ChatMessage): boolean => {
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  return currentId !== null && message.senderId === currentId;
};

/**
 * Get the other user's name
 */
const getOtherUserName = (): string => {
  if (!props.chat) return 'Unknown';
  
  // Get current user ID from localStorage (most reliable)
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  
  if (currentId === null) return 'Unknown';
  
  const user1Id = Number(props.chat.user1Id);
  const user2Id = Number(props.chat.user2Id);
  
  if (user1Id === currentId && props.chat.user2?.username) {
    return props.chat.user2.username;
  } else if (user2Id === currentId && props.chat.user1?.username) {
    return props.chat.user1.username;
  }
  
  return 'Unknown';
};

/**
 * Get the other user's role
 */
const getOtherUserRole = (): string => {
  if (!props.chat) return 'Unknown';
  
  // Get current user ID from localStorage (most reliable)
  const currentIdStr = localStorage.getItem('userId');
  const currentId = currentIdStr ? Number(currentIdStr) : null;
  
  if (currentId === null) return 'Unknown';
  
  const user1Id = Number(props.chat.user1Id);
  const user2Id = Number(props.chat.user2Id);
  
  if (user1Id === currentId && props.chat.user2?.role) {
    return props.chat.user2.role;
  } else if (user2Id === currentId && props.chat.user1?.role) {
    return props.chat.user1.role;
  }
  
  return 'Unknown';
};

/**
 * Format message timestamp for display
 */
const formatMessageTime = (isoTime: string): string => {
  const date = new Date(isoTime);
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

/**
 * Handle Enter key press (send on Ctrl+Enter, newline on Enter)
 */
const handleEnter = (e: KeyboardEvent) => {
  if (!e.ctrlKey && !e.metaKey) {
    // Regular Enter - allow newline
    return;
  }
  e.preventDefault();
  sendMessage();
};

/**
 * Send the message
 */
const sendMessage = async () => {
  const content = messageContent.value.trim();
  if (!content || isSending.value) return;

  try {
    isSending.value = true;
    const message = await ChatService.sendMessage(props.chat.id, content);
    
    messageContent.value = '';
    emit('message-sent', message);
    
    // Scroll to bottom
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

/**
 * Delete the chat
 */
const deleteChat = async () => {
  if (!confirm('Are you sure you want to delete this chat? This cannot be undone.')) return;

  try {
    const response = await fetch(`http://localhost:5064/api/chats/${props.chat.id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      }
    });

    if (response.ok) {
      // Automatically reload the page after successful deletion
      window.location.reload();
    } else {
      alert('Failed to delete chat');
    }
  } catch (error) {
    console.error('Failed to delete chat:', error);
    alert('Failed to delete chat');
  }
};

/**
 * Scroll to bottom when messages change
 */
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
