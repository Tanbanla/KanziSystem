let currentPage = 1;
let pageSize = 50;
let totalRecords = 0;

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

        if (page === "prev") currentPage--;
        else if (page === "next") currentPage++;
        else currentPage = parseInt(page);

        _load_material();
    });
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
            alert("Lỗi khi load dữ liệu");
            return;
        }

        const data = await res.json();

        renderTable(data.data || data);
        totalRecords = data.totalCount || data.length || 0;

        renderPagination();
        renderInfo();

    } catch (err) {
        console.error(err);
        alert("Lỗi hệ thống");
    }
}

function renderTable(data) {
    const tbody = document.getElementById("show_material");
    tbody.innerHTML = "";

    if (!data || data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="9" class="text-center">Không có dữ liệu</td></tr>`;
        return;
    }

    data.forEach(item => {
        const row = `
            <tr>
                <td>${item.material_Code || ""}</td>
                <td>${item.material_Name_VN || ""}</td>
                <td>${item.material_Name_EN || ""}</td>
                <td>${item.material_Name_JP || ""}</td>
                <td>${item.unit || ""}</td>
                <td>${item.category_VN || ""}</td>
                <td>${item.code_Suppiler || ""}</td>
                <td>${item.group_Code || ""}</td>
                <td>${item.type || ""}</td>
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

    document.getElementById("historyPaginationInfo").innerText =
        `Hiển thị ${start} - ${end} / ${totalRecords}`;
}

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
            alert("Lỗi download file");
            return;
        }

        const blob = await res.blob();

        // Lấy tên file từ header
        const disposition = res.headers.get("Content-Disposition");
        let fileName = "Material.xlsx";

        if (disposition && disposition.includes("filename=")) {
            fileName = disposition.split("filename=")[1].replace(/"/g, "");
        }

        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();

        window.URL.revokeObjectURL(url);

    } catch (err) {
        console.error(err);
        alert("Lỗi hệ thống khi tải file");
    }
}
function apiUrl(path) {
    const base = (window.apiBaseUrl || '').trim().replace(/\/$/, '');
    if (!base) return path;
    return `${base}${path.startsWith('/') ? '' : '/'}${path}`;
}
