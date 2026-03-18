// --- 1. Quản lý Trạng thái Phân trang ---
const userPagingState = {
    fullData: [],
    currentPage: 1,
    itemsPerPage: 18, // Số mục muốn hiển thị trên mỗi trang
    totalPages: 0
};

// --- 2. Hàm Tải Dữ liệu Chính (Fetch) ---
function _load_log() {
    var madon = document.getElementById("madon").value;
    var ngay_tu = document.getElementById("ngay_tu").value;
    var ngay_den = document.getElementById("ngay_den").value;
    var kho = document.getElementById("kho").value;
    var manguyenlieu = document.getElementById("manguyenlieu").value;
    var loai = document.getElementById("loai").value;
    var phong = document.getElementById("phong").value;

    const params = new URLSearchParams();
    params.append('madon', madon);
    params.append('ngay_tu', ngay_tu);
    params.append('ngay_den', ngay_den);
    params.append('kho', kho);
    params.append('manguyenlieu', manguyenlieu);
    params.append('loai', loai);
    params.append('phong', phong);

    fetch('/Import/_get_log', {
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
            document.getElementById("list_log").innerHTML = "";
            console.log(data);
            data.forEach((item) => {
                document.getElementById("list_log").innerHTML += `<tr>
                    <td>${item.maNguyenLieu}</td>
                    <td>${item.hanhdong}</td>
                    <td>${item.soluong}</td>
                    <td>${item.soluongPO}</td>
                    <td>${item.soPO}</td>
                    <td>${item.donviPO}</td>
                    <td>${item.loai}</td>
                    <td>${item.ngaynhaokho}</td>
                    <td>${item.nguoicapnhat}</td>
                    <td>${item.kho}</td>
                    <td>${item.khoi}</td>                 
                    <td>${item.vitri}</td>
                    <td>${item.phong}</td>
                </tr>`;
            });
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
    const tbody = document.getElementById("list_log");

    // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
    const htmlContent = data.map(user => {
   
        return `<tr>
                    <td>${user.maNguyenLieu}</td>
                    <td>${user.hanhdong}</td>
                    <td>${user.soluong}</td>
                    <td>${user.loai}</td>
                    <td>${user.ngaynhaokho}</td>
                    <td>${user.nguoicapnhat}</td>
                    <td>${user.kho}</td>
                    <td>${user.khoi}</td>                 
                    <td>${user.vitri}</td>
                    <td>${user.phong}</td>
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

async function downloadExcel() {
    // 1. Lấy dữ liệu
    const dateToElement = document.getElementById("date_to");
    const dateFromElement = document.getElementById("date_from");

    if (!dateToElement.value || !dateFromElement.value) {
        alert("Vui lòng chọn ngày!");
        return;
    }

    const parra = new URLSearchParams();
    parra.append('date_to', dateToElement.value);
    parra.append('date_from', dateFromElement.value);

    fetch('/Import/download_log', {

        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: parra.toString()
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`Yêu cầu thất bại với mã trạng thái: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
        })
        .catch(error => {
            console.error('Lỗi phán định:', error);
            alert("Phán định thất bại: " + error.message);
        });
}