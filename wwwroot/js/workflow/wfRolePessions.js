(() => {
    'use strict';

    const state = { workflows: [], roles: [], rows: [], workflowId: null, dirty: false };
    const permissionTypes = [
        { key: 'canView', label: 'Xem', short: 'X', className: 'view' },
        { key: 'canProcess', label: 'Xử lý', short: 'XL', className: 'process' },
        { key: 'canApprove', label: 'Duyệt', short: 'D', className: 'approve' },
        { key: 'canReject', label: 'Từ chối', short: 'TC', className: 'reject' }
    ];

    const elements = {
        workflow: document.getElementById('workflowSelect'), role: document.getElementById('roleFilter'),
        search: document.getElementById('permissionSearch'), save: document.getElementById('btnSavePermissions'),
        reset: document.getElementById('btnResetPermissions'), head: document.getElementById('permissionTableHead'),
        body: document.getElementById('permissionTableBody'), table: document.getElementById('permissionTableWrap'),
        loading: document.getElementById('permissionLoading'), error: document.getElementById('permissionError'),
        empty: document.getElementById('permissionEmpty'), count: document.getElementById('permissionCount'),
        hint: document.getElementById('permissionWorkflowHint'), changeState: document.getElementById('permissionChangeState'),
        alert: document.getElementById('permissionAlert')
    };

    const jsonRequest = async (url, options) => {
        const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
        const payload = await response.json().catch(() => ({}));
        if (!response.ok || payload.success === false) throw new Error(payload.message || 'Không thể xử lý yêu cầu.');
        return payload;
    };

    const normalizeRows = rows => (rows || []).map(row => {
        const permissions = {};
        (row.permissions || []).forEach(item => permissions[item.roleCode] = {
            canView: !!item.canView, canProcess: !!item.canProcess, canApprove: !!item.canApprove, canReject: !!item.canReject
        });
        return { ...row, permissions };
    });

    const setState = (name, visible) => document.getElementById(name).classList.toggle('d-none', !visible);
    const showAlert = (message, type = 'success') => {
        elements.alert.className = `alert alert-${type}`;
        elements.alert.textContent = message;
        window.setTimeout(() => elements.alert.classList.add('d-none'), 3500);
    };
    const markDirty = dirty => {
        state.dirty = dirty;
        elements.save.disabled = !dirty;
        elements.changeState.textContent = dirty ? 'Có thay đổi chưa lưu' : 'Chưa có thay đổi';
        elements.changeState.classList.toggle('is-dirty', dirty);
    };

    const renderFilters = () => {
        elements.workflow.innerHTML = state.workflows.length
            ? state.workflows.map(item => `<option value="${item.id}">${item.code} · ${item.name}</option>`).join('')
            : '<option value="">Không có workflow</option>';
        elements.workflow.value = state.workflowId || '';
        elements.role.innerHTML = '<option value="">Tất cả role</option>' + state.roles.map(role => `<option value="${role.code}">${role.code} · ${role.name}</option>`).join('');
    };

    const renderTable = () => {
        const query = elements.search.value.trim().toLowerCase();
        const roles = state.roles.filter(role => !elements.role.value || role.code === elements.role.value);
        const rows = state.rows.filter(row => !query || `${row.code} ${row.name} ${row.description || ''}`.toLowerCase().includes(query));
        const selectedWorkflow = state.workflows.find(item => item.id === state.workflowId);
        elements.hint.textContent = selectedWorkflow ? `${selectedWorkflow.code} · ${selectedWorkflow.name} · ${state.rows.length} bước` : 'Chưa chọn workflow';
        const totalPermissions = state.rows.reduce((total, row) => total + Object.values(row.permissions).reduce((value, permission) => value + permissionTypes.filter(type => permission[type.key]).length, 0), 0);
        elements.count.textContent = `${totalPermissions} quyền`;

        if (!rows.length || !roles.length) {
            setState('permissionTableWrap', false); setState('permissionEmpty', true);
            elements.empty.querySelector('span').textContent = !roles.length ? 'Chưa có role hoạt động để phân quyền.' : 'Không tìm thấy bước phù hợp.';
            return;
        }
        setState('permissionEmpty', false); setState('permissionTableWrap', true);
        elements.head.innerHTML = `<tr><th class="permission-step-heading">Bước xử lý</th>${roles.map(role => `<th class="permission-role-heading"><strong>${role.code}</strong><small>${role.name}</small></th>`).join('')}</tr>`;
        elements.body.innerHTML = rows.map(row => `<tr><td class="permission-step"><span class="step-order">${row.order}</span><div><strong>${row.name}</strong><small>${row.code}${row.description ? ` · ${row.description}` : ''}</small></div></td>${roles.map(role => {
            const permission = row.permissions[role.code] || { canView: false, canProcess: false, canApprove: false, canReject: false };
            row.permissions[role.code] = permission;
            return `<td class="permission-role-cell"><div class="permission-checks">${permissionTypes.map(type => `<label class="permission-check ${type.className}" title="${type.label}"><input type="checkbox" data-step="${row.id}" data-role="${role.code}" data-permission="${type.key}" ${permission[type.key] ? 'checked' : ''}><span>${type.short}</span></label>`).join('')}</div></td>`;
        }).join('')}</tr>`).join('');
    };

    const load = async workflowId => {
        setState('permissionLoading', true); setState('permissionError', false); setState('permissionTableWrap', false); setState('permissionEmpty', false);
        try {
            const suffix = workflowId ? `?workflowId=${encodeURIComponent(workflowId)}` : '';
            const payload = await jsonRequest(`${window.workflowRolePermissionsUrl}${suffix}`);
            state.workflows = payload.workflows || []; state.roles = payload.roles || []; state.rows = normalizeRows(payload.data); state.workflowId = payload.workflowId;
            renderFilters(); renderTable(); markDirty(false);
        } catch (error) { elements.error.textContent = error.message; setState('permissionError', true); }
        finally { setState('permissionLoading', false); }
    };

    const save = async () => {
        if (!state.workflowId) return;
        elements.save.disabled = true; elements.save.querySelector('span').textContent = 'Đang lưu...';
        const rows = state.rows.flatMap(row => state.roles.map(role => {
            const permission = row.permissions[role.code] || {};
            return { workflowStepId: row.id, roleCode: role.code, canView: !!permission.canView, canProcess: !!permission.canProcess, canApprove: !!permission.canApprove, canReject: !!permission.canReject };
        }));
        try {
            const payload = await jsonRequest(window.saveWorkflowRolePermissionsUrl, { method: 'POST', body: JSON.stringify({ workflowId: state.workflowId, rows }) });
            showAlert(payload.message || 'Đã lưu ma trận phân quyền.'); markDirty(false);
        } catch (error) { showAlert(error.message, 'danger'); markDirty(true); }
        finally { elements.save.querySelector('span').textContent = 'Lưu thay đổi'; elements.save.disabled = !state.dirty; }
    };

    elements.workflow.addEventListener('change', () => load(elements.workflow.value));
    elements.role.addEventListener('change', renderTable); elements.search.addEventListener('input', renderTable);
    elements.reset.addEventListener('click', () => load(state.workflowId)); elements.save.addEventListener('click', save);
    elements.body.addEventListener('change', event => {
        const input = event.target.closest('input[data-step]'); if (!input) return;
        const row = state.rows.find(item => item.id === Number(input.dataset.step)); if (!row) return;
        row.permissions[input.dataset.role] = row.permissions[input.dataset.role] || {};
        row.permissions[input.dataset.role][input.dataset.permission] = input.checked; markDirty(true); renderTable();
    });
    load();
})();
