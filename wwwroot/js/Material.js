let currentPage = 1;
let pageSize = 50;
let totalRecords = 0;
let materialsOnPage = [];
let selectedMaterialCodeToDelete = "";
let selectedMaterialForEdit = null;
const i18nMaterial = window.i18nMaterial || {};

document.addEventListener("DOMContentLoaded", function () {
    _load_material();

    // Enter để search
    document.querySelectorAll("#Material_Code, #Material_Name_VN, #Category_VN")
        .forEach(input => {
            input.addEventListener("keypress", function (e) {
                if (e.key === "Enter") {
                    currentPage = 1;
                    _load_material();
                }
            });
        });

    document.getElementById("btnSearch")?.addEventListener("click", function () {
        currentPage = 1;
        _load_material();
    });

    document.getElementById("lst_gc")?.addEventListener("change", function () {
        currentPage = 1;
        _load_material();
    });

    document.getElementById("btnDownload")?.addEventListener("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        _download_material();
    });

    // Change page size
    document.getElementById("historyPageSize").addEventListener("change", function () {
        pageSize = parseInt(this.value);
        currentPage = 1;
        _load_material();
    });

    // Pagination click
    document.getElementById("historyPagination").addEventListener("click", function (e) {
        let btn = e.target.closest("button");
        if (!btn) return;

        let page = btn.getAttribute("data-page");
        const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));

        if (page === "prev") currentPage--;
        else if (page === "next") currentPage++;
        else currentPage = parseInt(page);

        if (currentPage < 1) currentPage = 1;
        if (currentPage > totalPages) currentPage = totalPages;

        _load_material();
    });

    document.getElementById("show_material")?.addEventListener("click", handleTableActionClick);

    document.getElementById("btnConfirmDeleteMaterial")?.addEventListener("click", confirmDeleteMaterial);

    document.getElementById("btnSaveMaterialChanges")?.addEventListener("click", saveMaterialChanges);

    bindModalCloseButtons("deleteMaterialModal");
    bindModalCloseButtons("editMaterialModal");
});

function _getSearchData() {
    return {
        MaterialCode: document.getElementById("Material_Code").value.trim(),
        MaterialName: document.getElementById("Material_Name_VN").value.trim(),
        MaterialCatergory: document.getElementById("Category_VN").value.trim(),
        MaterialGroup: document.getElementById("lst_gc").value,
        pageIndex: currentPage,
        pageSize: pageSize
    };
}

async function _load_material() {
    try {
        const res = await fetch(apiUrl("/Material/SearchMaterialView"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(_getSearchData())
        });

        if (!res.ok) {
            alert(t("LoadDataError", "Lỗi khi load dữ liệu"));
            return;
        }

        const payload = await res.json();
        const data = Array.isArray(payload?.data)
            ? payload.data
            : (Array.isArray(payload) ? payload : []);

        materialsOnPage = data;
        renderTable(data);
        totalRecords = payload?.totalCount ?? data.length ?? 0;

        const totalPages = Math.max(1, Math.ceil(totalRecords / pageSize));
        if (currentPage > totalPages) {
            currentPage = totalPages;
            await _load_material();
            return;
        }

        renderPagination();
        renderInfo();

    } catch (err) {
        console.error(err);
        alert(t("SystemError", "Lỗi hệ thống"));
    }
}

function renderTable(data) {
    const tbody = document.getElementById("show_material");
    tbody.innerHTML = "";

    if (!data || data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" class="text-center">${escapeHtml(t("NoData", "Không có dữ liệu"))}</td></tr>`;
        return;
    }

    data.forEach(item => {
        const materialCode = item.material_Code || "";
        const row = `
            <tr>
                <td>${escapeHtml(materialCode)}</td>
                <td>${escapeHtml(item.material_Name_VN)}</td>
                <td>${escapeHtml(item.material_Name_EN)}</td>
                <td>${escapeHtml(item.material_Name_JP)}</td>
                <td>${escapeHtml(item.unit)}</td>
                <td>${escapeHtml(item.category_VN)}</td>
                <td>${escapeHtml(item.code_Suppiler)}</td>
                <td>${escapeHtml(item.group_Code)}</td>
                <td class="text-center">
                    <button type="button" class="btn btn-sm btn-outline-primary mr-1" data-action="edit" data-code="${escapeHtml(materialCode)}">
                        ${escapeHtml(t("Edit", "Sửa"))}
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-danger" data-action="delete" data-code="${escapeHtml(materialCode)}">
                        ${escapeHtml(t("Delete", "Xóa"))}
                    </button>
                </td>
            </tr>
        `;
        tbody.insertAdjacentHTML("beforeend", row);
    });
}

