<template>
  <div class="min-h-screen bg-gray-100">
    <nav v-if="authStore.isAuthenticated" class="bg-indigo-600 shadow-lg">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <div class="flex items-center">
            <div class="flex-shrink-0">
              <h1 class="text-white text-xl font-bold">Leave Manager</h1>
            </div>
            <div class="hidden md:block">
              <div class="ml-10 flex items-baseline space-x-4">
                <router-link
                  to="/dashboard"
                  class="text-white hover:bg-indigo-500 px-3 py-2 rounded-md text-sm font-medium"
                  active-class="bg-indigo-700"
                >
                  Dashboard
                </router-link>
                <router-link
                  to="/leave-requests"
                  class="text-white hover:bg-indigo-500 px-3 py-2 rounded-md text-sm font-medium"
                  active-class="bg-indigo-700"
                >
                  My Leaves
                </router-link>
                <router-link
                  v-if="authStore.isManagerOrAdmin"
                  to="/pending-approvals"
                  class="text-white hover:bg-indigo-500 px-3 py-2 rounded-md text-sm font-medium"
                  active-class="bg-indigo-700"
                >
                  Pending Approvals
                </router-link>
                <router-link
                  v-if="authStore.isAdmin"
                  to="/employees"
                  class="text-white hover:bg-indigo-500 px-3 py-2 rounded-md text-sm font-medium"
                  active-class="bg-indigo-700"
                >
                  Employees
                </router-link>
                <router-link
                  v-if="authStore.isAdmin"
                  to="/leave-types"
                  class="text-white hover:bg-indigo-500 px-3 py-2 rounded-md text-sm font-medium"
                  active-class="bg-indigo-700"
                >
                  Leave Types
                </router-link>
              </div>
            </div>
          </div>
          <div class="flex items-center gap-4">
            <span class="text-white text-sm">
              {{ authStore.user?.fullName }} ({{ authStore.user?.role }})
            </span>
            <button
              @click="handleLogout"
              class="bg-indigo-700 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-indigo-800"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </nav>

    <main>
      <router-view />
    </main>

    <!-- Toast Notification -->
    <div
      v-if="toast.show"
      class="fixed bottom-4 right-4 px-6 py-4 rounded-lg shadow-lg text-white z-50"
      :class="toast.type === 'success' ? 'bg-green-500' : 'bg-red-500'"
    >
      {{ toast.message }}
    </div>
  </div>
</template>

<script setup>
import { ref, provide } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from './stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

const showToast = (message, type = 'success') => {
  toast.value = { show: true, message, type }
  setTimeout(() => {
    toast.value.show = false
  }, 3000)
}

provide('showToast', showToast)

const handleLogout = async () => {
  await authStore.logout()
  router.push('/login')
}
</script>
