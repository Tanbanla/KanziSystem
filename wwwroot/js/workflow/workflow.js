(() => {
    (function () {
        const storageKey = 'cost-management-quotation-workflow';
        const typeNames = {
            goods: 'Hàng hóa',
            service: 'Dịch vụ',
            design: 'Hàng hóa thiết kế',
            construction: 'Cải tạo / công trình'
        };
        const typeIcons = {
            goods: 'fa-box',
            service: 'fa-concierge-bell',
            design: 'fa-pencil-ruler',
            construction: 'fa-hard-hat'
        };
        const defaultSteps = ['Tiếp nhận yêu cầu', 'Kiểm tra thông tin', 'Xác nhận tên hàng', 'Xin báo giá nhà cung cấp', 'So sánh báo giá', 'Phê duyệt báo giá', 'Hoàn tất yêu cầu'];
        let steps = [],
            flow = 'GA',
            type = 'goods',
            config = {};

        const key = () => `${flow}_${type}`;
        const ensureConfig = () => {
            if (!config[key()]) config[key()] = steps.map((_, i) => i < steps.length - 1);
        };
        const updateStatus = () => {
            const selected = (config[key()] || []).filter(Boolean).length;
            document.getElementById('statusFlow').textContent = `${flow} · ${typeNames[type]}`;
            document.getElementById('currentFlowLabel').textContent = flow;
            document.getElementById('currentTypeLabel').textContent = typeNames[type];
            document.getElementById('selectedCount').textContent = `${selected} bước được chọn`;
            document.getElementById('totalCount').textContent = `${steps.length} bước`;
            document.getElementById('progressBar').style.width = steps.length ? `${selected / steps.length * 100}%` : '0%';
        };
        const renderSteps = () => {
            ensureConfig();
            document.getElementById('stepList').innerHTML = steps.length ? steps.map((step, i) => `<div class="step-row ${config[key()][i] ? '' : 'disabled'}"><span class="step-number">${i + 1}</span><div class="flex-grow-1"><strong>${step.name}</strong>${step.description ? `<div class="small text-muted">${step.description}</div>` : ''}</div><div class="form-check form-switch mb-0"><input class="form-check-input step-toggle" type="checkbox" data-index="${i}" ${config[key()][i] ? 'checked' : ''} aria-label="Bật bước ${i + 1}"></div></div>`).join('') : '<div class="empty-state">Chưa có bước xử lý trong hệ thống.</div>';
            document.getElementById('loadStatus').className = 'badge bg-success';
            document.getElementById('loadStatus').innerHTML = '<i class="fas fa-check me-1"></i> Đã tải';
            updateStatus();
        };

        document.addEventListener('click', event => {
            const flowButton = event.target.closest('[data-flow]');
            if (flowButton) {
                flow = flowButton.dataset.flow;
                document.querySelectorAll('[data-flow]').forEach(item => item.classList.toggle('active', item === flowButton));
                renderSteps();
            }
            const typeCard = event.target.closest('[data-type]');
            if (typeCard) {
                type = typeCard.dataset.type;
                document.querySelectorAll('[data-type]').forEach(item => item.classList.toggle('active', item === typeCard));
                renderSteps();
            }
        });
        document.addEventListener('change', event => {
            if (event.target.classList.contains('step-toggle')) {
                config[key()][Number(event.target.dataset.index)] = event.target.checked;
                renderSteps();
            }
        });
        document.getElementById('btnSave')?.addEventListener('click', () => {
            localStorage.setItem(storageKey, JSON.stringify(config));
            const button = document.getElementById('btnSave');
            button.innerHTML = '<i class="fas fa-check me-1"></i> Đã lưu';
            setTimeout(() => button.innerHTML = '<i class="fas fa-save me-1"></i> Lưu bản nháp', 1600);
        });
        document.getElementById('btnReset')?.addEventListener('click', () => {
            config = {};
            localStorage.removeItem(storageKey);
            renderSteps();
        });
    })();
}
//    fetch(window.workflowStepsUrl).then(response => response.ok ? response.json() : Promise.reject()).then(response => {
//        steps = (response.data || []).map(item => ({
//            name: item.name,
//            description: item.nameEn || ''
//        }));
//        if (!steps.length) throw new Error();
//        config = JSON.parse(localStorage.getItem(storageKey) || '{}');
//        renderSteps();
//    }).catch(() => {
//        steps = defaultSteps.map(name => ({
//            name
//        }));
//        config = JSON.parse(localStorage.getItem(storageKey) || '{}');
//        renderSteps();
//        document.getElementById('loadStatus').className = 'badge bg-warning text-dark';
//        document.getElementById('loadStatus').innerHTML = '<i class="fas fa-info-circle me-1"></i> Dữ liệu mặc định';
//    });
//})();
