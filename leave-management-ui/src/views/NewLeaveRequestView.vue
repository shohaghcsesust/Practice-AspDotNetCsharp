<template>
  <div class="max-w-3xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">New Leave Request</h1>
      
      <div class="bg-white shadow rounded-lg p-6">
        <form @submit.prevent="submitRequest">
          <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded mb-4">
            {{ error }}
          </div>
          
          <div class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700">Leave Type</label>
              <select 
                v-model="form.leaveTypeId" 
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              >
                <option value="">Select a leave type</option>
                <option v-for="type in leaveStore.leaveTypes" :key="type.id" :value="type.id">
                  {{ type.name }} ({{ type.defaultDays }} days)
                </option>
              </select>
            </div>
            
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Start Date</label>
                <input 
                  v-model="form.startDate" 
                  type="date" 
                  required
                  :min="minDate"
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">End Date</label>
                <input 
                  v-model="form.endDate" 
                  type="date" 
                  required
                  :min="form.startDate || minDate"
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
            </div>
            
            <div v-if="calculatedDays > 0" class="bg-blue-50 border border-blue-200 text-blue-700 px-4 py-3 rounded">
              Total Days: {{ calculatedDays }}
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700">Reason</label>
              <textarea 
                v-model="form.reason" 
                required
                rows="4"
                placeholder="Please provide a reason for your leave request..."
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              ></textarea>
            </div>
            
            <div class="flex justify-end space-x-3">
              <router-link 
                to="/leave-requests"
                class="bg-gray-300 text-gray-700 px-4 py-2 rounded-md hover:bg-gray-400 transition"
              >
                Cancel
              </router-link>
              <button 
                type="submit"
                :disabled="loading"
                class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition disabled:opacity-50"
              >
                {{ loading ? 'Submitting...' : 'Submit Request' }}
              </button>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useLeaveStore } from '../stores/leave'

const router = useRouter()
const leaveStore = useLeaveStore()

const form = reactive({
  leaveTypeId: '',
  startDate: '',
  endDate: '',
  reason: ''
})

const error = ref('')
const loading = ref(false)

const minDate = computed(() => {
  return new Date().toISOString().split('T')[0]
})

const calculatedDays = computed(() => {
  if (!form.startDate || !form.endDate) return 0
  const start = new Date(form.startDate)
  const end = new Date(form.endDate)
  const diff = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1
  return diff > 0 ? diff : 0
})

onMounted(() => {
  leaveStore.fetchLeaveTypes()
})

const submitRequest = async () => {
  loading.value = true
  error.value = ''
  
  const result = await leaveStore.createLeaveRequest({
    leaveTypeId: parseInt(form.leaveTypeId),
    startDate: form.startDate,
    endDate: form.endDate,
    reason: form.reason
  })
  
  if (result.success) {
    router.push('/leave-requests')
  } else {
    error.value = result.message
  }
  
  loading.value = false
}
</script>
