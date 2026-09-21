(() => {
	'use strict';

	const state = { user: null, workflows: [], rows: [], stages: [], workflowId: null, dirty: false, searchTimer: null };
	const permissionTypes = [
		{ key: 'canView', label: 'Xem', short: 'X', className: 'view' },
		{ key: 'canProcess', label: 'Xử lý', short: 'XL', className: 'process' },
		{ key: 'canApprove', label: 'Duyệt', short: 'D', className: 'approve' },
		{ key: 'canReject', label: 'Từ chối', short: 'TC', className: 'reject' }
	];
	const elements = {
		userSearch: document.getElementById('userSearch'), suggestions: document.getElementById('userSuggestions'), searchUser: document.getElementById('btnSearchUser'),
		selectedName: document.getElementById('selectedUserName'), selectedMeta: document.getElementById('selectedUserMeta'), workflow: document.getElementById('workflowSelect'),
		refresh: document.getElementById('btnRefreshUserPermissions'), save: document.getElementById('btnSaveUserPermissions'), stepSearch: document.getElementById('stepSearch'),
		stage: document.getElementById('stageFilter'), loading: document.getElementById('userPermissionLoading'), error: document.getElementById('userPermissionError'),
		empty: document.getElementById('userPermissionEmpty'), table: document.getElementById('userPermissionTableWrap'), body: document.getElementById('userPermissionTableBody'),
		hint: document.getElementById('userPermissionHint'), count: document.getElementById('userPermissionCount'), changeState: document.getElementById('userPermissionChangeState'), alert: document.getElementById('userPermissionAlert')
	};

	const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
	const request = async (url, options = {}) => {
		const response = await fetch(url, { headers: { 'Content-Type': 'application/json' }, ...options });
		const payload = await response.json().catch(() => ({}));
		if (!response.ok || payload.success === false) throw new Error(payload.message || 'Không thể xử lý yêu cầu.');
		return payload;
	};
	const toggle = (element, visible) => element.classList.toggle('d-none', !visible);
	const showAlert = (message, type = 'success') => { elements.alert.className = `alert alert-${type}`; elements.alert.textContent = message; window.setTimeout(() => elements.alert.classList.add('d-none'), 3500); };
	const markDirty = dirty => { state.dirty = dirty; elements.save.disabled = !dirty; elements.changeState.textContent = dirty ? 'Có thay đổi chưa lưu' : 'Chưa có thay đổi'; elements.changeState.classList.toggle('is-dirty', dirty); };
	const emptyPermission = () => ({ canView: false, canProcess: false, canApprove: false, canReject: false });

	const renderSuggestions = users => {
		if (!users.length) { elements.suggestions.innerHTML = '<div class="suggestion-empty">Không tìm thấy user phù hợp.</div>'; toggle(elements.suggestions, true); return; }
		elements.suggestions.innerHTML = users.map(user => `<button type="button" class="user-suggestion" data-adid="${escapeHtml(user.adid)}"><span class="suggestion-avatar">${escapeHtml((user.name || user.adid).charAt(0).toUpperCase())}</span><span><strong>${escapeHtml(user.name || user.adid)}</strong><small>${escapeHtml(user.adid)}${user.department ? ` · ${escapeHtml(user.department)}` : ''}</small></span></button>`).join('');
		toggle(elements.suggestions, true);
	};
	const searchUsers = async () => {
		const query = elements.userSearch.value.trim();
		try { const payload = await request(`${window.searchWorkflowUsersUrl}?query=${encodeURIComponent(query)}`); renderSuggestions(payload.data || []); }
		catch (error) { showAlert(error.message, 'danger'); }
	};
	const renderUser = () => {
		elements.selectedName.textContent = state.user ? (state.user.name || state.user.adid) : 'Chưa chọn user';
		elements.selectedMeta.textContent = state.user ? `${state.user.adid}${state.user.department ? ` · ${state.user.department}` : ''}` : 'Tìm và chọn một user để bắt đầu';
	};
	const renderFilters = () => {
		elements.workflow.innerHTML = state.workflows.length ? state.workflows.map(item => `<option value="${item.id}">${escapeHtml(item.code)} · ${escapeHtml(item.name)}</option>`).join('') : '<option value="">Không có workflow</option>';
		elements.workflow.value = state.workflowId || '';
		const stages = [...new Map(state.rows.map(row => [`${row.stageCode}|${row.stageName}`, { code: row.stageCode, name: row.stageName }])).values()];
		elements.stage.innerHTML = '<option value="">Tất cả bước lớn</option>' + stages.map(stage => `<option value="${escapeHtml(stage.code)}">${escapeHtml(stage.code)} · ${escapeHtml(stage.name)}</option>`).join('');
	};
	const renderTable = () => {
		const query = elements.stepSearch.value.trim().toLowerCase();
		const rows = state.rows.filter(row => (!elements.stage.value || row.stageCode === elements.stage.value) && (!query || `${row.code} ${row.name} ${row.description || ''} ${row.stageCode} ${row.stageName}`.toLowerCase().includes(query)));
		const workflow = state.workflows.find(item => item.id === state.workflowId);
		elements.hint.textContent = workflow ? `${state.user.adid} · ${workflow.code} · ${workflow.name} · ${state.rows.length} bước` : (state.user ? `${state.user.adid} · Chưa có workflow` : 'Chưa chọn user');
		elements.count.textContent = `${state.rows.reduce((total, row) => total + permissionTypes.filter(type => row.permissions[type.key]).length, 0)} quyền`;
		if (!state.user || !rows.length) { toggle(elements.table, false); toggle(elements.empty, true); elements.empty.querySelector('span').textContent = state.user ? 'Không tìm thấy bước phù hợp.' : 'Chọn user để xem quyền riêng.'; return; }
		toggle(elements.empty, false); toggle(elements.table, true);
		elements.body.innerHTML = rows.map(row => `<tr><td class="user-step"><span class="step-order">${row.order}</span><div><strong>${escapeHtml(row.name)}</strong><small>${escapeHtml(row.code)}${row.description ? ` · ${escapeHtml(row.description)}` : ''}</small></div></td><td><span class="stage-label">${escapeHtml(row.stageCode)}</span><small class="d-block text-muted">${escapeHtml(row.stageName)}</small></td>${permissionTypes.map(type => `<td class="permission-cell"><label class="permission-check ${type.className}" title="${type.label}"><input type="checkbox" data-step="${row.id}" data-permission="${type.key}" ${row.permissions[type.key] ? 'checked' : ''}><span>${type.label}</span></label></td>`).join('')}</tr>`).join('');
	};
	const loadPermissions = async workflowId => {
		if (!state.user) return;
		toggle(elements.loading, true); toggle(elements.error, false); toggle(elements.table, false); toggle(elements.empty, false);
		try {
			const payload = await request(`${window.workflowUserPermissionsUrl}?userADID=${encodeURIComponent(state.user.adid)}&workflowId=${encodeURIComponent(workflowId || '')}`);
			state.workflows = payload.workflows || []; state.workflowId = payload.workflowId; state.rows = (payload.data || []).map(row => ({ ...row, permissions: { ...emptyPermission(), ...(row.permissions || {}) } }));
			renderFilters(); renderTable(); markDirty(false); elements.workflow.disabled = !state.workflows.length; elements.refresh.disabled = false;
		} catch (error) { elements.error.textContent = error.message; toggle(elements.error, true); toggle(elements.empty, false); }
		finally { toggle(elements.loading, false); }
	};
	const selectUser = async user => { state.user = user; elements.userSearch.value = user.adid; toggle(elements.suggestions, false); renderUser(); await loadPermissions(); };
	const save = async () => {
		if (!state.user || !state.workflowId) return;
		elements.save.disabled = true; elements.save.querySelector('span').textContent = 'Đang lưu...';
		try { const payload = await request(window.saveWorkflowUserPermissionsUrl, { method: 'POST', body: JSON.stringify({ workflowId: state.workflowId, userADID: state.user.adid, rows: state.rows.map(row => ({ workflowStepId: row.id, ...row.permissions })) }) }); showAlert(payload.message || 'Đã lưu quyền riêng theo user.'); markDirty(false); }
		catch (error) { showAlert(error.message, 'danger'); markDirty(true); }
		finally { elements.save.querySelector('span').textContent = 'Lưu thay đổi'; elements.save.disabled = !state.dirty; }
	};

	elements.userSearch.addEventListener('input', () => { window.clearTimeout(state.searchTimer); state.searchTimer = window.setTimeout(searchUsers, 250); });
	elements.searchUser.addEventListener('click', searchUsers);
	elements.suggestions.addEventListener('click', event => { const button = event.target.closest('[data-adid]'); if (!button) return; const user = { adid: button.dataset.adid, name: button.querySelector('strong').textContent, department: button.querySelector('small').textContent.split(' · ')[1] || '' }; selectUser(user); });
	elements.workflow.addEventListener('change', () => loadPermissions(elements.workflow.value));
	elements.refresh.addEventListener('click', () => loadPermissions(state.workflowId));
	elements.stepSearch.addEventListener('input', renderTable); elements.stage.addEventListener('change', renderTable); elements.save.addEventListener('click', save);
	elements.body.addEventListener('change', event => { const input = event.target.closest('input[data-step]'); if (!input) return; const row = state.rows.find(item => item.id === Number(input.dataset.step)); if (!row) return; row.permissions[input.dataset.permission] = input.checked; markDirty(true); renderTable(); });
	document.addEventListener('click', event => { if (!event.target.closest('.user-picker')) toggle(elements.suggestions, false); });
	renderUser();
})();
