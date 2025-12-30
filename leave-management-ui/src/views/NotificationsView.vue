<template>
  <div class="max-w-4xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Notifications</h1>
        <button
          v-if="unreadCount > 0"
          @click="markAllAsRead"
          class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition text-sm"
        >
          Mark all as read
        </button>
      </div>

      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="loading" class="text-center py-8">
          <span class="text-gray-500">Loading notifications...</span>
        </div>

        <div v-else-if="notifications.length === 0" class="text-center py-12">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900">No notifications</h3>
          <p class="mt-1 text-sm text-gray-500">You're all caught up!</p>
        </div>

        <ul v-else class="divide-y divide-gray-200">
          <li
            v-for="notification in notifications"
            :key="notification.id"
            class="p-4 hover:bg-gray-50 cursor-pointer transition"
            :class="{ 'bg-blue-50': !notification.isRead }"
            @click="handleNotificationClick(notification)"
          >
            <div class="flex items-start">
              <!-- Icon based on type -->
              <div
                class="flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center"
                :class="getTypeClasses(notification.type)"
              >
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path v-if="notification.type === 'success'" fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                  <path v-else-if="notification.type === 'error'" fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                  <path v-else-if="notification.type === 'warning'" fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
                  <path v-else fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                </svg>
              </div>

              <div class="ml-4 flex-1">
                <div class="flex items-center justify-between">
                  <p class="text-sm font-medium text-gray-900">{{ notification.title }}</p>
                  <div class="flex items-center space-x-2">
                    <span class="text-xs text-gray-400">{{ formatTime(notification.createdAt) }}</span>
                    <span v-if="!notification.isRead" class="inline-block w-2 h-2 bg-blue-600 rounded-full"></span>
                  </div>
                </div>
                <p class="mt-1 text-sm text-gray-600">{{ notification.message }}</p>
              </div>
            </div>
          </li>
        </ul>

        <!-- Pagination -->
        <div v-if="hasMore" class="p-4 border-t border-gray-200 text-center">
          <button
            @click="loadMore"
            :disabled="loadingMore"
            class="text-indigo-600 hover:text-indigo-800 text-sm font-medium"
          >
            {{ loadingMore ? 'Loading...' : 'Load more' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '../services/api'

const router = useRouter()

const notifications = ref([])
const loading = ref(true)
const loadingMore = ref(false)
const unreadCount = ref(0)
const hasMore = ref(false)
const page = ref(1)
const pageSize = 20

const fetchNotifications = async (append = false) => {
  try {
    if (append) {
      loadingMore.value = true
    } else {
      loading.value = true
    }
    
    const response = await api.get(`/api/notifications?page=${page.value}&pageSize=${pageSize}`)
    const data = Array.isArray(response.data) 
      ? response.data 
      : (response.data.data || response.data.$values || [])
    
    if (append) {
      notifications.value.push(...data)
    } else {
      notifications.value = data
    }
    
    hasMore.value = data.length === pageSize
    updateUnreadCount()
  } catch (error) {
    console.error('Failed to fetch notifications:', error)
  } finally {
    loading.value = false
    loadingMore.value = false
  }
}

const updateUnreadCount = async () => {
  try {
    const response = await api.get('/api/notifications/unread-count')
    unreadCount.value = response.data.unreadCount || response.data.count || 0
  } catch (error) {
    unreadCount.value = notifications.value.filter(n => !n.isRead).length
  }
}

const loadMore = () => {
  page.value++
  fetchNotifications(true)
}

const markAsRead = async (id) => {
  try {
    await api.patch(`/api/notifications/${id}/read`)
    const notification = notifications.value.find(n => n.id === id)
    if (notification) {
      notification.isRead = true
    }
    updateUnreadCount()
  } catch (error) {
    console.error('Failed to mark as read:', error)
  }
}

const markAllAsRead = async () => {
  try {
    await api.patch('/api/notifications/read-all')
    notifications.value.forEach(n => n.isRead = true)
    unreadCount.value = 0
  } catch (error) {
    console.error('Failed to mark all as read:', error)
  }
}

const handleNotificationClick = async (notification) => {
  if (!notification.isRead) {
    await markAsRead(notification.id)
  }
  if (notification.link) {
    router.push(notification.link)
  }
}

const getTypeClasses = (type) => {
  switch (type) {
    case 'success':
      return 'bg-green-100 text-green-600'
    case 'error':
      return 'bg-red-100 text-red-600'
    case 'warning':
      return 'bg-yellow-100 text-yellow-600'
    default:
      return 'bg-blue-100 text-blue-600'
  }
}

const formatTime = (dateString) => {
  const date = new Date(dateString)
  const now = new Date()
  const diff = now - date

  if (diff < 60000) return 'Just now'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}m ago`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}h ago`
  if (diff < 604800000) return `${Math.floor(diff / 86400000)}d ago`
  return date.toLocaleDateString()
}

onMounted(() => {
  fetchNotifications()
})
</script>
