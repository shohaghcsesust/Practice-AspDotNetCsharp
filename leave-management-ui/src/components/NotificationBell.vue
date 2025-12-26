<template>
  <div class="relative">
    <!-- Notification Bell Button -->
    <button
      @click="toggleDropdown"
      class="relative p-2 text-gray-600 hover:text-gray-900 focus:outline-none"
    >
      <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
      </svg>
      
      <!-- Unread Badge -->
      <span
        v-if="unreadCount > 0"
        class="absolute top-0 right-0 inline-flex items-center justify-center px-2 py-1 text-xs font-bold leading-none text-white transform translate-x-1/2 -translate-y-1/2 bg-red-600 rounded-full"
      >
        {{ unreadCount > 99 ? '99+' : unreadCount }}
      </span>
    </button>

    <!-- Dropdown -->
    <transition
      enter-active-class="transition ease-out duration-100"
      enter-from-class="transform opacity-0 scale-95"
      enter-to-class="transform opacity-100 scale-100"
      leave-active-class="transition ease-in duration-75"
      leave-from-class="transform opacity-100 scale-100"
      leave-to-class="transform opacity-0 scale-95"
    >
      <div
        v-if="isOpen"
        class="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 z-50"
      >
        <div class="p-4 border-b border-gray-200">
          <div class="flex justify-between items-center">
            <h3 class="text-lg font-medium text-gray-900">Notifications</h3>
            <button
              v-if="unreadCount > 0"
              @click="markAllAsRead"
              class="text-sm text-blue-600 hover:text-blue-800"
            >
              Mark all read
            </button>
          </div>
        </div>

        <div class="max-h-96 overflow-y-auto">
          <div v-if="notifications.length === 0" class="p-4 text-center text-gray-500">
            No notifications
          </div>

          <div
            v-for="notification in notifications"
            :key="notification.id"
            class="p-4 border-b border-gray-100 hover:bg-gray-50 cursor-pointer"
            :class="{ 'bg-blue-50': !notification.isRead }"
            @click="handleNotificationClick(notification)"
          >
            <div class="flex items-start">
              <!-- Icon based on type -->
              <div
                class="flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center"
                :class="getTypeClasses(notification.type)"
              >
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path v-if="notification.type === 'success'" fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                  <path v-else-if="notification.type === 'error'" fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
                  <path v-else-if="notification.type === 'warning'" fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
                  <path v-else fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                </svg>
              </div>

              <div class="ml-3 flex-1">
                <p class="text-sm font-medium text-gray-900">{{ notification.title }}</p>
                <p class="text-sm text-gray-500">{{ notification.message }}</p>
                <p class="text-xs text-gray-400 mt-1">{{ formatTime(notification.createdAt) }}</p>
              </div>

              <div v-if="!notification.isRead" class="flex-shrink-0 ml-2">
                <span class="inline-block w-2 h-2 bg-blue-600 rounded-full"></span>
              </div>
            </div>
          </div>
        </div>

        <div class="p-3 border-t border-gray-200 text-center">
          <router-link
            to="/notifications"
            class="text-sm text-blue-600 hover:text-blue-800"
            @click="isOpen = false"
          >
            View all notifications
          </router-link>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '../services/api'
import * as signalR from '@microsoft/signalr'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isOpen = ref(false)
const notifications = ref([])
const unreadCount = ref(0)
let connection = null

const toggleDropdown = () => {
  isOpen.value = !isOpen.value
  if (isOpen.value) {
    fetchNotifications()
  }
}

const fetchNotifications = async () => {
  try {
    const response = await api.get('/api/notifications')
    notifications.value = response.data.slice(0, 10)
    updateUnreadCount()
  } catch (error) {
    console.error('Failed to fetch notifications:', error)
  }
}

const updateUnreadCount = async () => {
  try {
    const response = await api.get('/api/notifications/unread-count')
    unreadCount.value = response.data.unreadCount
  } catch (error) {
    console.error('Failed to get unread count:', error)
  }
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
  isOpen.value = false
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
  return date.toLocaleDateString()
}

const setupSignalR = async () => {
  if (!authStore.token) return

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_URL || 'http://localhost:5000'}/hubs/notifications`, {
      accessTokenFactory: () => authStore.token
    })
    .withAutomaticReconnect()
    .build()

  connection.on('ReceiveNotification', (notification) => {
    notifications.value.unshift(notification)
    unreadCount.value++
    
    // Show browser notification if permitted
    if (Notification.permission === 'granted') {
      new Notification(notification.title, {
        body: notification.message,
        icon: '/favicon.ico'
      })
    }
  })

  connection.on('NotificationRead', (notificationId) => {
    const notification = notifications.value.find(n => n.id === notificationId)
    if (notification) {
      notification.isRead = true
    }
  })

  connection.on('AllNotificationsRead', () => {
    notifications.value.forEach(n => n.isRead = true)
    unreadCount.value = 0
  })

  try {
    await connection.start()
    console.log('SignalR connected')
  } catch (error) {
    console.error('SignalR connection failed:', error)
  }
}

onMounted(async () => {
  await updateUnreadCount()
  await setupSignalR()
  
  // Request browser notification permission
  if ('Notification' in window && Notification.permission === 'default') {
    Notification.requestPermission()
  }
})

onUnmounted(() => {
  if (connection) {
    connection.stop()
  }
})
</script>
