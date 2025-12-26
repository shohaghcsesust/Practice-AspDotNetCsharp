<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Navigation -->
    <nav v-if="authStore.isAuthenticated" class="bg-indigo-600 shadow-lg">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-16">
          <div class="flex items-center">
            <router-link to="/" class="text-white text-xl font-bold">
              Leave Management
            </router-link>
            <div class="hidden md:flex ml-10 space-x-4">
              <router-link
                to="/dashboard"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Dashboard
              </router-link>
              <router-link
                to="/leave-requests"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Leave Requests
              </router-link>
              <router-link
                to="/leave-balance"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Leave Balance
              </router-link>
              <router-link
                to="/calendar"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Calendar
              </router-link>
              <router-link
                v-if="authStore.isAdmin"
                to="/holidays"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Holidays
              </router-link>
              <router-link
                v-if="authStore.isManagerOrAdmin"
                to="/reports"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Reports
              </router-link>
              <router-link
                v-if="authStore.isAdmin"
                to="/employees"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Employees
              </router-link>
              <router-link
                v-if="authStore.isAdmin"
                to="/leave-types"
                class="text-indigo-100 hover:text-white px-3 py-2 rounded-md text-sm font-medium"
                active-class="bg-indigo-700 text-white"
              >
                Leave Types
              </router-link>
            </div>
          </div>
          <div class="flex items-center space-x-4">
            <NotificationBell />
            <div class="text-indigo-100 text-sm">
              <span class="font-medium">{{ authStore.fullName }}</span>
              <span class="ml-2 bg-indigo-800 px-2 py-1 rounded text-xs">{{ authStore.userRole }}</span>
            </div>
            <button
              @click="logout"
              class="bg-indigo-700 hover:bg-indigo-800 text-white px-4 py-2 rounded-md text-sm font-medium transition"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </nav>

    <!-- Main Content -->
    <main>
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'
import NotificationBell from './components/NotificationBell.vue'

const router = useRouter()
const authStore = useAuthStore()

const logout = () => {
  authStore.logout()
  router.push('/login')
}
</script>