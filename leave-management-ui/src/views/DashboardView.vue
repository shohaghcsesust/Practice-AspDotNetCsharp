<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">Dashboard</h1>

      <!-- Welcome Section -->
      <div class="bg-white overflow-hidden shadow rounded-lg mb-8">
        <div class="px-4 py-5 sm:p-6">
          <h2 class="text-xl font-semibold text-gray-800">
            Welcome back, {{ authStore.user?.fullName }}!
          </h2>
          <p class="mt-1 text-gray-600">
            {{ authStore.user?.position }} - {{ authStore.user?.department }}
          </p>
        </div>
      </div>

      <!-- Leave Balances -->
      <div class="mb-8">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">My Leave Balances</h3>
        <div v-if="loading" class="text-center py-8">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
        </div>
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div
            v-for="balance in leaveBalances"
            :key="balance.id"
            class="bg-white overflow-hidden shadow rounded-lg"
          >
            <div class="px-4 py-5 sm:p-6">
              <dt class="text-sm font-medium text-gray-500 truncate">
                {{ balance.leaveTypeName }}
              </dt>
              <dd class="mt-1 text-3xl font-semibold text-indigo-600">
                {{ balance.remainingDays }}
                <span class="text-sm text-gray-500 font-normal">/ {{ balance.totalDays }} days</span>
              </dd>
              <div class="mt-2">
                <div class="w-full bg-gray-200 rounded-full h-2">
                  <div
                    class="bg-indigo-600 h-2 rounded-full"
                    :style="{ width: `${(balance.remainingDays / balance.totalDays) * 100}%` }"
                  ></div>
                </div>
              </div>
              <p class="mt-2 text-sm text-gray-500">
                Used: {{ balance.usedDays }} days
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="mb-8">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h3>
        <div class="flex gap-4">
          <router-link
            to="/leave-requests/new"
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
          >
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
            </svg>
            Request Leave
          </router-link>
          <router-link
            to="/leave-requests"
            class="inline-flex items-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50"
          >
            View My Requests
          </router-link>
        </div>
      </div>

      <!-- Recent Leave Requests -->
      <div>
        <h3 class="text-lg font-semibold text-gray-900 mb-4">Recent Leave Requests</h3>
        <div class="bg-white shadow overflow-hidden sm:rounded-md">
          <ul v-if="recentRequests.length" class="divide-y divide-gray-200">
            <li v-for="request in recentRequests" :key="request.id">
              <div class="px-4 py-4 sm:px-6">
                <div class="flex items-center justify-between">
                  <div class="flex items-center">
                    <p class="text-sm font-medium text-indigo-600 truncate">
                      {{ request.leaveTypeName }}
                    </p>
                    <span
                      class="ml-2 px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                      :class="getStatusClass(request.status)"
                    >
                      {{ request.status }}
                    </span>
                  </div>
                  <div class="text-sm text-gray-500">
                    {{ formatDate(request.startDate) }} - {{ formatDate(request.endDate) }}
                  </div>
                </div>
                <div class="mt-2 text-sm text-gray-500">
                  {{ request.totalDays }} day(s) • {{ request.reason || 'No reason provided' }}
                </div>
              </div>
            </li>
          </ul>
          <div v-else class="px-4 py-8 text-center text-gray-500">
            No recent leave requests
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useLeaveStore } from '../stores/leave'

const authStore = useAuthStore()
const leaveStore = useLeaveStore()

const loading = ref(true)
const leaveBalances = ref([])
const leaveRequests = ref([])

const recentRequests = computed(() => {
  return leaveRequests.value.slice(0, 5)
})

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

const getStatusClass = (status) => {
  switch (status) {
    case 'Approved':
      return 'bg-green-100 text-green-800'
    case 'Rejected':
      return 'bg-red-100 text-red-800'
    case 'Cancelled':
      return 'bg-gray-100 text-gray-800'
    default:
      return 'bg-yellow-100 text-yellow-800'
  }
}

onMounted(async () => {
  try {
    await Promise.all([
      leaveStore.fetchLeaveBalances(),
      leaveStore.fetchMyLeaveRequests(authStore.user.id)
    ])
    leaveBalances.value = leaveStore.leaveBalances
    leaveRequests.value = leaveStore.leaveRequests
  } catch (error) {
    console.error('Error loading dashboard:', error)
  } finally {
    loading.value = false
  }
})
</script>
