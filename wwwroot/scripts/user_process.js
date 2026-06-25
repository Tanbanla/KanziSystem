
// --- 1. Quản lý Trạng thái Phân trang ---
const userPagingState = {
    fullData: [],
    currentPage: 1,
    itemsPerPage: 18, // Số mục muốn hiển thị trên mỗi trang
    totalPages: 0
};

// --- 2. Hàm Tải Dữ liệu Chính (Fetch) ---
function _load_user() {
    var us_code = document.getElementById("us_code").value;
    var us_dept = document.getElementById("us_dept").value;
    var us_adid = document.getElementById("us_adid").value;
 
    const params = new URLSearchParams();
    params.append('us_code', us_code);
    params.append('us_dept', us_dept);
    params.append('us_adid', us_adid);
  
 
    fetch('/ipcs/User/Load_User', {
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
    const tbody = document.getElementById("show_user");
  
    // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
    const htmlContent = data.map(user => {
      
        const btn = `<td><a class="text-primary"><i class="fas fa-pencil-alt" onclick="_get_modal('${user.id}')"></i></a></td>
                     <td><a class="text-danger"> <i class="fa fa-stop-circle"></i></a></td>`;        
        return `<tr>
                    <td>${user.id}</td>
                    <td id="staffCode_${user.id}" >${user.chR_STAFF_CODE}</td>
                    <td id="name_${user.id}">${user.chR_NAME}</td>
                    <td id="adid_${user.id}">${user.chR_ADID}</td>
                    <td id="dept_${user.id}">${user.chR_DEPT}</td>
                    <td id="mail_${user.id}">${user.chR_MAIL}</td>
                    <td id="role_${user.id}">${user.role}</td>
                    <td>${user.dtM_LAST_LOGIN}</td>
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
function _insert_user() {

    var employee_code = document.getElementById("employee_code").value;
    var employee_name = document.getElementById("employee_name").value;
    var employee_adid = document.getElementById("employee_adid").value;
    var employee_mail = document.getElementById("employee_mail").value;
    var employee_dept = document.getElementById("employee_dept").value;
    var employee_role = document.getElementById("employee_role").value;

    const params = new URLSearchParams();
    params.append('name', employee_name);
    params.append('adid', employee_adid);
    params.append('staffCode', employee_code);
    params.append('dept', employee_dept);
    params.append('role', employee_role);
    params.append('mail', employee_mail);
    if (employee_code != "" && employee_name != "" && employee_adid != "" && employee_mail != "" && employee_dept != "" && employee_role != "") {
        fetch('/ipcs/User/Insert_Edit_User', {

            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Yêu cầu thất bại với mã trạng thái: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                alert(data);
            })
            .catch(error => {
                console.error('Lỗi phán định:', error);
                alert("Phán định thất bại: " + error.message);
            });
    }
    else {
        alert("Không được để trống !");
    }
  
}
function _get_modal(id) {
    document.getElementById("ed_employee_code").value = document.getElementById("staffCode_" + id).innerHTML;
    document.getElementById("ed_employee_name").value = document.getElementById("name_" + id).innerHTML;
    document.getElementById("ed_employee_adid").value = document.getElementById("adid_" + id).innerHTML;
    document.getElementById("ed_employee_mail").value = document.getElementById("mail_" + id).innerHTML;
    document.getElementById("ed_employee_dept").value = document.getElementById("dept_" + id).textContent;
    document.getElementById("ed_employee_role").value = document.getElementById("role_" + id).innerHTML;
    document.getElementById("modal-5").click();
}
function _update_user() {

    var employee_code = document.getElementById("ed_employee_code").value;
    var employee_name = document.getElementById("ed_employee_name").value;
    var employee_adid = document.getElementById("ed_employee_adid").value;
    var employee_mail = document.getElementById("ed_employee_mail").value;
    var employee_dept = document.getElementById("ed_employee_dept").value;
    var employee_role = document.getElementById("ed_employee_role").value;

    const params = new URLSearchParams();
    params.append('name', employee_name);
    params.append('adid', employee_adid);
    params.append('staffCode', employee_code);
    params.append('dept', employee_dept);
    params.append('role', employee_role);
    params.append('mail', employee_mail);
    if (employee_name != "" && employee_adid != "" && employee_code != "" && employee_dept != "" && employee_role != "" && employee_mail != "") {
        fetch('/ipcs/User/Insert_Edit_User', {

            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Yêu cầu thất bại với mã trạng thái: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                alert(data);
            })
            .catch(error => {
                console.error('Lỗi phán định:', error);
                alert("Phán định thất bại: " + error.message);
            });
    }
    else {
        alert("Hãy nhập đủ thông tin !");
    }
 
}
