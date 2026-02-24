
// --- 1. Quản lý Trạng thái Phân trang ---
const userPagingState = {
    fullData: [],
    currentPage: 1,
    itemsPerPage: 12, // Số mục muốn hiển thị trên mỗi trang
    totalPages: 0
};

// --- 2. Hàm Tải Dữ liệu Chính (Fetch) ---
function _load_inv() {
    var mnl = document.getElementById("mnl").value;
    var kho = document.getElementById("kho").value;
    var cost = document.getElementById("cost").value;
    var Group_Code = document.getElementById("group_code").value;

    const params = new URLSearchParams();
    params.append('MaNguyenLieu', mnl);
    params.append('Kho', kho);
    params.append('NVCHR_COST', cost);
    params.append('IS_SAVE_WH', '0');
    params.append('Group_Code', Group_Code);


    fetch('/Import/_load_inv', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            userPagingState.fullData = data;
            // Tính tổng số trang
            userPagingState.totalPages = Math.ceil(data.length / userPagingState.itemsPerPage);
            if (data.length == 0) {
                document.getElementById("show_kho").innerHTML = "<tr><td>&nbsp; Không có dữ liệu </td></tr>";
                document.getElementById("pagination-controls").innerHTML = "";
            }
            else {
                // Khởi tạo hiển thị trang đầu tiên
                goToPage(1);
            }
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

// --- 3. Hàm Xử lý Chuyển Trang ---
function goToPage(page) {
    if (page < 1 || page > userPagingState.totalPages) return;

    userPagingState.currentPage = page;

    // Tính toán chỉ số
    const startIndex = (page - 1) * userPagingState.itemsPerPage;
    const endIndex = page * userPagingState.itemsPerPage;

    // Lấy dữ liệu trang hiện tại
    const pageData = userPagingState.fullData.slice(startIndex, endIndex);

    // Render giao diện
    renderUserTable(pageData);
    renderPaginationControls();
}

// --- 4. Hàm Render Bảng  ---
function renderUserTable(data) {
    const tbody = document.getElementById("show_kho");
    console.log(data);
    // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
    const htmlContent = data.map(vd => {
        const btn = `<td><button class=" btn btn-outline-success" onclick="_modal11('${vd.maNguyenLieu}','${vd.kho}','${vd.hientai}','${vd.material_Name}')"><i class="fas ion-arrow-swap" ></i> Chuyển kho</button></td>`;
        return `<tr>
                    <td>${vd.maNguyenLieu}</td>
                    <td>${vd.material_Name}</td>
                    <td>${vd.hientai}</td>
                    <td>${vd.toiThieu}</td>
                    <td>${vd.toiDa}</td>
                    <td>${vd.group_Code}</td>
                    <td>${vd.kho}</td>
                    <td>${vd.nvchR_COST}</td>
                    <td>${vd.nvchr_note}</td>
                    <td>${vd.dtM_UPDATE}</td>
                        ${btn}
                </tr>`;
    }).join(''); // Nối tất cả các chuỗi thành một chuỗi HTML lớn
    // Cập nhật DOM chỉ MỘT lần
    tbody.innerHTML = htmlContent;
}

// --- 5. Hàm Render Nút Phân trang ---
function renderPaginationControls() {

    const container = document.getElementById("pagination-controls");
    const { currentPage, totalPages } = userPagingState;
    const maxButtonsToShow = 5; // Số nút tối đa (không tính Previous/Next)
    let controlsHTML = '';

    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }

    // Nút Previous
    controlsHTML += ` <li class="page-item"><a class="page-link ${currentPage === 1 ? 'disabled' : ''} onclick="goToPage(${currentPage - 1})">« </a></li>`;

    // Tính toán phạm vi hiển thị
    let startPage = Math.max(1, currentPage - Math.floor((maxButtonsToShow - 1) / 2));
    let endPage = Math.min(totalPages, startPage + maxButtonsToShow - 1);

    // Điều chỉnh nếu phạm vi bị thu hẹp ở cuối
    if (endPage - startPage + 1 < maxButtonsToShow) {
        startPage = Math.max(1, endPage - maxButtonsToShow + 1);
    }

    // 1. Nút Trang 1
    if (startPage > 1) {
        controlsHTML += renderButton(1, currentPage);
        // Thêm dấu ... nếu trang 1 không sát với nút bắt đầu
        if (startPage > 2) {
            controlsHTML += `<span class="px-2">...</span>`;
        }
    }

    // 2. Các nút chính giữa
    for (let i = startPage; i <= endPage; i++) {
        controlsHTML += renderButton(i, currentPage);
    }

    // 3. Nút Trang Cuối
    if (endPage < totalPages) {
        // Thêm dấu ... nếu nút cuối không sát với trang cuối
        if (endPage < totalPages - 1) {
            controlsHTML += `<span class="px-2">...</span>`;
        }
        controlsHTML += renderButton(totalPages, currentPage);
    }

    // Nút Next

    controlsHTML += ` <li class="page-item"><a class="page-link  ${currentPage === totalPages ? 'disabled' : ''} onclick="goToPage(${currentPage + 1})"> »</a></li>`;
    container.innerHTML = controlsHTML;
}

// Hàm trợ giúp để tạo nút
function renderButton(pageNumber, currentPage) {
    const activeClass = pageNumber === currentPage ? 'btn-primary text-white' : '';
    return ` <li class="page-item"><a class="page-link  ${activeClass}"  onclick="goToPage(${pageNumber})">${pageNumber}</a></li>`;
}

// Biến lưu dữ liệu nạp từ server để dùng cho các dòng clone sau này
let globalMaterialData = [];

// Hàm khởi tạo Select2 (Dùng chung)
function initSelect2($element) {
    if ($.fn.select2) {
        $element.select2({
            placeholder: "Chọn hàng hóa...",
            width: '100%',
            allowClear: true
        });
    }
}

// Cập nhật lại hàm load trong import_process.js hoặc tại đây
function _load_name_inv(group_code) {
    const params = new URLSearchParams({ group_code: group_code });
    fetch('/Import/_load_material', {
        method: 'POST',
        body: params
    })
        .then(res => res.json())
        .then(data => {
            globalMaterialData = data; // Lưu lại để dùng sau
            const $allSelects = $('.select2');

            // Nạp data cho toàn bộ select2 đang có trên màn hình
            fillDataToSelect($allSelects, data);
        });
}

// Hàm đổ dữ liệu vào ô Select
function fillDataToSelect($el, data) {
    $el.empty().append('<option></option>');
    data.forEach(item => {
        $el.append(new Option(item, item));
    });
    $el.trigger('change');
}

function _load_info_adid() {

    var us = document.getElementById("us").innerHTML;

    const params = new URLSearchParams();
    params.append('us', us);

    fetch('/Import/_load_user_info', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            document.getElementById("name_dept").innerHTML = "";
            data.forEach(item => {

                document.getElementById("name_dept").innerHTML += `<option>${item.cost_Center}:${item.name}</option>`
            });
        })
        .catch(error => console.error('Error:', error));
}

