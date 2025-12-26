<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">My Leave Requests</h1>
        <router-link
          to="/leave-requests/new"
          class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          New Request
        </router-link>
      </div>

      <!-- Filters -->
      <div class="bg-white shadow rounded-lg mb-6 p-4">
        <div class="flex flex-wrap gap-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              v-model="statusFilter"
              class="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="">All Status</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Leave Type</label>
            <select
              v-model="typeFilter"
              class="border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option value="">All Types</option>
              <option v-for="type in leaveTypes" :key="type.id" :value="type.name">
                {{ type.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Leave Requests Table -->
      <div v-else class="bg-white shadow overflow-hidden sm:rounded-lg">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Leave Type
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Dates
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Days
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Reason
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="request in filteredRequests" :key="request.id">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm font-medium text-gray-900">{{ request.leaveTypeName }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">
                  {{ formatDate(request.startDate) }} - {{ formatDate(request.endDate) }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ request.totalDays }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
                  :class="getStatusClass(request.status)"
                >
                  {{ request.status }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="text-sm text-gray-500 max-w-xs truncate">
                  {{ request.reason || '-' }}
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button
                  v-if="request.status === 'Pending'"
                  @click="handleCancel(request.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  Cancel
                </button>
                <span v-else-if="request.approverComments" class="text-gray-500">
                  {{ request.approverComments }}
                </span>
                <span v-else class="text-gray-400">-</span>
              </td>
            </tr>
            <tr v-if="filteredRequests.length === 0">
              <td colspan="6" class="px-6 py-12 text-center text-gray-500">
                No leave requests found
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, inject } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useLeaveStore } from '../stores/leave'

const authStore = useAuthStore()
const leaveStore = useLeaveStore()
const showToast = inject('showToast')

const loading = ref(true)
const statusFilter = ref('')
const typeFilter = ref('')

const leaveRequests = computed(() => leaveStore.leaveRequests)
const leaveTypes = computed(() => leaveStore.leaveTypes)

const filteredRequests = computed(() => {
  return leaveRequests.value.filter((request) => {
    if (statusFilter.value && request.status !== statusFilter.value) return false
    if (typeFilter.value && request.leaveTypeName !== typeFilter.value) return false
    return true
  })
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

const handleCancel = async (id) => {
  if (!confirm('Are you sure you want to cancel this leave request?')) return

  const result = await leaveStore.cancelRequest(id)
  if (result.success) {
    showToast('Leave request cancelled successfully', 'success')
    await leaveStore.fetchMyLeaveRequests(authStore.user.id)
  } else {
    showToast(result.message, 'error')
  }
}

onMounted(async () => {
  try {
    await Promise.all([
      leaveStore.fetchMyLeaveRequests(authStore.user.id),
      leaveStore.fetchLeaveTypes()
    ])
  } catch (error) {
    console.error('Error loading leave requests:', error)
  } finally {
    loading.value = false
  }
})
</script>
