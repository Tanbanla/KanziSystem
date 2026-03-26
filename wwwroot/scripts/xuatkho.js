// Load trang xuất kho 
function _load_xuatkho() {
    var mayeucau = document.getElementById("mayeucau").value;
    var nguoitao = document.getElementById("nguoitao").value;
    var khoi = document.getElementById("khoi").value;
    const params = new URLSearchParams({ mayeucau: mayeucau, nguoitao: nguoitao, khoi: khoi });
    fetch('/Import/_load_xuatkhohang', {
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
            const tbody = document.getElementById("list_xuatkho");
            // Sử dụng Array.map() và Array.join('') để tối ưu hóa việc tạo HTML
            const htmlContent = data.map(user => {
                let loaichiphi = "";
                if (user.declaration == "AUXILIARY")
                {
                    loaichiphi = "Chi phí biến đổi theo sản lượng";
                }
                if (user.declaration == "EUQIMENT") {
                    loaichiphi = "Chi phí cố định";
                }
                if (user.declaration == "FIXED") {
                    loaichiphi = "Tài sản cố định";
                }
                if (user.declaration == "COMMON") {
                    loaichiphi = "Chi phí chung";
                }
                if (user.declaration == "OTHER") {
                    loaichiphi = "OTHER";
                }
                let hangdanhmuc = "";
                if (user.kind == "OUT") {
                    hangdanhmuc = "Hàng ngoài danh mục";
                }
                if (user.kind == "IN") {
                    hangdanhmuc = "Hàng trong danh mục";
                }
                let tinhtrang = "";
                if (user.status == "WAITCONFIRM") {
                    tinhtrang = "Đang chờ xác nhận";
                }
                if (user.status == "DONE") {
                    tinhtrang = "Đã hoàn thành";
                } 
                if (user.status == "ACCEPT") {
                    tinhtrang = "Chờ xác nhận";
                } 
                return `<tr class="main-row">
                <td class="text-primary text-center" onclick="_modal_chitietxuatkho('${user.code_Request}')"><button type="button" class="btn btn-outline-success" style="padding:2px 5px 2px 5px" >chi tiết</button></td>
                <td>${tinhtrang}</td>
                <td hidden id="idrq_${user.code_Request}">${user.iD_REQUEST}</td>
                <td class="text-primary" style="cursor:pointer" onclick="toggleRow('${user.code_Request}')">${user.code_Request}</td>
                <td>${user.cost_Center}</td>
                <td>${user.create_Date}</td>
                <td id="lcp_${user.code_Request}">${loaichiphi}</td>
                <td>${user.dealine.split(' ')[0]}</td>
                <td>${user.total_exchange}</td>            
                <td>${user.total}</td>
                <td id="hdm_${user.code_Request}">${hangdanhmuc}</td>
                <td>${user.type}</td>
                <td>${user.create_Date}</td>
                <td>${user.user_Create}</td>
                <td>${user.last_Update}</td>
                <td>${user.user_Update}</td>
                <td>${user.group_Code}</td>
                </tr>
                <tr id="dt_${user.code_Request}" class="collapse-row" style="display: none; background-color: #f8f9fa;">
                 <td colspan="18">
                        <div class="p-3">
                            <div class="row">
                               <table class="table table-bordered">
                                        <tr class="bg-light">
                                        <td>No</td>
                                        <td>Mã hàng</td>
                                        <td>Tên hàng (VN)</td>                                
                                        <td>Hãng</td>
                                        <td>Mã sản phẩm </td>
                                        <td>Số tài khoản</td>
                                        <td>Tên tài khoản</td>
                                        <td>Số lượng</td>
                                        <td>Số lượng thực tế</td>
                                        <td>Đơn vị</td>
                                        <td>Đơn giá</td>
                                        <td>Đơn giá thực tế</td>
                                        <td>VAT(%)</td>
                                        <td>Tổng chi phí</td>
                                        <td>Tổng chi phí thực tế</td>
                                        <td>Số PO</td>
                                        <td></td>
                                    </tr>   
                                    <tbody id="_bd_${user.code_Request}"></tbody>
                                </table>
                            </div>
                        </div>
                    </td>
                </tr>`;
            }).join(''); // Thêm .join('') để biến mảng thành một chuỗi HTML lớn 
            tbody.innerHTML = htmlContent;
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
function toggleRow(id) {
    // id is expected to be the raw code_Request (without "dt_" prefix)
    var element = document.getElementById("dt_" + id);
    // defensive fallback in case callers pass full id
    if (!element) {
        element = document.getElementById(id);
    }
    if (!element) {
        console.warn('toggleRow: element not found for', id);
        return;
    }
    if (element.style.display === "none") {
        element.style.display = "table-row";
    } else {
        element.style.display = "none";
    }
    _load_body_detail(id);
}
// load thông tin chi tiết trong modal
function _load_body_detail(code_request) {

    const params = new URLSearchParams();
    params.append('code_request', code_request);

    fetch('/Import/_load_body_detail', {
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
            const container = document.getElementById("_bd_" + code_request);
            console.log(data);
            container.innerHTML = "";
            data.forEach((item,i) => {
                    container.innerHTML += `<tr>
                    <td>${i + 1}</td>
                    <td>${item.material_Code}</td>
                    <td>${item.material_Name}</td>                 
                    <td>${item.brand}</td>
                    <td>${item.good_Code}</td>
                    <td>${item.account_Code}</td>
                    <td>${item.account_Name}</td>
                    <td>${item.amount}</td>
                    <td>${item.amount}</td>
                    <td>${item.unit} </td>
                    <td>${item.price} </td>
                    <td>${item.price_Real} </td>
                    <td>${item.vat} % </td>
                    <td>${item.total_exchange}</td>
                    <td>${item.total_exchange_real}</td>
                    <td>${item.po}</td>
                </tr>`;
            })
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
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

    if (currentVal > canxuat) {
        alert("Số lượng xuất không được vượt quá số lượng yêu cầu : " + canxuat + "");
        document.getElementById("sl_xuat").value = canxuat; // Tự động đưa về giá trị Max (tồn kho)   
    }
    if (currentVal > tonKho) {
        alert("Số lượng xuất vượt quá tồn kho : " + tonKho);
        document.getElementById("sl_xuat").value = tonKho;
    }

}
function _xuatkho() {
    var code_request = document.getElementById("madonn").innerHTML;
    var adid_nx = document.getElementById("us").innerHTML;
    var nguoinhan = document.getElementById("nguoinhan_thucte").value;
    var nguoixuatkho = document.getElementById("nguoixuatkho_thucte").value;
    var thoigian = document.getElementById("thoigianxuat_thucte").value;
    var vitri = document.getElementById("vitri").textContent;
    var khoi = document.getElementById("khoi_yc").innerHTML;
    var phong = document.getElementById("phong_yc").innerHTML;
    var id_rq = document.getElementById("id_rq").innerHTML;
    var kho = document.getElementById("khoSelect").value;
    if (nguoixuatkho == "" || nguoinhan == "") {
        alert("Điền thông tin người xuất kho và người nhận !");
    }
    else {
        const checkboxes = document.querySelectorAll('input.itemsmall');

        checkboxes.forEach((item, i) => {
            var manguyenlieu = document.getElementById("mahang_" + i).innerHTML;
            var soluong = document.getElementById("slthucte_" + i).value;
            var giathucte = document.getElementById("dgthucte_" + i).value;
            var donvi = document.getElementById("donvi_" + i).innerHTML;         
            var tongchiphi = document.getElementById("ttthucte_" + i).innerHTML;
            var tongchiphiold = document.getElementById("tongchiphiold_" + i).innerHTML;
        
            const params = new URLSearchParams();
            params.append('code_request', code_request);
            params.append('adid_nx', adid_nx);
            params.append('nguoinhan', nguoinhan);
            params.append('nguoixuatkho', nguoixuatkho);
            params.append('thoigian', thoigian);
            params.append('manguyenlieu', manguyenlieu);
            params.append('soluong', soluong);
            params.append('giathucte', giathucte);
            params.append('donvi', donvi);
            params.append('kho', kho);
            params.append('tongchiphi', tongchiphi);
            params.append('tongchiphi', tongchiphi);
            params.append('vitri', vitri);
            params.append('phong', phong);
            params.append('khoi', khoi);
            params.append('tongchiphiold', tongchiphiold);
            params.append('id_rq', id_rq);

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
        });
    }
    
  
}
function _dlxuatkho() {
    var code_request = document.getElementById("madonn").innerHTML;
    const params = new URLSearchParams();
    params.append('code_request', code_request);
    fetch('/Import/ExportModalDetail', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.blob();
        })
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            const timestamp = new Date().getTime();
            a.download = `HangTrongDanhMuc_${code_request}.xlsm`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);

            alert("Hoàn thành download dữ liệu");
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
// Hiển thị chi tiết khi hiện modal
function _modal_chitietxuatkho(code_request) {
    document.getElementById("modal-19").click();
    const params = new URLSearchParams();
    params.append('code_request', code_request);

    fetch('/Import/_load_modal_detail', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: params.toString()
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            // Cập nhật thông tin Header Modal
            const header = data.load[0];
            document.getElementById("loaiphieu").innerHTML = document.getElementById("hdm_" + code_request).innerHTML;
            document.getElementById("loaichiphi").innerHTML = document.getElementById("lcp_" + code_request).innerHTML;
            document.getElementById("phongban").innerHTML = `${header.cost_Center}:${header.name}`;
            document.getElementById("tenphong").innerHTML = header.name;
            document.getElementById("bophan").innerHTML = header.cost_Center_Group;
            document.getElementById("vitri").innerHTML = header.place;
            document.getElementById("usd").innerHTML = header.total_exchange;
            document.getElementById("tongdonhang").innerHTML = header.total;
            document.getElementById("ngayyeucau").innerHTML = header.create_Date;
            document.getElementById("thoihanmuonnhan").innerHTML = header.dealine.split(' ')[0];
            document.getElementById("loaihang").innerHTML = header.typee;
            document.getElementById("loaihinhtokhai").innerHTML = header.loaihinhtokhai;
            document.getElementById("thucteusd").innerHTML = header.total_exchange_real || 0;
            document.getElementById("thucte").innerHTML = header.total_Real || 0;
            document.getElementById("madonn").innerHTML = header.code_Request;
            document.getElementById("nguoitaoo").innerHTML = `${header.user_Create} - ${header.create_Date}`;
            document.getElementById("ghichu").innerHTML = header.note || "-";
            document.getElementById("khoi_yc").innerHTML = header.group_Code || "-";
            document.getElementById("phong_yc").innerHTML = header.cost_Center || "-";
            document.getElementById("id_rq").innerHTML = header.iD_REQUEST || "-";

            //Tạo chuỗi HTML cho toàn bộ danh sách
            const container = document.getElementById("_bd_hienchitiet");
            let htmlContent = "";

            data.list.forEach((item, i) => {
                htmlContent += `<tr>
                <td><input type="checkbox" class="form-control itemsmall" /></td>
                <td>${i + 1}</td>
                <td id="mahang_${i}">${item.material_Code}</td>
                <td>${item.material_Name}</td>                  
                <td>${item.brand}</td>
                <td>${item.good_Code}</td>
                <td>${item.account_Code}</td>
                <td>${item.account_Name}</td>
                <td id="hienThiSoLuong_${i}" style="font-weight:bold; color:blue">0</td>
                <td id="slpo_${i}">${item.amount}</td>                  
                <td id="donvi_${i}">${item.unit}</td>
                <td>${item.price}</td>
                <td><input type="number" class="form-control" style="background-color:#d0ffd8ab" id="slthucte_${i}" value="${item.amount}" onblur="_tinhthucte('${i}')" /></td>
                <td><input type="number" class="form-control" style="background-color:#d0ffd8ab" id="dgthucte_${i}" value="${item.price}" onblur="_tinhthucte('${i}')" /></td>
                <td>${item.vat} %</td>
                <td id="tongchiphiold_${i}">${item.total_exchange}</td>
                <td style="background-color:#d0ffd8ab" id="ttthucte_${i}">${item.total_exchange_real}</td>
                <td>${item.po}</td>
            </tr>`;
            });

            container.innerHTML = htmlContent;

            //  chạy vòng lặp để xử lý Select và Event
            data.list.forEach((item, i) => {
                const selectElement = document.getElementById(`khoSelect`);
                const displayElement = document.getElementById(`hienThiSoLuong_${i}`);

                if (item.slk && Array.isArray(item.slk)) {
                    // Đổ dữ liệu vào select
                    item.slk.forEach(kho => {
                        const option = document.createElement('option');
                        option.value = kho.tenkho;
                        option.textContent = kho.tenkho;
                        selectElement.appendChild(option);
                    });

                    // Gán sự kiện thay đổi
                    selectElement.addEventListener('change', function () {
                        const selectedKho = this.value;
                        const khoData = item.slk.find(k => k.tenkho === selectedKho);
                        displayElement.innerText = khoData ? khoData.soluong : "0";
                    });
                    if (item.slk.length > 0) {
                        selectElement.dispatchEvent(new Event('change'));
                    }
                }
            });


            const select = document.getElementById('khoSelect');
            const uniqueOptions = [];
            const values = new Set();

            // Lọc lấy các option không trùng
            for (let option of select.options) {
                if (!values.has(option.value)) {
                    values.add(option.value);
                    uniqueOptions.push({ value: option.value, text: option.text });
                }
            }

            // Xóa sạch select cũ và nạp lại
            select.innerHTML = '';
            uniqueOptions.forEach(opt => {
                const newOpt = new Option(opt.text, opt.value);
                select.add(newOpt);
            });

        })
        .catch(error => {
            console.error('Lỗi khi tải chi tiết:', error);
        });
}
function _tinhthucte(id) {
    var solgPo = parseFloat(document.getElementById("slpo_" + id).innerHTML);
    var solgThucte = parseFloat(document.getElementById("slthucte_" + id).value);
    var hienThiSoLuong = parseFloat(document.getElementById("hienThiSoLuong_" + id).innerHTML);
    if (solgThucte > solgPo) {
        alert("Số lượng thực tế quá số lượng trong đơn !");
        document.getElementById("slthucte_" + id).value = solgPo;
    }
    if (solgThucte > hienThiSoLuong) {
        alert("Số lượng thực tế nhiều hơn số lượng trong kho !");
        document.getElementById("slthucte_" + id).value = "0";
    }
    document.getElementById("ttthucte_" + id).innerHTML = solgThucte * parseFloat(document.getElementById("dgthucte_" + id).value);
}