function renderPagination() {
    const totalPages = Math.ceil(totalRecords / pageSize) || 1;
    const container = document.getElementById("historyPagination");

    let html = "";

    html += `
        <li class="page-item ${currentPage === 1 ? "disabled" : ""}">
            <button class="page-link" data-page="prev">«</button>
        </li>
    `;

    for (let i = 1; i <= totalPages; i++) {
        if (i === currentPage ||
            i === 1 ||
            i === totalPages ||
            (i >= currentPage - 1 && i <= currentPage + 1)) {

            html += `
                <li class="page-item ${i === currentPage ? "active" : ""}">
                    <button class="page-link" data-page="${i}">${i}</button>
                </li>
            `;
        }
    }

    html += `
        <li class="page-item ${currentPage === totalPages ? "disabled" : ""}">
            <button class="page-link" data-page="next">»</button>
        </li>
    `;

    container.innerHTML = html;
}

function renderInfo() {
    const start = (currentPage - 1) * pageSize + 1;
    const end = Math.min(start + pageSize - 1, totalRecords);

    const paginationInfo =
        totalRecords === 0
            ? formatString(t("PaginationInfo", "Hiển thị {0} - {1} / {2}"), 0, 0, 0)
            : formatString(t("PaginationInfo", "Hiển thị {0} - {1} / {2}"), start, end, totalRecords);

    document.getElementById("historyPaginationInfo").innerText = paginationInfo;
}

function handleTableActionClick(event) {
    const btn = event.target.closest("button[data-action]");
    if (!btn) {
        return;
    }

    const action = btn.getAttribute("data-action");
    const materialCode = btn.getAttribute("data-code");
    if (!materialCode) {
        return;
    }

    if (action === "delete") {
        openDeleteModal(materialCode);
        return;
    }

    if (action === "edit") {
        openEditModal(materialCode);
    }
}

function openDeleteModal(materialCode) {
    selectedMaterialCodeToDelete = materialCode;
    const messageEl = document.getElementById("deleteMaterialMessage");
    if (messageEl) {
        messageEl.textContent = formatString(
            t("ConfirmDeleteMessageFormat", "Bạn có chắc chắn muốn xóa mã vật tư \"{0}\"?"),
            materialCode
        );
    }

    showModal("deleteMaterialModal");
}

async function confirmDeleteMaterial() {
    if (!selectedMaterialCodeToDelete) {
        return;
    }

    try {
        const res = await fetch(apiUrl(`/Material/DeleteMaterial?codeMaterial=${encodeURIComponent(selectedMaterialCodeToDelete)}`), {
            method: "GET"
        });

        if (!res.ok) {
            const errorText = await res.text();
            alert(errorText || t("DeleteFailed", "Xóa mã vật tư thất bại"));
            return;
        }

        hideModal("deleteMaterialModal");

        if (materialsOnPage.length === 1 && currentPage > 1) {
            currentPage--;
        }

        selectedMaterialCodeToDelete = "";
        await _load_material();
    } catch (err) {
        console.error(err);
        alert(t("DeleteSystemError", "Lỗi hệ thống khi xóa mã vật tư"));
    }
}

