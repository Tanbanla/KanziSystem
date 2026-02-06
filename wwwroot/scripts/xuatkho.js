// --- 1. Quản lý Trạng thái Phân trang ---
const userPagingState = {
    fullData: [],
    currentPage: 1,
    itemsPerPage: 18, // Số mục muốn hiển thị trên mỗi trang
    totalPages: 0
};

// --- 2. Hàm Tải Dữ liệu Chính (Fetch) ---
function _load_xuatkho() {

    fetch('/Import/chitiet_xuatkho', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        /*body: params.toString()*/
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log(data);
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
    const tbody = document.getElementById("list_xuatkho");
    // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
        const htmlContent = data.map(user => {           
            return `<tr>
                <td class="text-center"><input type="checkbox" /></td>
                <td>${user.code_Request}</td>
                <td>${user.material_Code}</td>
                <td>${user.material_Name}</td>
                <td>${user.amount}</td>
                <td>${user.unit}</td>
                <td>${user.price}</td>            
                <td>${user.chR_ADID_NGUOIYEUCAU}</td>
                <td>${user.chR_ADID_XUATKHO}</td>        
                <td><button class="btn btn-danger" onclick="_load_tonkhotheonhamay('${user.material_Code}','${user.amount}','${user.id_RequestDetail}','${user.unit}')"><i class="fa ion-ios-cart"></i>&nbsp; XK</button></td>
            </tr>`;
        }).join(''); // Thêm .join('') để biến mảng thành một chuỗi HTML lớn 
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

function _load_tonkhotheonhamay(mahang, sl, id_rq, unit) {
    document.getElementById("modal-10").click();
    const params = new URLSearchParams();
    params.append('mahang', mahang);

    fetch('/Import/_tonkhotheonhamay', {
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
            document.getElementById("_lskho").innerHTML = "";
            data.lst_kho.forEach(item => {
                document.getElementById("_lskho").innerHTML += `<option value="${item}">${item}</option>`;
            });
            document.getElementById("sl_ton").value = data.soluong;
            document.getElementById("mahagg").innerHTML = mahang;
            document.getElementById("sl_canxuat").innerHTML = sl;
            document.getElementById("madon").innerHTML = id_rq;
            document.getElementById("donvi").innerHTML = unit;
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

function _soluongtontainhamay(kho) {
    var mahang = document.getElementById("mahagg").innerHTML;
    const params = new URLSearchParams();
    params.append('mahang', mahang);
    params.append('kho', kho);

    fetch('/Import/_chonnhamay', {
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
            document.getElementById("sl_ton").value = data;
          
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}

function caculator() {
    document.getElementById("sl_xuat").setAttribute('max', parseFloat(document.getElementById("sl_ton").value));
    var currentVal = parseFloat(document.getElementById("sl_xuat").value);
    var tonKho = parseFloat(document.getElementById("sl_ton").value);
    var canxuat = parseFloat(document.getElementById("sl_canxuat").innerHTML);
    // Nếu nhập nhiều hơn tồn kho
  
    if (currentVal > canxuat ) {
        alert("Số lượng xuất không được vượt quá số lượng yêu cầu : " + canxuat + "");
        document.getElementById("sl_xuat").value = canxuat; // Tự động đưa về giá trị Max (tồn kho)   
    }
    if (currentVal > tonKho) {
        alert("Số lượng xuất vượt quá tồn kho : " + tonKho + "");
        document.getElementById("sl_xuat").value = tonKho;
    }
  
}

function _xuatkho() {
    var code_request = document.getElementById("madon").innerHTML;
    var adid_nx = document.getElementById("us").innerHTML;
    var manguyenlieu = document.getElementById("mahagg").innerHTML;
    var soluong = document.getElementById("sl_xuat").value;
    var donvi = document.getElementById("donvi").innerHTML;
    var kho = document.getElementById("_lskho").value;
    var nguoinhan = document.getElementById("nguoinhan").value;

    const params = new URLSearchParams();
    params.append('code_request', code_request);
    params.append('adid_nx', adid_nx);
    params.append('manguyenlieu', manguyenlieu);
    params.append('soluong', soluong);
    params.append('donvi', donvi);
    params.append('kho', kho);
    params.append('nguoinhan', nguoinhan);

    fetch('/Import/_xuatkhothucte', {
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
            alert(data);
            document.querySelectorAll('.close').forEach(button => button.click());
            _load_xuatkho();
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}