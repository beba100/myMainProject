document.addEventListener('DOMContentLoaded', function() {
    const popup = document.getElementById('global-popup');
    const content = document.getElementById('popup-body');
    const timerDisplay = document.getElementById('popup-timer');
    const closeBtn = popup.querySelector('.popup-close');
    let timerId = null;

    function hidePopup() {
        if (timerId) clearInterval(timerId);
        popup.classList.remove('active');
        setTimeout(() => (popup.style.display = 'none'), 400);
    }

    closeBtn.addEventListener('click', hidePopup);
    popup.addEventListener('click', (e) => {
        if (e.target === popup) hidePopup();
    });
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && popup.style.display !== 'none') hidePopup();
    });

    function showPopup(message, options = {}) {
        if (timerId) clearInterval(timerId);
        content.innerHTML = `
            <div class="popup-icon">${options.icon || '🎉'}</div>
            <p class="popup-message">${message}</p>
        `;
        popup.style.display = 'flex';
        requestAnimationFrame(() => popup.classList.add('active'));

        const countdown = options.countdown || 5;
        if (countdown > 0) {
            let timeLeft = countdown;
            timerDisplay.textContent = `Closes in ${timeLeft}s`;
            timerId = setInterval(() => {
                timeLeft--;
                if (timeLeft > 0) {
                    timerDisplay.textContent = `Closes in ${timeLeft}s`;
                } else {
                    hidePopup();
                }
            }, 1000);
        } else {
            timerDisplay.textContent = '';
        }

        const popupContent = popup.querySelector('.popup-content');
        popupContent.style.background = options.bg || 'var(--default-bg)';
        popupContent.style.color = options.text || 'var(--default-text)';
    }

    window.showSuccess = (message, countdown) => {
        showPopup(message, {
            bg: 'var(--success-bg)',
            text: 'var(--success-text)',
            icon: '✅',
            countdown
        });
    };

    window.showError = (message, countdown) => {
        showPopup(message, {
            bg: 'var(--error-bg)',
            text: 'var(--error-text)',
            icon: '❌',
            countdown
        });
    };

    window.showInfo = (message, countdown) => {
        showPopup(message, {
            bg: 'var(--info-bg)',
            text: 'var(--info-text)',
            icon: 'ℹ️',
            countdown
        });
    };

    window.showWarning = (message, countdown) => {
        showPopup(message, {
            bg: 'var(--warning-bg, #fff3cd)', // Default to a warning color if not defined
            text: 'var(--warning-text, #856404)',
            icon: '⚠',
            countdown
        });
    };

    window.showPopup = (message, countdown) => {
        showPopup(message, { countdown });
    };
});