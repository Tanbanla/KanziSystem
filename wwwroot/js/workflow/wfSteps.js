(() => {
    const state = { steps: [], stages: [], modal: null };
    const $ = selector => document.querySelector(selector);

    document.addEventListener('DOMContentLoaded', () => {
        state.modal = $('#stepModal');
        $('#btnAddStep').addEventListener('click', () => openForm());
        $('#stepForm').addEventListener('submit', saveStep);
        $('#stepSearch').addEventListener('input', renderSteps);
        $('#stepStage').addEventListener('change', renderSteps);
        $('#stepStageId').addEventListener('change', () => {
            if (!$('#stepId').value) $('#stepOrder').value = getNextStepOrder(Number($('#stepStageId').value));
        });
        $('#stepStatus').addEventListener('change', renderSteps);
        $('#btnSearchStep').addEventListener('click', renderSteps);
        loadData();
    });

    async function loadData() {
        try {
            const [steps, stages] = await Promise.all([
                callApi(window.workflowStepCatalogUrl),
                callApi(window.workflowStagesUrl)
            ]);

            // Chỉ lấy các stage đang hoạt động
            state.stages = (stages.data || []).filter(stage => stage.isActive === true);

            // Danh sách ID stage active
            const activeStageIds = new Set(
                state.stages.map(stage => stage.id)
            );

            // Chỉ lấy step thuộc stage active
            state.steps = (steps.data || []).filter(
                step => activeStageIds.has(step.stageId)
            );

            fillStageSelects();
            renderSteps();
        } catch (error) {
            showAlert(error.message || 'Không thể tải dữ liệu bước nhỏ.', true);
        }
    }

    async function callApi(url, options = {}) {
        const response = await fetch((window.apiBaseUrl || '') + url, options);
        const text = await response.text();
        let result;
        try { result = text ? JSON.parse(text) : null; } catch { result = null; }
        if (!response.ok || !result?.success) throw new Error(result?.message || text || 'Không thể kết nối đến máy chủ.');
        return result;
    }

    function fillStageSelects() {
        const options = state.stages.map(stage => `<option value="${stage.id}">${escapeHtml(stage.code)} - ${escapeHtml(stage.name)}</option>`).join('');
        $('#stepStage').querySelectorAll('option:not(:first-child)').forEach(option => option.remove());
        $('#stepStageId').querySelectorAll('option:not(:first-child)').forEach(option => option.remove());
        $('#stepStage').insertAdjacentHTML('beforeend', options);
        $('#stepStageId').insertAdjacentHTML('beforeend', options);
    }

    function renderSteps() {
        const keyword = $('#stepSearch').value.trim().toLowerCase();
        const stageId = $('#stepStage').value;
        const status = $('#stepStatus').value;
        const filtered = state.steps.filter(step => {
            const stage = state.stages.find(item => item.id === step.stageId);
            const text = `${step.code} ${step.name} ${step.nameEn || ''} ${stage?.name || ''}`.toLowerCase();
            return (!keyword || text.includes(keyword)) && (!stageId || String(step.stageId) === stageId) && (status === '' || String(step.isActive).toLowerCase() === status);
        });
        $('#stepCount').textContent = `${filtered.length} bản ghi`;
        $('#stepTableBody').innerHTML = filtered.length ? filtered.map(renderRow).join('') : '<tr><td colspan="7" class="empty-state"><i class="fas fa-inbox d-block mb-2"></i>Không tìm thấy bước nhỏ phù hợp.</td></tr>';
        $('#stepTableBody').querySelectorAll('[data-action]').forEach(button => button.addEventListener('click', () => {
            const step = state.steps.find(item => item.id === Number(button.dataset.id));
            button.dataset.action === 'edit' ? openForm(step) : deleteStep(step);
        }));
    }

    function renderRow(step) {
        const stage = state.stages.find(item => item.id === step.stageId);
        const duration = step.durationHours == null ? '-' : `${step.durationHours} giờ`;
        return `<tr><td class="text-center"><span class="step-number">${step.stepOrder ?? '-'}</span></td><td class="text-center"><code>${escapeHtml(step.code)}</code></td><td class="text-center">${escapeHtml(stage?.name || 'Chưa phân loại')}</td><td><strong>${escapeHtml(step.name)}</strong>${step.nameEn ? `<div class="small text-muted">${escapeHtml(step.nameEn)}</div>` : ''}${step.description ? `<div class="small text-muted">${escapeHtml(step.description)}</div>` : ''}</td><td class="text-center">${duration}</td><td class="text-center"><span class="status-badge ${step.isActive ? 'status-active' : 'status-inactive'}"><i class="fas ${step.isActive ? 'fa-check-circle' : 'fa-pause-circle'} me-1"></i>${step.isActive ? 'Hoạt động' : 'Không hoạt động'}</span></td><td class="text-center"><div class="action-group"><button class="btn-icon btn-edit" type="button" data-action="edit" data-id="${step.id}" title="Sửa"><i class="fas fa-pen"></i></button><button class="btn-icon btn-delete" type="button" data-action="delete" data-id="${step.id}" title="Xóa"><i class="fas fa-trash"></i></button></div></td></tr>`;
    }

    function openForm(step = null) {
        $('#stepForm').reset();
        $('#stepForm').classList.remove('was-validated');
        $('#stepId').value = step?.id || '';
        $('#stepCode').value = step?.code || '';
        $('#stepName').value = step?.name || '';
        $('#stepNameEn').value = step?.nameEn || '';
        $('#stepDescription').value = step?.description || '';
        $('#stepOrder').value = step?.stepOrder ?? getNextStepOrder(step?.stageId);
        $('#stepDuration').value = step?.durationHours ?? '';
        $('#stepStageId').value = step?.stageId || '';
        $('#stepIsActive').checked = step?.isActive ?? true;
        $('#stepModalTitle').textContent = step ? 'Sửa bước nhỏ' : 'Thêm bước nhỏ';
        window.jQuery(state.modal).modal('show');
    }

    async function saveStep(event) {
        event.preventDefault();
        if (!$('#stepForm').checkValidity()) { $('#stepForm').classList.add('was-validated'); return; }
        const id = $('#stepId').value;
        const payload = { stageID: Number($('#stepStageId').value), stepOrder: Number($('#stepOrder').value), stepCode: $('#stepCode').value.trim(), stepName: $('#stepName').value.trim(), stepNameEN: $('#stepNameEn').value.trim() || null, description: $('#stepDescription').value.trim() || null, defaultDurationHours: $('#stepDuration').value === '' ? null : Number($('#stepDuration').value), isActive: $('#stepIsActive').checked };
        const button = $('#btnSaveStep'); button.disabled = true;
        try {
            await callApi(id ? `/Workflow/UpdateWorkflowStep?id=${id}` : '/Workflow/CreateWorkflowStep', { method: id ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            window.jQuery(state.modal).modal('hide'); showAlert(id ? 'Đã cập nhật bước nhỏ.' : 'Đã thêm bước nhỏ.'); await loadData();
        } catch (error) { showAlert(error.message || 'Không thể lưu dữ liệu.', true); } finally { button.disabled = false; }
    }

    function getNextStepOrder(stageId) {
        const stageSteps = state.steps.filter(step => step.stageId === stageId);
        return stageSteps.reduce((max, step) => Math.max(max, Number(step.stepOrder) || 0), 0) + 1;
    }

    async function deleteStep(step) {
        if (!step || !confirm(`Bạn có chắc muốn xóa bước "${step.name}"?`)) return;
        try { await callApi(`/Workflow/DeleteWorkflowStep?id=${step.id}`, { method: 'DELETE' }); showAlert('Đã xóa bước nhỏ.'); await loadData(); }
        catch (error) { showAlert(error.message || 'Không thể xóa dữ liệu.', true); }
    }

    function showAlert(message, isError = false) {
        const alert = $('#stepAlert'); alert.textContent = message; alert.className = `alert position-fixed bottom-0 end-0 m-3 ${isError ? 'alert-danger' : 'alert-success'}`; setTimeout(() => alert.classList.add('d-none'), 3000);
    }

    function escapeHtml(value) { return String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character])); }
})();