function _load_dept(dept) {
    var us = document.getElementById("us").innerHTML;

    const params = new URLSearchParams();
    params.append('dept', dept);
    params.append('us', us);

    fetch('/Import/_load_dept', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: params
    })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            document.getElementById("dept").value = dept.split(':')[1];
            document.getElementById("secs").value = data.cost_Center_Group;
            
        })
        .catch(error => console.error('Error:', error));
}

function _load_material(Material_Code, id) {

    let idd = id.split('_')[1];
    var groupcode = document.getElementById("group_code").value;
    const params = new URLSearchParams();
    params.append('Material_Code', Material_Code);
    params.append('Material_Name_VN', '');
    params.append('Account_Name_VN', '');
    params.append('Group_Code', groupcode);


    fetch('/Import/_info_material', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
            document.getElementById("stk_" + idd).value = data[0].account_Code + ":" + data[0].account_Name_EN;
            document.getElementById("tk_" + idd).value = data[0].num_Inventory;
            document.getElementById("kho_" + idd).value = data[0].inventory;
            if (data[0].unit_Note == "" || data[0].unit_Note == "-") {
                document.getElementById("dv_" + idd).value = data[0].unit;
            }
            else {
                document.getElementById("dv_" + idd).value = data[0].unit_Note;
            }
            document.getElementById("dg_" + idd).value = data[0].price;
            document.getElementById("nt_" + idd).value = data[0].currency;
            if (document.getElementById("sl_" + idd).value != "") {
                document.getElementById("tcp_" + idd).value = parseFloat(data[0].price) * parseFloat(document.getElementById("sl_" + idd).value);
            }
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

function _modal11(ma, kho, soluon, ten) {
    document.getElementById("modal-11").click();
    document.getElementById("ma_ck").innerHTML = ma + " -  " + ten;
    document.getElementById("kho_hientai").value = kho;
    document.getElementById("tonkho").value = soluon;
    _Fac()
}

function _Fac() {

    fetch('/Master/load_warehouse', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        /* body: params.toString()*/
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("nhamay_chuyen").innerHTML = "<option></option>";
            for (var i = 0; i < data.length; i++) {
                document.getElementById("nhamay_chuyen").innerHTML += `<option value="${data[i].chR_FACTORY}">${data[i].chR_FACTORY}</option>`
            }
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
function _SEC(fac) {

    const params = new URLSearchParams({ fac: fac });
        fetch('/Master/load_sec', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
             body: params.toString()
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("phongban_chuyen").innerHTML = "<option></option>";
            for (var i = 0; i < data.length; i++) {
                document.getElementById("phongban_chuyen").innerHTML += `<option>${data[i].chR_DEPT_USE}</option>`
            }
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
function _WH() {

    var fac = document.getElementById("nhamay_chuyen").value;
    var sec = document.getElementById("phongban_chuyen").value;

    const params = new URLSearchParams({ fac: fac, sec : sec });
    fetch('/Master/load_wh', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
            document.getElementById("den_kho").innerHTML = "<option></option>";
            for (var i = 0; i < data.length; i++) {
                document.getElementById("den_kho").innerHTML += `<option>${data[i].chR_WAREHOUSE}</option>`
            }
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}