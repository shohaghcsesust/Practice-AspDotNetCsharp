<template>
  <div class="max-w-3xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="mb-8">
        <router-link to="/leave-requests" class="text-indigo-600 hover:text-indigo-900 flex items-center">
          <svg class="w-5 h-5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
          </svg>
          Back to Leave Requests
        </router-link>
      </div>

      <h1 class="text-3xl font-bold text-gray-900 mb-8">New Leave Request</h1>

      <div class="bg-white shadow rounded-lg p-6">
        <form @submit.prevent="handleSubmit">
          <div v-if="error" class="mb-6 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
            {{ error }}
          </div>

          <div class="space-y-6">
            <!-- Leave Type -->
            <div>
              <label for="leaveType" class="block text-sm font-medium text-gray-700">
                Leave Type <span class="text-red-500">*</span>
              </label>
              <select
                id="leaveType"
                v-model="form.leaveTypeId"
                required
                class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              >
                <option value="">Select a leave type</option>
                <option v-for="type in leaveTypes" :key="type.id" :value="type.id">
                  {{ type.name }} ({{ getBalance(type.id) }} days available)
                </option>
              </select>
            </div>

            <!-- Date Range -->
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label for="startDate" class="block text-sm font-medium text-gray-700">
                  Start Date <span class="text-red-500">*</span>
                </label>
                <input
                  id="startDate"
                  v-model="form.startDate"
                  type="date"
                  required
                  :min="minDate"
                  class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
              <div>
                <label for="endDate" class="block text-sm font-medium text-gray-700">
                  End Date <span class="text-red-500">*</span>
                </label>
                <input
                  id="endDate"
                  v-model="form.endDate"
                  type="date"
                  required
                  :min="form.startDate || minDate"
                  class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
            </div>

            <!-- Days Calculation -->
            <div v-if="totalDays > 0" class="bg-indigo-50 rounded-md p-4">
              <p class="text-sm text-indigo-700">
                <strong>Total Days:</strong> {{ totalDays }} day(s)
              </p>
            </div>

            <!-- Reason -->
            <div>
              <label for="reason" class="block text-sm font-medium text-gray-700">
                Reason
              </label>
              <textarea
                id="reason"
                v-model="form.reason"
                rows="4"
                placeholder="Please provide a reason for your leave request..."
                class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
              ></textarea>
            </div>

            <!-- Submit Button -->
            <div class="flex justify-end gap-4">
              <router-link
                to="/leave-requests"
                class="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
              >
                Cancel
              </router-link>
              <button
                type="submit"
                :disabled="loading"
                class="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50"
              >
                <span v-if="loading">Submitting...</span>
                <span v-else>Submit Request</span>
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, inject } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useLeaveStore } from '../stores/leave'

const router = useRouter()
const authStore = useAuthStore()
const leaveStore = useLeaveStore()
const showToast = inject('showToast')

const loading = ref(false)
const error = ref('')

const form = reactive({
  leaveTypeId: '',
  startDate: '',
  endDate: '',
  reason: ''
})

const leaveTypes = computed(() => leaveStore.leaveTypes)
const leaveBalances = computed(() => leaveStore.leaveBalances)

const minDate = computed(() => {
  const today = new Date()
  return today.toISOString().split('T')[0]
})

const totalDays = computed(() => {
  if (!form.startDate || !form.endDate) return 0
  const start = new Date(form.startDate)
  const end = new Date(form.endDate)
  const diffTime = end - start
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
  return diffDays > 0 ? diffDays : 0
})

const getBalance = (leaveTypeId) => {
  const balance = leaveBalances.value.find((b) => b.leaveTypeId === leaveTypeId)
  return balance ? balance.remainingDays : 0
}

const handleSubmit = async () => {
  error.value = ''
  loading.value = true

  const data = {
    employeeId: authStore.user.id,
    leaveTypeId: parseInt(form.leaveTypeId),
    startDate: form.startDate,
    endDate: form.endDate,
    reason: form.reason
  }

  const result = await leaveStore.createLeaveRequest(data)

  if (result.success) {
    showToast('Leave request submitted successfully!', 'success')
    router.push('/leave-requests')
  } else {
    error.value = result.message
  }

  loading.value = false
}

onMounted(async () => {
  await Promise.all([leaveStore.fetchLeaveTypes(), leaveStore.fetchLeaveBalances()])
})
</script>
