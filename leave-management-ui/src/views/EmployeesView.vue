<template>
  <div class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
    <div class="px-4 py-6 sm:px-0">
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-bold text-gray-900">Employees</h1>
        <button 
          @click="showAddModal = true"
          class="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700 transition"
        >
          Add Employee
        </button>
      </div>
      
      <div class="bg-white shadow rounded-lg overflow-hidden">
        <div v-if="loading" class="text-center py-8">Loading...</div>
        <div v-else-if="employees.length === 0" class="text-center py-8 text-gray-500">
          No employees found
        </div>
        <table v-else class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Department</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Position</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="employee in employees" :key="employee.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                {{ employee.firstName }} {{ employee.lastName }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ employee.email }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ employee.department }}</td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{{ employee.position }}</td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span
                  :class="roleClass(employee.role)"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ employee.role }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm space-x-2">
                <button
                  @click="editEmployee(employee)"
                  class="text-indigo-600 hover:text-indigo-900"
                >
                  Edit
                </button>
                <button
                  @click="changeRole(employee)"
                  class="text-blue-600 hover:text-blue-900"
                >
                  Change Role
                </button>
                <button
                  @click="deleteEmployee(employee.id)"
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
          {{ showEditModal ? 'Edit Employee' : 'Add Employee' }}
        </h3>
        <form @submit.prevent="saveEmployee">
          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-medium text-gray-700">First Name</label>
                <input
                  v-model="form.firstName"
                  type="text"
                  required
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
              <div>
                <label class="block text-sm font-medium text-gray-700">Last Name</label>
                <input
                  v-model="form.lastName"
                  type="text"
                  required
                  class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
                />
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Email</label>
              <input
                v-model="form.email"
                type="email"
                required
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Department</label>
              <input
                v-model="form.department"
                type="text"
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Position</label>
              <input
                v-model="form.position"
                type="text"
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>
            <div v-if="!showEditModal">
              <label class="block text-sm font-medium text-gray-700">Password</label>
              <input
                v-model="form.password"
                type="password"
                :required="!showEditModal"
                class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
              />
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

    <!-- Change Role Modal -->
    <div v-if="showRoleModal" class="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-sm w-full">
        <h3 class="text-lg font-medium text-gray-900 mb-4">
          Change Role for {{ selectedEmployee?.firstName }} {{ selectedEmployee?.lastName }}
        </h3>
        <div class="mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2">Select Role</label>
          <select
            v-model="selectedRole"
            class="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"
          >
            <option value="Employee">Employee</option>
            <option value="Manager">Manager</option>
            <option value="Admin">Admin</option>
          </select>
        </div>
        <div class="flex justify-end space-x-2">
          <button
            type="button"
            @click="showRoleModal = false"
            class="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400"
          >
            Cancel
          </button>
          <button
            @click="updateRole"
            class="bg-indigo-600 text-white px-4 py-2 rounded hover:bg-indigo-700"
          >
            Update Role
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import api from '../services/api'

const employees = ref([])
const loading = ref(false)
const showAddModal = ref(false)
const showEditModal = ref(false)
const showRoleModal = ref(false)
const editingId = ref(null)
const selectedEmployee = ref(null)
const selectedRole = ref('Employee')

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  department: '',
  position: '',
  password: ''
})

onMounted(() => {
  fetchEmployees()
})

const fetchEmployees = async () => {
  loading.value = true
  try {
    const response = await api.get('/api/admin/users')
    employees.value = response.data
  } catch (error) {
    console.error('Failed to fetch employees:', error)
  }
  loading.value = false
}

const roleClass = (role) => {
  switch (role) {
    case 'Admin': return 'bg-purple-100 text-purple-800'
    case 'Manager': return 'bg-blue-100 text-blue-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

const editEmployee = (employee) => {
  editingId.value = employee.id
  form.firstName = employee.firstName
  form.lastName = employee.lastName
  form.email = employee.email
  form.department = employee.department
  form.position = employee.position
  showEditModal.value = true
}

const changeRole = (employee) => {
  selectedEmployee.value = employee
  selectedRole.value = employee.role
  showRoleModal.value = true
}

const updateRole = async () => {
  try {
    await api.put(`/api/admin/users/${selectedEmployee.value.id}/role`, JSON.stringify(selectedRole.value), {
      headers: { 'Content-Type': 'application/json' }
    })
    await fetchEmployees()
    showRoleModal.value = false
  } catch (error) {
    console.error('Failed to update role:', error)
    alert(error.response?.data?.message || 'Failed to update role')
  }
}

const saveEmployee = async () => {
  try {
    if (showEditModal.value) {
      await api.put(`/api/employees/${editingId.value}`, {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        department: form.department,
        position: form.position,
        isActive: true
      })
    } else {
      await api.post('/api/auth/register', {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        department: form.department,
        position: form.position
      })
    }
    await fetchEmployees()
    closeModal()
  } catch (error) {
    console.error('Failed to save employee:', error)
    alert(error.response?.data?.message || 'Failed to save employee')
  }
}

const deleteEmployee = async (id) => {
  if (confirm('Are you sure you want to delete this employee?')) {
    try {
      await api.delete(`/api/employees/${id}`)
      await fetchEmployees()
    } catch (error) {
      console.error('Failed to delete employee:', error)
      alert(error.response?.data?.message || 'Failed to delete employee')
    }
  }
}

const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  editingId.value = null
  form.firstName = ''
  form.lastName = ''
  form.email = ''
  form.department = ''
  form.position = ''
  form.password = ''
}
</script>