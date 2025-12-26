<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Leave Requests</h1>
        <router-link 
          to="/leave-requests/new" 
          class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition"
        >
          New Request
        </router-link>
      </div>
      
      <!-- Tabs -->
      <div class="border-b border-gray-200 mb-6">
        <nav class="-mb-px flex space-x-8">
          <button
            @click="activeTab = 'my'"
            :class="[activeTab === 'my' ? 'border-indigo-500 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
            class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"
          >
            My Requests
          </button>
          <button
            v-if="authStore.isManagerOrAdmin"
            @click="activeTab = 'pending'; loadPending()"
            :class="[activeTab === 'pending' ? 'border-indigo-500 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
            class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"
          >
            Pending Approvals
            <span
              v-if="leaveStore.pendingCount > 0"
              class="ml-2 bg-red-100 text-red-600 px-2 py-0.5 rounded-full text-xs"
            >
              {{ leaveStore.pendingCount }}
            </span>
          </button>
        </nav>
      </div>

      <!-- My Requests Tab -->
      <div v-if="activeTab === 'my'" class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="leaveStore.loading" class="text-center py-8">Loading...</div>
        <div v-else-if="leaveStore.leaveRequests.length === 0" class="text-center py-8 text-gray-500">
          No leave requests found
        </div>
        <table v-else class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Start Date</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">End Date</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Days</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="request in leaveStore.leaveRequests" :key="request.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ request.leaveTypeName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(request.startDate) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ formatDate(request.endDate) }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ request.totalDays }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  :class="statusClass(request.status)"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ request.status }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button
                  v-if="request.status === 'Pending'"
                  @click="cancelRequest(request.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  Cancel
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pending Approvals Tab -->
      <div v-if="activeTab === 'pending'" class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="leaveStore.loading" class="text-center py-8">Loading...</div>
        <div v-else-if="leaveStore.pendingRequests.length === 0" class="text-center py-8 text-gray-500">
          No pending requests
        </div>
        <table v-else class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Dates</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Days</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reason</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="request in leaveStore.pendingRequests" :key="request.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{{ request.employeeName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ request.leaveTypeName }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(request.startDate) }} - {{ formatDate(request.endDate) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ request.totalDays }}</td>
              <td class="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">{{ request.reason }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm space-x-2">
                <button
                  @click="approveRequest(request.id)"
                  class="bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700"
                >
                  Approve
                </button>
                <button
                  @click="showRejectModal(request.id)"
                  class="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
                >
                  Reject
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Reject Modal -->
    <div v-if="rejectModalOpen" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 class="text-lg font-medium text-gray-900 mb-4">Reject Leave Request</h3>
        <textarea
          v-model="rejectComments"
          placeholder="Enter rejection reason..."
          class="w-full border border-gray-300 rounded-md p-2 mb-4"
          rows="3"
        ></textarea>
        <div class="flex justify-end space-x-2">
          <button
            @click="rejectModalOpen = false"
            class="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400"
          >
            Cancel
          </button>
          <button
            @click="confirmReject"
            class="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
          >
            Reject
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useLeaveStore } from '../stores/leave'
import { useAuthStore } from '../stores/auth'

const leaveStore = useLeaveStore()
const authStore = useAuthStore()

const activeTab = ref('my')
const rejectModalOpen = ref(false)
const rejectComments = ref('')
const selectedRequestId = ref(null)

onMounted(() => {
  leaveStore.fetchMyLeaveRequests()
  if (authStore.isManagerOrAdmin) {
    leaveStore.fetchPendingRequests()
  }
})

const loadPending = () => {
  leaveStore.fetchPendingRequests()
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString()
}

const statusClass = (status) => {
  switch (status) {
    case 'Approved': return 'bg-green-100 text-green-800'
    case 'Rejected': return 'bg-red-100 text-red-800'
    case 'Pending': return 'bg-yellow-100 text-yellow-800'
    case 'Cancelled': return 'bg-gray-100 text-gray-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

const cancelRequest = async (id) => {
  if (confirm('Are you sure you want to cancel this request?')) {
    const result = await leaveStore.cancelLeaveRequest(id)
    if (!result.success) {
      alert(result.message)
    }
  }
}

const approveRequest = async (id) => {
  const result = await leaveStore.approveLeaveRequest(id, 'Approved')
  if (!result.success) {
    alert(result.message)
  }
}

const showRejectModal = (id) => {
  selectedRequestId.value = id
  rejectComments.value = ''
  rejectModalOpen.value = true
}

const confirmReject = async () => {
  if (!rejectComments.value.trim()) {
    alert('Please provide a rejection reason')
    return
  }
  const result = await leaveStore.rejectLeaveRequest(selectedRequestId.value, rejectComments.value)
  if (!result.success) {
    alert(result.message)
  }
  rejectModalOpen.value = false
}
</script>