function openEditModal(materialCode) {
    selectedMaterialForEdit = materialsOnPage.find(item => item.material_Code === materialCode) || null;

    if (!selectedMaterialForEdit) {
        alert(t("EditNotFound", "Không tìm thấy thông tin vật tư để sửa"));
        return;
    }

    setInputValue("editMaterialCode", selectedMaterialForEdit.material_Code);
    setInputValue("editMaterialNameVN", selectedMaterialForEdit.material_Name_VN);
    setInputValue("editMaterialNameEN", selectedMaterialForEdit.material_Name_EN);
    setInputValue("editMaterialNameJP", selectedMaterialForEdit.material_Name_JP);
    setInputValue("editUnit", selectedMaterialForEdit.unit);
    setInputValue("editCategoryVN", selectedMaterialForEdit.category_VN);
    setInputValue("editCodeSupplier", selectedMaterialForEdit.code_Suppiler);
    setInputValue("editGroupCode", selectedMaterialForEdit.group_Code);

    showModal("editMaterialModal");
}

async function saveMaterialChanges() {
    if (!selectedMaterialForEdit) {
        return;
    }

    const materialCode = getInputValue("editMaterialCode");
    if (!materialCode) {
        alert(t("InvalidMaterialCode", "Mã vật tư không hợp lệ"));
        return;
    }

    const payload = {
        Id_Material: getProp(selectedMaterialForEdit, ["id_Material", "idMaterial"], 0),
        Material_Code: materialCode,
        Material_Name_VN: getInputValue("editMaterialNameVN"),
        Material_Name_EN: getInputValue("editMaterialNameEN"),
        Material_Name_JP: getInputValue("editMaterialNameJP"),
        Unit: getInputValue("editUnit"),
        Category_VN: getInputValue("editCategoryVN"),
        Category_EN: getProp(selectedMaterialForEdit, ["category_EN", "categoryEn"], ""),
        Category_JP: getProp(selectedMaterialForEdit, ["category_JP", "categoryJp"], ""),
        Group_Code: getInputValue("editGroupCode"),
        Shape: getProp(selectedMaterialForEdit, ["shape"], ""),
        Material: getProp(selectedMaterialForEdit, ["material", "material1"], ""),
        Composition: getProp(selectedMaterialForEdit, ["composition"], ""),
        Dimension: getProp(selectedMaterialForEdit, ["dimension"], ""),
        UsedFor: getProp(selectedMaterialForEdit, ["usedFor"], ""),
        Purpose: getProp(selectedMaterialForEdit, ["purpose"], ""),
        Code_Suppiler: getInputValue("editCodeSupplier")
    };

    try {
        const res = await fetch(apiUrl("/Material/UpdateMaterial"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const errorText = await res.text();
            alert(errorText || t("UpdateFailed", "Cập nhật mã vật tư thất bại"));
            return;
        }

        hideModal("editMaterialModal");
        selectedMaterialForEdit = null;
        await _load_material();
    } catch (err) {
        console.error(err);
        alert(t("UpdateSystemError", "Lỗi hệ thống khi cập nhật vật tư"));
    }
}

function t(key, fallback) {
    const value = i18nMaterial[key];
    return value === undefined || value === null || value === "" ? fallback : value;
}

function formatString(template, ...args) {
    return String(template).replace(/\{(\d+)\}/g, (match, index) => {
        const value = args[Number(index)];
        return value === undefined || value === null ? "" : String(value);
    });
}

function setInputValue(id, value) {
    const element = document.getElementById(id);
    if (!element) {
        return;
    }

    element.value = value ?? "";
}

function getInputValue(id) {
    const element = document.getElementById(id);
    return element ? element.value.trim() : "";
}

function getProp(source, keys, defaultValue) {
    for (const key of keys) {
        if (source[key] !== undefined && source[key] !== null) {
            return source[key];
        }
    }

    return defaultValue;
}

function showModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) {
        return;
    }

    removeModalBackdrops();

    if (window.bootstrap?.Modal) {
        const bsModal = window.bootstrap.Modal;
        if (typeof bsModal.getOrCreateInstance === "function") {
            bsModal.getOrCreateInstance(modalElement).show();
            return;
        }

        if (typeof bsModal.getInstance === "function") {
            const instance = bsModal.getInstance(modalElement);
            if (instance && typeof instance.show === "function") {
                instance.show();
                return;
            }
        }

        try {
            const instance = new bsModal(modalElement);
            if (instance && typeof instance.show === "function") {
                instance.show();
                return;
            }
        } catch (e) {
            // ignore 
        }
    }

    if (window.jQuery && typeof window.jQuery.fn.modal === "function") {
        window.jQuery(modalElement).modal("show");
        return;
    }

    modalElement.style.display = "block";
    modalElement.classList.add("show");
}

