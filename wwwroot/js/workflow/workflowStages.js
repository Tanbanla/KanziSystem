(() => {
    const state = { stages: [], modal: null };
    const $ = selector => document.querySelector(selector);

    document.addEventListener('DOMContentLoaded', () => {
        state.modal = $('#stageModal');
        $('#btnAddStage').addEventListener('click', () => openForm());
        $('#stageForm').addEventListener('submit', saveStage);
        $('#btnSearchStage').addEventListener('click', renderStages);
        $('#stageSearch').addEventListener('input', renderStages);
        $('#stageStatus').addEventListener('change', renderStages);
        loadStages();
    });

    async function loadStages() {
        try {
            const result = await callApi('/Workflow/GetWorkflowStages');
            state.stages = result.data || [];
            renderStages();
        } catch (error) { showAlert(error.message || 'Không thể tải danh sách bước lớn.', true); }
    }

    async function callApi(path, options = {}) {
        const response = await fetch((window.apiBaseUrl || '') + path, options);
        const responseText = await response.text().catch(() => '');
        let result = null;
        try { result = responseText ? JSON.parse(responseText) : null; } catch { }

        if (!response.ok) {
            throw new Error(result?.message || responseText || 'Không thể kết nối đến máy chủ.');
        }
        if (!result?.success) {
            throw new Error(result?.message || 'API trả về kết quả không hợp lệ.');
        }
        return result;
    }

    function renderStages() {
        const keyword = $('#stageSearch').value.trim().toLowerCase();
        const status = $('#stageStatus').value;
        const stages = state.stages.filter(stage =>
            (!keyword || `${stage.code} ${stage.name}`.toLowerCase().includes(keyword)) &&
            (status === '' || String(stage.isActive).toLowerCase() === status));
        $('#stageCount').textContent = `${stages.length} bản ghi`;
        $('#stageTableBody').innerHTML = stages.length ? stages.map(stage => `
            <tr>
                <td class="text-center"><span class="step-number">${stage.order}</span></td>
                <td class="text-center"><code>${escapeHtml(stage.code)}</code></td>
                <td><strong>${escapeHtml(stage.name)}</strong></td>
                <td class="text-center">${stage.stepCount ?? 0}</td>
                <td class="text-center"><span class="status-badge ${stage.isActive ? 'status-active' : 'status-inactive'}"><i class="fas ${stage.isActive ? 'fa-check-circle' : 'fa-pause-circle'} me-1"></i>${stage.isActive ? 'Hoạt động' : 'Không hoạt động'}</span></td>
                <td class="text-center"><div class="action-group"><button class="btn-icon btn-edit" type="button" data-action="edit" data-id="${stage.id}" title="Sửa ${escapeHtml(stage.name)}" aria-label="Sửa ${escapeHtml(stage.name)}"><i class="fas fa-pen"></i></button><button class="btn-icon btn-delete" type="button" data-action="delete" data-id="${stage.id}" title="Xóa ${escapeHtml(stage.name)}" aria-label="Xóa ${escapeHtml(stage.name)}"><i class="fas fa-trash"></i></button></div></td>
            </tr>`).join('') : '<tr><td colspan="6" class="empty-state"><i class="fas fa-inbox d-block mb-2"></i>Không tìm thấy bước lớn phù hợp.</td></tr>';
        $('#stageTableBody').querySelectorAll('button').forEach(button => {
            const stage = state.stages.find(item => item.id === Number(button.dataset.id));
            button.addEventListener('click', () => button.dataset.action === 'edit' ? openForm(stage) : deleteStage(stage));
        });
    }

    function openForm(stage = null) {
        $('#stageForm').reset();
        $('#stageId').value = stage?.id || '';
        $('#stageCode').value = stage?.code || '';
        $('#stageName').value = stage?.name || '';
        $('#stageOrder').value = stage?.order || state.stages.length + 1;
        $('#stageIsActive').checked = stage?.isActive ?? true;
        $('#stageModalTitle').textContent = stage ? 'Sửa bước lớn' : 'Thêm bước lớn';
        window.jQuery(state.modal).modal('show');
    }

    async function saveStage(event) {
        event.preventDefault();
        const id = $('#stageId').value;
        const payload = { stageCode: $('#stageCode').value.trim(), stageName: $('#stageName').value.trim(), stageOrder: Number($('#stageOrder').value), isActive: $('#stageIsActive').checked };
        try {
            const path = id ? `/Workflow/UpdateWorkflowStage?id=${id}` : '/Workflow/CreateWorkflowStage';
            await callApi(path, { method: id ? 'PUT' : 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
            window.jQuery(state.modal).modal('hide'); showAlert(id ? 'Đã cập nhật bước lớn.' : 'Đã thêm bước lớn.'); await loadStages();
        } catch (error) { showAlert(error.message || 'Không thể lưu dữ liệu.', true); }
    }

    async function deleteStage(stage) {
        if (!stage || !confirm(`Bạn có chắc muốn xóa bước "${stage.name}"?`)) return;
        try {
            await callApi(`/Workflow/DeleteWorkflowStage?id=${stage.id}`, { method: 'DELETE' });
            showAlert('Đã xóa bước lớn.'); await loadStages();
        } catch (error) { showAlert(error.message || 'Không thể xóa dữ liệu.', true); }
    }

    function showAlert(message, isError = false) {
        const alert = $('#stageAlert');
        alert.textContent = message;
        alert.className = `alert position-fixed bottom-0 end-0 m-3 ${isError ? 'alert-danger' : 'alert-success'}`;
        setTimeout(() => alert.classList.add('d-none'), 3000);
    }

    function escapeHtml(value) {
        return String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
    }
})();
