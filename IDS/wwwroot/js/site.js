// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/**
 * IDS Security System - Main JavaScript
 */

(function () {
    'use strict';

    // ============================================
    // Theme Management
    // ============================================
    const ThemeManager = {
        STORAGE_KEY: 'ids-theme',
    
        init() {
            const savedTheme = localStorage.getItem(this.STORAGE_KEY) || 'dark';
            this.apply(savedTheme);
        },
        
   apply(theme) {
            const root = document.documentElement;
const icon = document.getElementById('themeIcon');
          
    if (theme === 'dark') {
       root.classList.add('theme-dark');
   if (icon) {
          icon.classList.remove('fa-moon');
      icon.classList.add('fa-sun');
  }
          } else {
           root.classList.remove('theme-dark');
      if (icon) {
        icon.classList.remove('fa-sun');
        icon.classList.add('fa-moon');
        }
}
        },
        
   toggle() {
       const isDark = document.documentElement.classList.contains('theme-dark');
            const newTheme = isDark ? 'light' : 'dark';
          localStorage.setItem(this.STORAGE_KEY, newTheme);
            this.apply(newTheme);
      }
    };

    // ============================================
    // Utility Functions
    // ============================================
    const Utils = {
        /**
      * Escape HTML to prevent XSS
  */
        escapeHtml(text) {
if (!text) return '';
  const div = document.createElement('div');
       div.textContent = text;
            return div.innerHTML;
 },

    /**
    * Format number with locale-specific separators
    */
   formatNumber(num) {
            return num.toLocaleString();
        },

        /**
      * Format date/time
         */
        formatDateTime(dateString) {
            const date = new Date(dateString);
    return date.toLocaleString();
        },

        /**
     * Debounce function calls
         */
        debounce(func, wait) {
  let timeout;
            return function executedFunction(...args) {
       const later = () => {
            clearTimeout(timeout);
       func(...args);
        };
 clearTimeout(timeout);
      timeout = setTimeout(later, wait);
            };
        },

   /**
         * Show toast notification
         */
        showToast(message, type = 'info') {
  // Create toast container if it doesn't exist
   let container = document.getElementById('toast-container');
            if (!container) {
          container = document.createElement('div');
       container.id = 'toast-container';
      container.style.cssText = 'position: fixed; top: 1rem; right: 1rem; z-index: 9999; display: flex; flex-direction: column; gap: 0.5rem;';
  document.body.appendChild(container);
         }

            const toast = document.createElement('div');
            toast.className = `alert-ids alert-ids-${type}`;
  toast.style.cssText = 'min-width: 300px; animation: slideIn 0.3s ease;';
   toast.innerHTML = `
       <i class="alert-ids-icon fa-solid ${type === 'success' ? 'fa-check-circle' : type === 'danger' ? 'fa-exclamation-circle' : 'fa-info-circle'}"></i>
         <div class="alert-ids-content">${Utils.escapeHtml(message)}</div>
            `;
    
            container.appendChild(toast);
            
         // Remove after 5 seconds
      setTimeout(() => {
           toast.style.opacity = '0';
           toast.style.transform = 'translateX(100%)';
    toast.style.transition = 'all 0.3s ease';
    setTimeout(() => toast.remove(), 300);
            }, 5000);
      }
    };

    // ============================================
    // Count-up Animation
    // ============================================
    const CountUp = {
    animate(element, target, duration = 1000) {
     const start = performance.now();
            
      const step = (timestamp) => {
                const progress = Math.min((timestamp - start) / duration, 1);
              const value = Math.floor(progress * target);
         element.textContent = Utils.formatNumber(value);
      
                if (progress < 1) {
             requestAnimationFrame(step);
       }
       };
      
    requestAnimationFrame(step);
  },
  
        initAll() {
 document.querySelectorAll('.count-up[data-target]').forEach(el => {
          const target = parseInt(el.dataset.target) || 0;
         this.animate(el, target);
       });
        }
    };

    // ============================================
    // Form Validation Helpers
    // ============================================
    const FormValidation = {
        init() {
            // Add validation styling on form submit
      document.querySelectorAll('form').forEach(form => {
     form.addEventListener('submit', (e) => {
      if (!form.checkValidity()) {
           e.preventDefault();
         e.stopPropagation();
     }
        form.classList.add('was-validated');
       });
            });
        }
    };

    // ============================================
    // Keyboard Shortcuts
    // ============================================
    const KeyboardShortcuts = {
        init() {
 document.addEventListener('keydown', (e) => {
           // Don't trigger if user is typing in an input
           if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') {
   return;
}
           
      switch (e.key.toLowerCase()) {
      case 't':
              // Toggle theme
    ThemeManager.toggle();
             break;
          case 'd':
   // Go to dashboard
      if (e.ctrlKey || e.metaKey) return;
      window.location.href = '/Dashboard';
      break;
   case '/':
      // Focus search
      e.preventDefault();
             const searchInput = document.querySelector('input[type="search"], input[name="search"]');
        if (searchInput) {
       searchInput.focus();
             }
     break;
                }
     });
        }
    };

    // ============================================
    // Initialize on DOM Ready
    // ============================================
    document.addEventListener('DOMContentLoaded', () => {
        ThemeManager.init();
        FormValidation.init();
      KeyboardShortcuts.init();
        
   // Initialize count-up animations if present
        if (document.querySelector('.count-up')) {
     CountUp.initAll();
 }
    });

    // Export to global scope for use in pages
    window.IDS = {
     Utils,
        ThemeManager,
    CountUp
    };

})();
