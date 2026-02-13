// --- 1. Quản lý Trạng thái Phân trang ---
const userPagingState = {
    fullData: [],
    currentPage: 1,
    itemsPerPage: 15, // Số mục muốn hiển thị trên mỗi trang
    totalPages: 0
};
// --- 2. Hàm Tải Dữ liệu Chính (Fetch) ---
function _load_warehouse() {

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
            userPagingState.fullData = data;
            // Tính tổng số trang
            userPagingState.totalPages = Math.ceil(data.length / userPagingState.itemsPerPage);

            // Khởi tạo hiển thị trang đầu tiên
            goToPage(1);
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
    const tbody = document.getElementById("show_wh");
    console.log(data);
    // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
    const htmlContent = data.map(vd => {

        const btn = `<td><a class="text-primary"><i class="fas fa-pencil-alt" onclick="_get_modal('${vd.id}')"></i></a></td>
                     <td><a class="text-danger"> <i class="fa fa-trash"></i></a></td>`;
        return `<tr>                 
                    <td id="wh_${vd.id}" >${vd.chR_WAREHOUSE}</td>
                    <td id="pb_${vd.id}">${vd.chR_DEPT_USE}</td>
                    <td id="fac_${vd.id}">${vd.chR_FACTORY}</td>
                    <td id="dt_${vd.id}">${vd.dtM_UPDATE}</td>
                    <td id="us_${vd.id}">${vd.chR_USER}</td>                   
                 
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