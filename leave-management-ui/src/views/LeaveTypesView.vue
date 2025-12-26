<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Leave Types</h1>
        <button
          @click="openCreateModal"
          class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
        >
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4" />
          </svg>
          Add Leave Type
        </button>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Leave Types Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div
          v-for="leaveType in leaveTypes"
          :key="leaveType.id"
          class="bg-white shadow rounded-lg p-6"
        >
          <div class="flex justify-between items-start mb-4">
            <div>
              <h3 class="text-lg font-semibold text-gray-900">{{ leaveType.name }}</h3>
              <span
                class="mt-1 px-2 py-1 text-xs font-semibold rounded-full inline-block"
                :class="leaveType.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'"
              >
                {{ leaveType.isActive ? 'Active' : 'Inactive' }}
              </span>
            </div>
            <button
              @click="openEditModal(leaveType)"
              class="text-indigo-600 hover:text-indigo-900"
            >
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
              </svg>
            </button>
          </div>
          <p class="text-sm text-gray-500 mb-4">
            {{ leaveType.description || 'No description' }}
          </p>
          <div class="text-sm">
            <span class="text-gray-500">Default Days:</span>
            <span class="ml-2 font-semibold text-indigo-600">{{ leaveType.defaultDays }} days</span>
          </div>
        </div>
      </div>

      <!-- Create/Edit Modal -->
      <div v-if="showModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
        <div class="bg-white rounded-lg shadow-xl max-w-md w-full mx-4">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900">
              {{ isEditing ? 'Edit Leave Type' : 'Create Leave Type' }}
            </h3>
          </div>
          <form @submit.prevent="handleSubmit">
            <div class="px-6 py-4 space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">Name</label>
                <input
                  v-model="form.name"
                  type="text"
                  required
                  class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Description</label>
                <textarea
                  v-model="form.description"
                  rows="3"
                  class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                ></textarea>
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Default Days</label>
                <input
                  v-model.number="form.defaultDays"
                  type="number"
                  min="1"
                  max="365"
                  required
                  class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"
                />
              </div>
              <div v-if="isEditing" class="flex items-center">
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
            <div class="px-6 py-4 border-t border-gray-200 flex justify-end gap-3">
              <button
                type="button"
                @click="showModal = false"
                class="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
              >
                Cancel
              </button>
              <button
                type="submit"
                :disabled="submitting"
                class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:opacity-50"
              >
                {{ submitting ? 'Saving...' : 'Save' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, inject } from 'vue'
import api from '../services/api'

const showToast = inject('showToast')

const loading = ref(true)
const leaveTypes = ref([])
const showModal = ref(false)
const isEditing = ref(false)
const editingId = ref(null)
const submitting = ref(false)

const form = reactive({
  name: '',
  description: '',
  defaultDays: 10,
  isActive: true
})

const resetForm = () => {
  form.name = ''
  form.description = ''
  form.defaultDays = 10
  form.isActive = true
}

const fetchLeaveTypes = async () => {
  try {
    const response = await api.get('/leavetypes')
    leaveTypes.value = response.data
  } catch (error) {
    showToast('Failed to fetch leave types', 'error')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  isEditing.value = false
  editingId.value = null
  showModal.value = true
}

const openEditModal = (leaveType) => {
  form.name = leaveType.name
  form.description = leaveType.description
  form.defaultDays = leaveType.defaultDays
  form.isActive = leaveType.isActive
  isEditing.value = true
  editingId.value = leaveType.id
  showModal.value = true
}

const handleSubmit = async () => {
  submitting.value = true

  try {
    if (isEditing.value) {
      await api.put(`/leavetypes/${editingId.value}`, form)
      showToast('Leave type updated successfully', 'success')
    } else {
      await api.post('/leavetypes', form)
      showToast('Leave type created successfully', 'success')
    }
    showModal.value = false
    await fetchLeaveTypes()
  } catch (error) {
    showToast(error.response?.data?.message || 'Operation failed', 'error')
  } finally {
    submitting.value = false
  }
}

onMounted(fetchLeaveTypes)
</script>
