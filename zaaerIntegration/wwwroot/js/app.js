// Zaaer Integration API Test - JavaScript Functions
// Additional utility functions for the frontend

// Global variables
let currentPage = 1;
let pageSize = 10;
let totalPages = 1;

// Utility functions
const Utils = {
    // Format date for display
    formatDate: (dateString) => {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleDateString('ar-SA', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    },

    // Format currency
    formatCurrency: (amount) => {
        if (!amount) return '0.00';
        return new Intl.NumberFormat('ar-SA', {
            style: 'currency',
            currency: 'SAR'
        }).format(amount);
    },

    // Show notification
    showNotification: (message, type = 'info') => {
        const alertClass = {
            'success': 'alert-success',
            'error': 'alert-danger',
            'warning': 'alert-warning',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const notification = document.createElement('div');
        notification.className = `alert ${alertClass} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(notification);

        // Auto remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    },

    // Validate email
    isValidEmail: (email) => {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    },

    // Validate phone number
    isValidPhone: (phone) => {
        const phoneRegex = /^[\+]?[0-9\s\-\(\)]{10,}$/;
        return phoneRegex.test(phone);
    },

    // Debounce function for search
    debounce: (func, wait) => {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// Enhanced API functions
const API = {
    baseURL: window.location.origin,

    // Generic API call
    call: async (endpoint, options = {}) => {
        try {
            const response = await fetch(`${API.baseURL}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers
                },
                ...options
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            } else {
                return await response.text();
            }
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    // Health check
    health: () => API.call('/api/Health'),

    // Hotel endpoints
    hotels: {
        getAll: (page = 1, size = 10, search = '') => 
            API.call(`/api/Hotel?pageNumber=${page}&pageSize=${size}&searchTerm=${encodeURIComponent(search)}`),
        
        getById: (id) => API.call(`/api/Hotel/${id}`),
        
        getByCode: (code) => API.call(`/api/Hotel/code/${code}`),
        
        create: (data) => API.call('/api/Hotel', {
            method: 'POST',
            body: JSON.stringify(data)
        }),
        
        update: (id, data) => API.call(`/api/Hotel/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data)
        }),
        
        delete: (id) => API.call(`/api/Hotel/${id}`, {
            method: 'DELETE'
        }),
        
        search: (name) => API.call(`/api/Hotel/search?name=${encodeURIComponent(name)}`),
        
        statistics: () => API.call('/api/Hotel/statistics'),
        
        checkCode: (code, excludeId = null) => 
            API.call(`/api/Hotel/check-code?code=${encodeURIComponent(code)}${excludeId ? `&excludeId=${excludeId}` : ''}`)
    },

    // Customer endpoints
    customers: {
        getAll: (page = 1, size = 10, search = '') => 
            API.call(`/api/Customer?pageNumber=${page}&pageSize=${size}&searchTerm=${encodeURIComponent(search)}`),
        
        getById: (id) => API.call(`/api/Customer/${id}`),
        
        create: (data) => API.call('/api/Customer', {
            method: 'POST',
            body: JSON.stringify(data)
        }),
        
        update: (id, data) => API.call(`/api/Customer/${id}`, {
            method: 'PUT',
            body: JSON.stringify(data)
        }),
        
        delete: (id) => API.call(`/api/Customer/${id}`, {
            method: 'DELETE'
        }),
        
        search: (name) => API.call(`/api/Customer?searchTerm=${encodeURIComponent(name)}`),
        
        statistics: () => API.call('/api/Customer/statistics')
    }
};

// Enhanced UI functions
const UI = {
    // Show loading state
    showLoading: (elementId = 'loading') => {
        const loading = document.getElementById(elementId);
        if (loading) {
            loading.style.display = 'block';
        }
    },

    // Hide loading state
    hideLoading: (elementId = 'loading') => {
        const loading = document.getElementById(elementId);
        if (loading) {
            loading.style.display = 'none';
        }
    },

    // Update API status
    updateApiStatus: (status, message) => {
        const statusElement = document.getElementById('apiStatus');
        if (statusElement) {
            statusElement.className = `status-badge status-${status}`;
            statusElement.innerHTML = `<i class="fas fa-${status === 'success' ? 'check-circle' : 'times-circle'}"></i> API Status: ${message}`;
        }
    },

    // Clear form
    clearForm: (formId) => {
        const form = document.getElementById(formId);
        if (form) {
            form.reset();
        }
    },

    // Validate form
    validateForm: (formId) => {
        const form = document.getElementById(formId);
        if (!form) return false;

        const requiredFields = form.querySelectorAll('[required]');
        let isValid = true;

        requiredFields.forEach(field => {
            if (!field.value.trim()) {
                field.classList.add('is-invalid');
                isValid = false;
            } else {
                field.classList.remove('is-invalid');
                field.classList.add('is-valid');
            }
        });

        return isValid;
    }
};

// Enhanced search with debouncing
const debouncedSearch = Utils.debounce((searchTerm, searchType) => {
    if (searchTerm.length < 2) return;
    
    if (searchType === 'hotel') {
        searchHotels();
    } else if (searchType === 'customer') {
        searchCustomers();
    }
}, 500);

// Add event listeners for enhanced functionality
document.addEventListener('DOMContentLoaded', function() {
    // Add search event listeners
    const hotelSearch = document.getElementById('searchHotel');
    const customerSearch = document.getElementById('searchCustomer');
    
    if (hotelSearch) {
        hotelSearch.addEventListener('input', (e) => {
            debouncedSearch(e.target.value, 'hotel');
        });
    }
    
    if (customerSearch) {
        customerSearch.addEventListener('input', (e) => {
            debouncedSearch(e.target.value, 'customer');
        });
    }

    // Add form validation
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            if (!UI.validateForm(form.id)) {
                Utils.showNotification('Please fill in all required fields', 'warning');
            }
        });
    });

    // Initialize tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});

// Export functions for global use
window.Utils = Utils;
window.API = API;
window.UI = UI;
