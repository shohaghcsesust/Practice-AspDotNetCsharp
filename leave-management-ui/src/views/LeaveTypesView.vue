<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Leave Types</h1>
        <button 
          @click="showAddModal = true"
          class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition"
        >
          Add Leave Type
        </button>
      </div>
      
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="loading" class="text-center py-8">Loading...</div>
        <div v-else-if="leaveStore.leaveTypes.length === 0" class="text-center py-8 text-gray-500">
          No leave types found
        </div>
        <table v-else class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Default Days</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Carry Forward</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="type in leaveStore.leaveTypes" :key="type.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{{ type.name }}</td>
              <td class="px-6 py-4 text-sm text-gray-500 max-w-xs truncate">{{ type.description }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ type.defaultDays }} days</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ type.allowCarryForward ? `Yes (Max: ${type.maxCarryForwardDays})` : 'No' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span 
                  :class="type.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ type.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm space-x-2">
                <button 
                  @click="editLeaveType(type)"
                  class="text-indigo-600 hover:text-indigo-900"
                >
                  Edit
                </button>
                <button 
                  @click="deleteLeaveType(type.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  Delete
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    
    <!-- Add/Edit Modal -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 class="text-lg font-medium text-gray-900 mb-4">
          {{ showEditModal ? 'Edit Leave Type' : 'Add Leave Type' }}
        </h3>
        <form @submit.prevent="saveLeaveType">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Name</label>
              <input 
                v-model="form.name" 
                type="text" 
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Description</label>
              <textarea 
                v-model="form.description" 
                rows="2"
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              ></textarea>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Default Days</label>
              <input 
                v-model.number="form.defaultDays" 
                type="number" 
                min="1"
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div class="flex items-center">
              <input 
                v-model="form.allowCarryForward" 
                type="checkbox" 
                id="allowCarryForward"
                class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label for="allowCarryForward" class="ml-2 block text-sm text-gray-900">
                Allow Carry Forward
              </label>
            </div>
            <div v-if="form.allowCarryForward">
              <label class="block text-sm font-medium text-gray-700">Max Carry Forward Days</label>
              <input 
                v-model.number="form.maxCarryForwardDays" 
                type="number" 
                min="0"
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div class="flex items-center">
              <input 
                v-model="form.isActive" 
                type="checkbox" 
                id="isActive"
                class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label for="isActive" class="ml-2 block text-sm text-gray-900">
                Active
              </label>
            </div>
          </div>
          <div class="flex justify-end space-x-2 mt-6">
            <button 
              type="button"
              @click="closeModal"
              class="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400"
            >
              Cancel
            </button>
            <button 
              type="submit"
              class="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700"
            >
              {{ showEditModal ? 'Update' : 'Add' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '../services/api'
import { useLeaveStore } from '../stores/leave'

const leaveStore = useLeaveStore()
const loading = ref(false)
const showAddModal = ref(false)
const showEditModal = ref(false)
const editingId = ref(null)

const form = reactive({
  name: '',
  description: '',
  defaultDays: 10,
  allowCarryForward: false,
  maxCarryForwardDays: 0,
  isActive: true
})

onMounted(() => {
  leaveStore.fetchLeaveTypes()
})

const editLeaveType = (type) => {
  editingId.value = type.id
  form.name = type.name
  form.description = type.description || ''
  form.defaultDays = type.defaultDays
  form.allowCarryForward = type.allowCarryForward
  form.maxCarryForwardDays = type.maxCarryForwardDays || 0
  form.isActive = type.isActive
  showEditModal.value = true
}

const saveLeaveType = async () => {
  try {
    if (showEditModal.value) {
      await api.put(`/api/leavetypes/${editingId.value}`, form)
    } else {
      await api.post('/api/leavetypes', form)
    }
    await leaveStore.fetchLeaveTypes()
    closeModal()
  } catch (error) {
    console.error('Failed to save leave type:', error)
    alert(error.response?.data?.message || 'Failed to save leave type')
  }
}

const deleteLeaveType = async (id) => {
  if (confirm('Are you sure you want to delete this leave type?')) {
    try {
      await api.delete(`/api/leavetypes/${id}`)
      await leaveStore.fetchLeaveTypes()
    } catch (error) {
      console.error('Failed to delete leave type:', error)
      alert(error.response?.data?.message || 'Failed to delete leave type')
    }
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  editingId.value = null
  form.name = ''
  form.description = ''
  form.defaultDays = 10
  form.allowCarryForward = false
  form.maxCarryForwardDays = 0
  form.isActive = true
}
</script>
