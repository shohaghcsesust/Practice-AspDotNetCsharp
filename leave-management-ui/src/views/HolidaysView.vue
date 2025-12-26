<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Public Holidays</h1>
        <button 
          @click="showAddModal = true"
          class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition"
        >
          Add Holiday
        </button>
      </div>
      
      <!-- Year Filter -->
      <div class="bg-white shadow rounded-lg p-4 mb-6">
        <div class="flex items-center">
          <label class="mr-4 text-sm font-medium text-gray-700">Year:</label>
          <select 
            v-model="selectedYear"
            @change="fetchHolidays"
            class="border border-gray-300 rounded-md px-3 py-2"
          >
            <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
          </select>
        </div>
      </div>
      
      <!-- Holidays List -->
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="loading" class="text-center py-8">Loading...</div>
        <div v-else-if="holidays.length === 0" class="text-center py-8 text-gray-500">
          No holidays found for {{ selectedYear }}
        </div>
        <div v-else class="divide-y divide-gray-200">
          <div 
            v-for="holiday in sortedHolidays" 
            :key="holiday.id"
            class="p-4 flex items-center justify-between hover:bg-gray-50"
          >
            <div class="flex items-center">
              <div 
                class="w-16 h-16 rounded-lg flex flex-col items-center justify-center mr-4"
                :class="isPastHoliday(holiday.date) ? 'bg-gray-100' : 'bg-indigo-100'"
              >
                <span class="text-xs font-medium text-gray-500">{{ getMonth(holiday.date) }}</span>
                <span class="text-xl font-bold" :class="isPastHoliday(holiday.date) ? 'text-gray-500' : 'text-indigo-600'">
                  {{ getDay(holiday.date) }}
                </span>
              </div>
              <div>
                <h3 class="text-lg font-medium text-gray-900">{{ holiday.name }}</h3>
                <p class="text-sm text-gray-500">
                  {{ formatDate(holiday.date) }}
                  <span v-if="holiday.isRecurring" class="ml-2 text-indigo-600">
                    (Recurring yearly)
                  </span>
                </p>
                <p v-if="holiday.description" class="text-sm text-gray-400 mt-1">
                  {{ holiday.description }}
                </p>
              </div>
            </div>
            <div class="flex space-x-2">
              <button 
                @click="editHoliday(holiday)"
                class="text-indigo-600 hover:text-indigo-900 px-3 py-1"
              >
                Edit
              </button>
              <button 
                @click="deleteHoliday(holiday.id)"
                class="text-red-600 hover:text-red-900 px-3 py-1"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Add/Edit Modal -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full">
        <h3 class="text-lg font-medium text-gray-900 mb-4">
          {{ showEditModal ? 'Edit Holiday' : 'Add Holiday' }}
        </h3>
        <form @submit.prevent="saveHoliday">
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">Holiday Name</label>
              <input 
                v-model="form.name" 
                type="text" 
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Date</label>
              <input 
                v-model="form.date" 
                type="date" 
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
            <div class="flex items-center">
              <input 
                v-model="form.isRecurring" 
                type="checkbox" 
                id="isRecurring"
                class="h-4 w-4 text-indigo-600 focus:ring-indigo-500 border-gray-300 rounded"
              />
              <label for="isRecurring" class="ml-2 block text-sm text-gray-900">
                Recurring every year
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
import { ref, reactive, computed, onMounted } from 'vue'
import api from '../services/api'

const holidays = ref([])
const loading = ref(false)
const showAddModal = ref(false)
const showEditModal = ref(false)
const editingId = ref(null)
const selectedYear = ref(new Date().getFullYear())

const years = computed(() => {
  const currentYear = new Date().getFullYear()
  return [currentYear - 1, currentYear, currentYear + 1, currentYear + 2]
})

const form = reactive({
  name: '',
  date: '',
  description: '',
  isRecurring: false
})

const sortedHolidays = computed(() => {
  return [...holidays.value].sort((a, b) => new Date(a.date) - new Date(b.date))
})

const fetchHolidays = async () => {
  loading.value = true
  try {
    const response = await api.get(`/api/publicholidays?year=${selectedYear.value}`)
    holidays.value = response.data
  } catch (error) {
    console.error('Failed to fetch holidays:', error)
  }
  loading.value = false
}

const getMonth = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('en-US', { month: 'short' })
}

const getDay = (dateStr) => {
  return new Date(dateStr).getDate()
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('en-US', { 
    weekday: 'long',
    year: 'numeric', 
    month: 'long', 
    day: 'numeric' 
  })
}

const isPastHoliday = (dateStr) => {
  return new Date(dateStr) < new Date()
}

const editHoliday = (holiday) => {
  editingId.value = holiday.id
  form.name = holiday.name
  form.date = holiday.date.split('T')[0]
  form.description = holiday.description || ''
  form.isRecurring = holiday.isRecurring
  showEditModal.value = true
}

const saveHoliday = async () => {
  try {
    if (showEditModal.value) {
      await api.put(`/api/publicholidays/${editingId.value}`, form)
    } else {
      await api.post('/api/publicholidays', form)
    }
    await fetchHolidays()
    closeModal()
  } catch (error) {
    console.error('Failed to save holiday:', error)
    alert(error.response?.data?.message || 'Failed to save holiday')
  }
}

const deleteHoliday = async (id) => {
  if (confirm('Are you sure you want to delete this holiday?')) {
    try {
      await api.delete(`/api/publicholidays/${id}`)
      await fetchHolidays()
    } catch (error) {
      console.error('Failed to delete holiday:', error)
      alert(error.response?.data?.message || 'Failed to delete holiday')
    }
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  editingId.value = null
  form.name = ''
  form.date = ''
  form.description = ''
  form.isRecurring = false
}

onMounted(() => {
  fetchHolidays()
})
</script>