function hideModal(modalId) {
    const modalElement = document.getElementById(modalId);
    if (!modalElement) {
        return;
    }

    const forceHide = () => {
        modalElement.classList.remove("show");
        modalElement.style.display = "none";
        modalElement.setAttribute("aria-hidden", "true");
        modalElement.removeAttribute("aria-modal");
        removeModalBackdrops();
    };

    if (window.bootstrap?.Modal) {
        const bsModal = window.bootstrap.Modal;
        if (typeof bsModal.getOrCreateInstance === "function") {
            bsModal.getOrCreateInstance(modalElement).hide();
            setTimeout(forceHide, 200);
            return;
        }

        if (typeof bsModal.getInstance === "function") {
            const instance = bsModal.getInstance(modalElement);
            if (instance && typeof instance.hide === "function") {
                instance.hide();
                setTimeout(forceHide, 200);
                return;
            }
        }

        try {
            const instance = new bsModal(modalElement);
            if (instance && typeof instance.hide === "function") {
                instance.hide();
                setTimeout(forceHide, 200);
                return;
            }
        } catch (e) {
            // ignore 
        }
    }

    if (window.jQuery && typeof window.jQuery.fn.modal === "function") {
        window.jQuery(modalElement).modal("hide");
        setTimeout(forceHide, 200);
        return;
    }

    forceHide();
}

function bindModalCloseButtons(modalId) {
    const modal = document.getElementById(modalId);
    if (!modal) {
        return;
    }

    modal.querySelectorAll("[data-dismiss='modal'], .close").forEach(button => {
        if (button.dataset.modalCloseBound === "true") {
            return;
        }

        button.dataset.modalCloseBound = "true";
        button.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
            hideModal(modalId);
        });
    });
}

function removeModalBackdrops() {
    document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());

    document.body.classList.remove('modal-open');
    document.body.style.removeProperty('padding-right');
}


