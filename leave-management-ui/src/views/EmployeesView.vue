<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Employees</h1>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-12">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-indigo-500 border-t-transparent"></div>
      </div>

      <!-- Employees Table -->
      <div v-else class="bg-white shadow overflow-hidden sm:rounded-lg">
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Employee
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Email
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Department
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Position
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Role
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Status
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                Actions
              </th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="employee in employees" :key="employee.id">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="h-10 w-10 flex-shrink-0">
                    <div class="h-10 w-10 rounded-full bg-indigo-500 flex items-center justify-center text-white font-medium">
                      {{ getInitials(employee) }}
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">
                      {{ employee.firstName }} {{ employee.lastName }}
                    </div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ employee.email }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ employee.department || '-' }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ employee.position || '-' }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <select
                  :value="employee.role"
                  @change="updateRole(employee.id, $event.target.value)"
                  class="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
                >
                  <option value="Employee">Employee</option>
                  <option value="Manager">Manager</option>
                  <option value="Admin">Admin</option>
                </select>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  class="px-2 py-1 inline-flex text-xs leading-5 font-semibold rounded-full"
                  :class="employee.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                >
                  {{ employee.isActive ? 'Active' : 'Inactive' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm">
                <button
                  @click="viewBalances(employee.id)"
                  class="text-indigo-600 hover:text-indigo-900"
                >
                  View Balances
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Balances Modal -->
      <div v-if="showBalancesModal" class="fixed inset-0 bg-gray-600 bg-opacity-50 flex items-center justify-center z-50">
        <div class="bg-white rounded-lg shadow-xl max-w-lg w-full mx-4">
          <div class="px-6 py-4 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900">Leave Balances</h3>
          </div>
          <div class="px-6 py-4">
            <div v-if="loadingBalances" class="text-center py-4">
              <div class="inline-block animate-spin rounded-full h-6 w-6 border-4 border-indigo-500 border-t-transparent"></div>
            </div>
            <div v-else-if="selectedBalances.length === 0" class="text-center py-4 text-gray-500">
              No leave balances found
            </div>
            <div v-else class="space-y-3">
              <div v-for="balance in selectedBalances" :key="balance.id" class="flex justify-between items-center p-3 bg-gray-50 rounded">
                <span class="font-medium">{{ balance.leaveTypeName }}</span>
                <span>
                  <span class="text-indigo-600 font-semibold">{{ balance.remainingDays }}</span>
                  <span class="text-gray-500"> / {{ balance.totalDays }} days</span>
                </span>
              </div>
            </div>
          </div>
          <div class="px-6 py-4 border-t border-gray-200 flex justify-end">
            <button
              @click="showBalancesModal = false"
              class="px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, inject } from 'vue'
import api from '../services/api'

const showToast = inject('showToast')

const loading = ref(true)
const employees = ref([])
const showBalancesModal = ref(false)
const loadingBalances = ref(false)
const selectedBalances = ref([])

const getInitials = (employee) => {
  return `${employee.firstName[0]}${employee.lastName[0]}`.toUpperCase()
}

const fetchEmployees = async () => {
  try {
    const response = await api.get('/admin/users')
    employees.value = response.data
  } catch (error) {
    showToast('Failed to fetch employees', 'error')
  } finally {
    loading.value = false
  }
}

const updateRole = async (id, role) => {
  try {
    await api.put(`/admin/users/${id}/role`, JSON.stringify(role), {
      headers: { 'Content-Type': 'application/json' }
    })
    showToast('Role updated successfully', 'success')
    await fetchEmployees()
  } catch (error) {
    showToast('Failed to update role', 'error')
  }
}

const viewBalances = async (employeeId) => {
  showBalancesModal.value = true
  loadingBalances.value = true
  selectedBalances.value = []

  try {
    const response = await api.get(`/leavebalance/employee/${employeeId}`)
    selectedBalances.value = response.data
  } catch (error) {
    showToast('Failed to fetch balances', 'error')
  } finally {
    loadingBalances.value = false
  }
}

onMounted(fetchEmployees)
</script>
