(() => {
    'use strict';

    class QuoteNotification {
        constructor(container = '#quoteNotifications', timeout = 8000) {
            this.container = typeof container === 'string'
                ? document.querySelector(container)
                : container;
            this.timeout = timeout;
        }

        show(message, type = 'danger') {
            if (!this.container) return null;

            const notification = document.createElement('div');
            notification.className = `quote-notification quote-notification-${type}`;
            notification.setAttribute('role', 'alert');

            const messageContent = document.createElement('span');
            messageContent.textContent = message;
            notification.appendChild(messageContent);

            const closeButton = document.createElement('button');
            closeButton.type = 'button';
            closeButton.className = 'quote-notification-close';
            closeButton.setAttribute('aria-label', 'Đóng thông báo');
            closeButton.innerHTML = '&times;';
            closeButton.addEventListener('click', () => notification.remove());
            notification.appendChild(closeButton);

            this.container.appendChild(notification);
            window.setTimeout(() => notification.remove(), this.timeout);

            return notification;
        }
    }

    window.QuoteNotification = QuoteNotification;
})();
