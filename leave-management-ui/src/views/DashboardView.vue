<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">Dashboard</h1>
      
      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div class="rounded-md bg-blue-500 p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                </div>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Total Requests</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ leaveStore.leaveRequests.length }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div class="rounded-md bg-yellow-500 p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Pending</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ pendingCount }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div class="rounded-md bg-green-500 p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Approved</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ approvedCount }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
        
        <div class="bg-white overflow-hidden shadow rounded-lg">
          <div class="p-5">
            <div class="flex items-center">
              <div class="flex-shrink-0">
                <div class="rounded-md bg-red-500 p-3">
                  <svg class="h-6 w-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                </div>
              </div>
              <div class="ml-5 w-0 flex-1">
                <dl>
                  <dt class="text-sm font-medium text-gray-500 truncate">Rejected</dt>
                  <dd class="text-lg font-medium text-gray-900">{{ rejectedCount }}</dd>
                </dl>
              </div>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Quick Actions -->
      <div class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Quick Actions</h2>
        <div class="flex space-x-4">
          <router-link 
            to="/leave-requests/new" 
            class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition"
          >
            New Leave Request
          </router-link>
          <router-link 
            to="/leave-balance" 
            class="bg-gray-600 text-white px-4 py-2 rounded-md hover:bg-gray-700 transition"
          >
            View Balance
          </router-link>
          <router-link 
            to="/calendar" 
            class="bg-green-600 text-white px-4 py-2 rounded-md hover:bg-green-700 transition"
          >
            View Calendar
          </router-link>
        </div>
      </div>
      
      <!-- Leave Balances -->
      <div class="bg-white shadow rounded-lg p-6 mb-8">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Leave Balances</h2>
        <div v-if="leaveStore.loading" class="text-center py-4">Loading...</div>
        <div v-else class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div 
            v-for="balance in leaveStore.leaveBalances" 
            :key="balance.id"
            class="border rounded-lg p-4"
          >
            <h3 class="font-medium text-gray-900">{{ balance.leaveTypeName }}</h3>
            <div class="mt-2 flex justify-between text-sm">
              <span class="text-gray-500">Available:</span>
              <span class="font-medium text-green-600">{{ balance.remainingDays }} days</span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-gray-500">Used:</span>
              <span class="font-medium text-red-600">{{ balance.usedDays }} days</span>
            </div>
            <div class="flex justify-between text-sm">
              <span class="text-gray-500">Total:</span>
              <span class="font-medium">{{ balance.totalDays }} days</span>
            </div>
          </div>
        </div>
      </div>
      
      <!-- Recent Leave Requests -->
      <div class="bg-white shadow rounded-lg p-6">
        <h2 class="text-lg font-medium text-gray-900 mb-4">Recent Leave Requests</h2>
        <div v-if="leaveStore.loading" class="text-center py-4">Loading...</div>
        <div v-else-if="recentRequests.length === 0" class="text-center py-4 text-gray-500">
          No leave requests yet
        </div>
        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Start Date</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">End Date</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="request in recentRequests" :key="request.id">
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ request.leaveTypeName }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(request.startDate) }}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(request.endDate) }}</td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span 
                    :class="statusClass(request.status)"
                    class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  >
                    {{ request.status }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useLeaveStore } from '../stores/leave'

const leaveStore = useLeaveStore()

onMounted(() => {
  leaveStore.fetchMyLeaveRequests()
  leaveStore.fetchLeaveBalances()
})

const pendingCount = computed(() => 
  leaveStore.leaveRequests.filter(r => r.status === 'Pending').length
)

const approvedCount = computed(() => 
  leaveStore.leaveRequests.filter(r => r.status === 'Approved').length
)

const rejectedCount = computed(() => 
  leaveStore.leaveRequests.filter(r => r.status === 'Rejected').length
)

const recentRequests = computed(() => 
  [...leaveStore.leaveRequests].slice(-5).reverse()
)

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString()
}

const statusClass = (status) => {
  switch (status) {
    case 'Approved': return 'bg-green-100 text-green-800'
    case 'Rejected': return 'bg-red-100 text-red-800'
    case 'Pending': return 'bg-yellow-100 text-yellow-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}
</script>