function escapeHtml(value) {
    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/\"/g, "&quot;")
        .replace(/'/g, "&#39;");
}
// Input material info from excel file
document.getElementById('btnImportExcelMaterial')?.addEventListener('click', () => document.getElementById('itemsExcelFileInputMaterial')?.click());
document.getElementById('itemsExcelFileInputMaterial')?.addEventListener('change', async (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
        const fd = new FormData();
        fd.append('FileExcel', file);
        const res = await fetch('/Master/ImportExcelMaterial', { method: 'POST', body: fd });
        if (!res.ok) {
            let txt = await res.text();
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportFailed || 'Nhập thất bại') + ': ' + (txt || res.statusText), type: 'error' });
        } else {
            const T = window.i18nSupplierMana || {};
            showDialog({ title: (T.ImportExcel || 'Nhập Excel'), message: (T.ImportSuccess || 'Nhập file thành công'), type: 'success' });
            await loadSupplierItems(currentItemSupplier.ma);
        }
    } catch (err) {
        const T = window.i18nSupplierMana || {};
        showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.CannotSendFile || 'Không thể gửi file') + ': ' + (err.message || err), type: 'error' });
    }
    e.target.value = '';
});
// download template excel file
document.getElementById('btnTemplateImportExcel')?.addEventListener('click', async () => {
    const T = window.i18nSupplierMana || {};
    try {
        const templates = [
            { url: (window.apiBaseUrl || '') + '/template/MaterialMasterActions.xlsx', filename: 'Mẫu file Master Material.xlsx' },
        ];

        for (const template of templates) {
            const a = document.createElement('a');
            a.href = template.url;
            a.download = template.filename;
            document.body.appendChild(a);
            a.click();
            a.remove();
        }

        showDialog({ title: (T.SuccessTitle || 'Thành công'), message: (T.ExportSuccess || 'Xuất file mẫu hoàn tất'), type: 'success' });
    } catch (err) {
        const T = window.i18nSupplierMana || {};
        showDialog({ title: (T.ErrorTitle || 'Lỗi'), message: (T.ExportFailed || 'Xuất file thất bại') + ': ' + (err && err.message ? err.message : err), type: 'error' });
    }
});
async function _download_material() {
    try {
        const res = await fetch(apiUrl("/Material/ExportMaterialViewToExcel"), {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(_getSearchData())
        });

        if (!res.ok) {
            alert(t("DownloadError", "Lỗi download file"));
            return;
        }

        const blob = await res.blob();

        // Lấy tên file từ header
        const disposition = res.headers.get("Content-Disposition");
        let fileName = "Material.xlsx";

        if (disposition) {
            const fileNameStarMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);

            if (fileNameStarMatch?.[1]) {
                fileName = decodeURIComponent(fileNameStarMatch[1]);
            } else {
                const fileNameMatch = disposition.match(/filename="?([^"]+)"?/i);

                if (fileNameMatch?.[1]) {
                    fileName = fileNameMatch[1];
                }
            }
        }

        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = fileName;
        a.target = "_blank";
        a.rel = "noopener";
        document.body.appendChild(a);
        a.click();
        a.remove();

        window.URL.revokeObjectURL(url);

    } catch (err) {
        console.error(err);
        alert(t("DownloadSystemError", "Lỗi hệ thống khi tải file"));
    }
}
function apiUrl(path) {
    const base = (window.apiBaseUrl || '').trim().replace(/\/$/, '');
    if (!base) return path;
    return `${base}${path.startsWith('/') ? '' : '/'}${path}`;
}
// show message dialog
function getDialogEls() {
    const overlay = document.getElementById('cmDialogOverlay');
    const titleEl = document.getElementById('cmDialogTitle');
    const bodyEl = document.getElementById('cmDialogBody');
    const footerEl = document.getElementById('cmDialogFooter');
    return { overlay, titleEl, bodyEl, footerEl };
}
function showDialog({ title = (window.i18nSupplierMana && window.i18nSupplierMana.Notification) || 'Thông báo', message = '', type = 'info', buttons } = {}) {
    const { overlay, titleEl, bodyEl, footerEl } = getDialogEls();
    if (!overlay) return alert(message);

    // Ensure overlay is attached to body so fixed positioning is not clipped by parent containers
    try {
        if (overlay.parentElement !== document.body) document.body.appendChild(overlay);
    } catch (e) { /* ignore */ }

    titleEl.textContent = title;
    bodyEl.innerHTML = `<div class="d-flex align-items-start gap-2">
            <i class="fas ${type === 'success' ? 'fa-check-circle text-success' : type === 'error' ? 'fa-exclamation-circle text-danger' : 'fa-info-circle text-primary'}"></i>
            <div>${message}</div>
        </div>`;
    footerEl.innerHTML = '';
    const okBtn = document.createElement('button');
    okBtn.className = 'cm-btn cm-btn-primary';
    const T = window.i18nSupplierMana || {};
    okBtn.textContent = (buttons && buttons.okText) || (T.OK || 'Đồng ý');
    okBtn.addEventListener('click', () => hideDialog());
    footerEl.appendChild(okBtn);

    overlay.setAttribute('aria-hidden', 'false');
    overlay.style.display = 'flex';
    attachDialogCloseHandlers();
}
function hideDialog() {
    const { overlay } = getDialogEls();
    if (overlay) {
        overlay.style.display = 'none';
        overlay.setAttribute('aria-hidden', 'true');
    }
}

function attachDialogCloseHandlers() {
    const { overlay, footerEl } = getDialogEls();
    const closeBtn = overlay.querySelector('[data-cm-action="close"]');
    if (closeBtn) {
        closeBtn.onclick = () => {
            // If a confirm dialog is waiting, resolve it as false
            if (typeof window.__cmPendingResolve === 'function') {
                const r = window.__cmPendingResolve;
                window.__cmPendingResolve = null;
                r(false);
            }
            hideDialog();
        };
    }
    const overlayClick = overlay.querySelector('[data-cm-action="overlay"]');
    if (overlayClick) overlayClick.onclick = () => {
        if (typeof window.__cmPendingResolve === 'function') {
            const r = window.__cmPendingResolve;
            window.__cmPendingResolve = null;
            r(false);
        }
        hideDialog();
    };
}
