class SessionManager {
    constructor() {
        this.init();
    }

    init() {
        // Chỉ thiết lập listener để nhận biết hoạt động của người dùng
        this.setupActivityListeners();
        console.info('SessionManager initialized (timeout checks disabled)');
    }

    setupActivityListeners() {
        // Khi có hoạt động của user, có thể dùng để reset các bộ đếm phía client nếu cần
        const events = ['click', 'keypress', 'scroll', 'mousemove', 'touchstart'];
        events.forEach(evt => {
            document.addEventListener(evt, this._onUserActivity.bind(this), true);
        });
    }

    _onUserActivity() {
       
    }

    destroy() {
        // nếu cần giải phóng resource
    }
}

// Khởi tạo session manager khi document ready
document.addEventListener('DOMContentLoaded', function() {
    // Khởi tạo SessionManager chỉ khi chưa có và khi trang đánh dấu là đã authenticated
    if (typeof window.sessionManager === 'undefined' && (document.querySelector('[data-authenticated="true"]') || document.body.classList.contains('authenticated'))) {
        window.sessionManager = new SessionManager();
    }
});

