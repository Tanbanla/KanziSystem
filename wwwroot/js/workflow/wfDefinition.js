(() => {
    const state = { workflows: [], requestTypes: [], modal: null };
    const $ = selector => document.querySelector(selector);

    document.addEventListener('DOMContentLoaded', async () => {
        state.modal = $('#workflowModal');
        $('#btnAddWorkflow').addEventListener('click', () => openForm());
        $('#workflowForm').addEventListener('submit', saveWorkflow);
        $('#workflowSearch').addEventListener('input', renderWorkflows);
        $('#workflowRequestType').addEventListener('change', renderWorkflows);
        $('#workflowStatus').addEventListener('change', renderWorkflows);
        $('#btnSearchWorkflow').addEventListener('click', renderWorkflows);
        await loadData();
    });

    async function callApi(url, options = {}) {
        const response = await fetch((window.apiBaseUrl || '') + url, options);
        const text = await response.text();
        let result = null;
        try { result = text ? JSON.parse(text) : null; } catch { }
        if (!response.ok || !result?.success) throw new Error(result?.message || text || 'Không thể kết nối đến máy chủ.');
        return result;
    }

    async function loadData() {
        try {
            const [workflows, requestTypes] = await Promise.all([callApi(window.workflowDefinitionsUrl), callApi(window.workflowRequestTypesUrl)]);
            state.workflows = workflows.data || [];
            state.requestTypes = requestTypes.data || [];
            fillRequestTypes();
            renderWorkflows();
        } catch (error) { showAlert(error.message || 'Không thể tải danh sách workflow.', true); }
    }

    function fillRequestTypes() {
        const options = state.requestTypes.map(type => `<option value="${type.id}">${escapeHtml(type.code)} - ${escapeHtml(type.name)}</option>`).join('');
        $('#workflowRequestType').insertAdjacentHTML('beforeend', options);
        $('#workflowRequestTypeId').insertAdjacentHTML('beforeend', options);
    }

    function renderWorkflows() {
        const keyword = $('#workflowSearch').value.trim().toLowerCase();
        const typeId = $('#workflowRequestType').value;
        const status = $('#workflowStatus').value;
        const filtered = state.workflows.filter(workflow => {
            const text = `${workflow.flowCode} ${workflow.workflowName} ${workflow.requestTypeCode} ${workflow.requestTypeName}`.toLowerCase();
            return (!keyword || text.includes(keyword)) && (!typeId || String(workflow.requestTypeId) === typeId) && (status === '' || String(workflow.isActive).toLowerCase() === status);
        });
        $('#workflowCount').textContent = `${filtered.length} bản ghi`;
        $('#workflowTableBody').innerHTML = filtered.length ? filtered.map(renderRow).join('') : '<tr><td colspan="6" class="empty-state"><i class="fas fa-inbox d-block mb-2"></i>Không tìm thấy workflow phù hợp.</td></tr>';
        $('#workflowTableBody').querySelectorAll('[data-action]').forEach(button => button.addEventListener('click', () => {
            const workflow = state.workflows.find(item => item.id === Number(button.dataset.id));
            button.dataset.action === 'edit' ? openForm(workflow) : deleteWorkflow(workflow);
        }));
    }

    function renderRow(workflow) {
        return `<tr><td><span class="type-code">${escapeHtml(workflow.requestTypeCode)}</span><div class="small text-muted">${escapeHtml(workflow.requestTypeName)}</div></td><td><code>${escapeHtml(workflow.flowCode)}</code></td><td><strong>${escapeHtml(workflow.workflowName)}</strong></td><td class="text-center">${workflow.stepCount ?? 0}</td><td class="text-center"><span class="status-badge ${workflow.isActive ? 'status-active' : 'status-inactive'}"><i class="fas ${workflow.isActive ? 'fa-check-circle' : 'fa-pause-circle'} me-1"></i>${workflow.isActive ? 'Hoạt động' : 'Không hoạt động'}</span></td><td class="text-center"><div class="action-group"><button class="btn-icon btn-edit" type="button" data-action="edit" data-id="${workflow.id}" title="Sửa workflow" aria-label="Sửa workflow"><i class="fas fa-pen"></i></button><button class="btn-icon btn-delete" type="button" data-action="delete" data-id="${workflow.id}" title="Ngừng hoạt động" aria-label="Ngừng hoạt động"><i class="fas fa-ban"></i></button></div></td></tr>`;
    }

    function openForm(workflow = null) {
        $('#workflowForm').reset();
        $('#workflowForm').classList.remove('was-validated');
        $('#workflowId').value = workflow?.id || '';
        $('#workflowRequestTypeId').value = workflow?.requestTypeId || '';
        $('#workflowFlowCode').value = workflow?.flowCode || '';
        $('#workflowName').value = workflow?.workflowName || '';
        $('#workflowIsActive').checked = workflow?.isActive ?? true;
        $('#workflowModalTitle').textContent = workflow ? 'Sửa workflow' : 'Thêm workflow';
        window.jQuery(state.modal).modal('show');
    }

    async function saveWorkflow(event) {
        event.preventDefault();
        if (!$('#workflowForm').checkValidity()) { $('#workflowForm').classList.add('was-validated'); return; }
        const id = $('#workflowId').value;
        const payload = { requestTypeID: Number($('#workflowRequestTypeId').value), flowCode: $('#workflowFlowCode').value.trim(), workflowName: $('#workflowName').value.trim(), isActive: $('#workflowIsActive').checked };
        const button = $('#btnSaveWorkflow'); button.disabled = true;
        try {
            await callApi(id ? `/Workflow/UpdateWorkflowDefinition?id=${id}` : '/Workflow/CreateWorkflowDefinition', { method: id ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            window.jQuery(state.modal).modal('hide'); showAlert(id ? 'Đã cập nhật workflow.' : 'Đã thêm workflow.'); await reloadData();
        } catch (error) { showAlert(error.message || 'Không thể lưu workflow.', true); } finally { button.disabled = false; }
    }

    async function deleteWorkflow(workflow) {
        if (!workflow || !confirm(`Bạn có chắc muốn ngừng hoạt động workflow "${workflow.workflowName}"?`)) return;
        try { await callApi(`/Workflow/DeleteWorkflowDefinition?id=${workflow.id}`, { method: 'DELETE' }); showAlert('Đã ngừng hoạt động workflow.'); await reloadData(); }
        catch (error) { showAlert(error.message || 'Không thể cập nhật workflow.', true); }
    }

    async function reloadData() {
        const result = await callApi(window.workflowDefinitionsUrl);
        state.workflows = result.data || [];
        renderWorkflows();
    }

    function showAlert(message, isError = false) {
        const alert = $('#workflowAlert'); alert.textContent = message; alert.className = `alert position-fixed bottom-0 end-0 m-3 ${isError ? 'alert-danger' : 'alert-success'}`; setTimeout(() => alert.classList.add('d-none'), 3000);
    }

    function escapeHtml(value) { return String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character])); }
})();
