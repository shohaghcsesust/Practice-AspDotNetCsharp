<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">Pending Approvals</h1>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Pending Requests -->
      <div v-else>
        <div v-if="pendingRequests.length === 0" class="bg-white shadow rounded-lg p-12 text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-4 text-lg font-medium text-gray-900">No pending requests</h3>
          <p class="mt-2 text-gray-500">All leave requests have been processed.</p>
        </div>

        <div v-else class="space-y-4">
          <div
            v-for="request in pendingRequests"
            :key="request.id"
            class="bg-white shadow rounded-lg p-6"
          >
            <div class="flex justify-between items-start">
              <div>
                <h3 class="text-lg font-semibold text-gray-900">{{ request.employeeName }}</h3>
                <p class="text-sm text-gray-500 mt-1">
                  {{ request.leaveTypeName }} • {{ request.totalDays }} day(s)
                </p>
              </div>
              <span class="px-2 py-1 text-xs font-semibold rounded-full bg-yellow-100 text-yellow-800">
                Pending
              </span>
            </div>

            <div class="mt-4 grid grid-cols-2 gap-4 text-sm">
              <div>
                <p class="text-gray-500">Start Date</p>
                <p class="font-medium">{{ formatDate(request.startDate) }}</p>
              </div>
              <div>
                <p class="text-gray-500">End Date</p>
                <p class="font-medium">{{ formatDate(request.endDate) }}</p>
              </div>
            </div>

            <div v-if="request.reason" class="mt-4">
              <p class="text-gray-500 text-sm">Reason</p>
              <p class="text-gray-700 mt-1">{{ request.reason }}</p>
            </div>

            <div class="mt-4">
              <label class="block text-sm text-gray-500 mb-2">Comments (optional)</label>
              <textarea
                v-model="comments[request.id]"
                rows="2"
                placeholder="Add comments..."
                class="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
              ></textarea>
            </div>

            <div class="mt-4 flex justify-end gap-3">
              <button
                @click="handleReject(request.id)"
                :disabled="processing[request.id]"
                class="px-4 py-2 border border-red-300 rounded-md text-sm font-medium text-red-700 bg-white hover:bg-red-50 disabled:opacity-50"
              >
                Reject
              </button>
              <button
                @click="handleApprove(request.id)"
                :disabled="processing[request.id]"
                class="px-4 py-2 border border-transparent rounded-md text-sm font-medium text-white bg-green-600 hover:bg-green-700 disabled:opacity-50"
              >
                Approve
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive, onMounted, inject } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useLeaveStore } from '../stores/leave'

const authStore = useAuthStore()
const leaveStore = useLeaveStore()
const showToast = inject('showToast')

const loading = ref(true)
const comments = reactive({})
const processing = reactive({})

const pendingRequests = computed(() => leaveStore.pendingRequests)

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('en-US', {
    weekday: 'short',
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

const handleApprove = async (id) => {
  processing[id] = true
  const result = await leaveStore.approveRequest(id, authStore.user.id, comments[id] || '')

  if (result.success) {
    showToast('Leave request approved successfully', 'success')
    await leaveStore.fetchPendingRequests()
  } else {
    showToast(result.message, 'error')
  }

  processing[id] = false
}

const handleReject = async (id) => {
  processing[id] = true
  const result = await leaveStore.rejectRequest(id, authStore.user.id, comments[id] || '')

  if (result.success) {
    showToast('Leave request rejected', 'success')
    await leaveStore.fetchPendingRequests()
  } else {
    showToast(result.message, 'error')
  }

  processing[id] = false
}

onMounted(async () => {
  await leaveStore.fetchPendingRequests()
  loading.value = false
})
</script>
