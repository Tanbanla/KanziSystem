function resett() {
    document.getElementById("show_kho_iv").innerHTML = "";
    document.getElementById("poNumber").value = "";
    _Load_PO()
}
function ImportWarehouse() {
    const url = '/Delivery/NhapKhoAction';

    let table = document.getElementById('show_kho_iv');
    //Scan table with row selected will import to warehouse
    // let dateInput = document.getElementById('idTimeDelivery').value;
    //let warehouseName = document.getElementById('IdWarehouse').value;

    //if (dateInput == '' || warehouseName == '--') {
    //    alert("Bạn cần nhập đầy đủ : ngày nhận và kho nhận");
    //    return;
    // }
    let group_code = document.getElementById("IdDept").value;
    let UserName = document.getElementById("us").innerHTML;
    //let [year, month, day] = dateInput.split('-');
    //let formattedDate = `${month}/${day}/${year}`;
    var ngaynhap = document.getElementById("idTimeDelivery").value;
    for (let i = 0; i < table.rows.length; i++) {
        let row = table.rows[i];
        let cbxSelect = row.cells[0].querySelector('input[type="checkbox"]');
        let txtLuongVeKho = row.cells[10].querySelector('input[type="number"]');
        if (cbxSelect && cbxSelect.checked) {
            let payload = {
                PO_Detail_Id: row.cells[4].innerHTML,
                Id_nhapkho: row.cells[1].innerHTML,
                benXacNhanTruoc: 'STOCK',
                luongvethuctekho:  txtLuongVeKho.value,
                NgayNhap: ngaynhap,
                Mahang: row.cells[5].innerHTML,
                Soluong: row.cells[9].innerHTML,
                Group_Code: group_code,
                UserName: UserName
            };
            const options = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            };

            fetch(url, options)
                .then(response => response.json())
                .then(result => {
                  
                })
                .catch(err => console.error(err));
        }
    }
    alert("Nhận hàng thành công !");
    SearchPoDel();
}
function Usingg() {
    const url = '/Delivery/Sudungngay';

    let table = document.getElementById('show_kho_iv');
    //Scan table with row selected will import to warehouse
    // let dateInput = document.getElementById('idTimeDelivery').value;
    //let warehouseName = document.getElementById('IdWarehouse').value;

    //if (dateInput == '' || warehouseName == '--') {
    //    alert("Bạn cần nhập đầy đủ : ngày nhận và kho nhận");
    //    return;
    // }
    let group_code = document.getElementById("IdDept").value;
    let UserName = document.getElementById("us").innerHTML;
    //let [year, month, day] = dateInput.split('-');
    //let formattedDate = `${month}/${day}/${year}`;
    var ngaynhap = document.getElementById("idTimeDelivery").value;
    for (let i = 0; i < table.rows.length; i++) {
        let row = table.rows[i];
        let cbxSelect = row.cells[0].querySelector('input[type="checkbox"]');
        let txtLuongVeKho = row.cells[11].querySelector('input[type="number"]');
        if (cbxSelect && cbxSelect.checked) {
            let payload = {
                PO_Detail_Id: row.cells[4].innerHTML,
                Id_nhapkho: row.cells[1].innerHTML,
                benXacNhanTruoc: 'STOCK',
                luongvethuctekho: txtLuongVeKho ? txtLuongVeKho.value : '',
                NgayNhap: ngaynhap,
                Mahang: row.cells[6].innerHTML,
                Soluong: row.cells[10].innerHTML,
                Group_Code: group_code,
                UserName: UserName
            };
            const options = {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            };

            fetch(url, options)
                .then(response => response.json())
                .then(result => {
                    alert("Sử dụng hàng PO " + row.cells[4].innerHTML + " (Mã " + row.cells[1].innerHTML + "): " + result);
                    SearchPoDel();
                })
                .catch(err => console.error(err));
        }
    }
}
async function SearchPoDel() {     
    let UserName = document.getElementById("us").innerHTML;
    let GetPO = document.getElementById('poNumber').value;
    let GetDept = document.getElementById('IdDept').value;
    let mayeucau = document.getElementById('mayeucau').value;
    let mahang = document.getElementById('mahang').value;
    let Phongbanyeucau = document.getElementById('phongyeucau').value;
    const url = '/Delivery/SearchDataPo';
    let payload =
    {
        PoNumber: GetPO,
        Department: GetDept,
        Mayeucau: mayeucau,
        Mahang: mahang,
        Phongbanyeucau: Phongbanyeucau,
        UserName: UserName
    };
    let options =
    {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    };
    try {
        const response = await fetch(url, options);
        const result = await response.json();

        console.log(result);
        let opt = document.getElementById('show_kho_iv');
        opt.innerHTML = "";
        result.forEach((value) => {
            // 1. Chuyển đổi kiểu dữ liệu
            let solg = parseFloat(value.soluong) || 0;
            let lgvekho = parseFloat(value.luongvekho) || 0;
            let rowClass = "";

            // Lưu ý: Đảm bảo kiểu dữ liệu khi so sánh id để class text-primary hoạt động đúng
          
            let benXacNhan = String(value.benxacnhantruoc || "").trim().toUpperCase();

            //if (solg > lgvekho && benXacNhan === "STOCK" && (String(value.id_Goc).trim() !== "")) {
            //    rowClass = "text-danger";   // Màu đỏ
            //}

            // update
            if (String(value.luongvekho) == "") {
                rowClass = "text-danger";   // Màu đỏ
            }
            else if (solg > lgvekho ) {
                rowClass = "text-primary";  // Màu xanh dương
            }
            else {
                rowClass = "text-dark";     // Màu đen
            }
           
            opt.innerHTML += `<tr class="${rowClass}"><td class="text-center"><input type="checkbox" class="item" value="${value.pO_Detail_Id}" /></td><td>${value.pO_Detail_Id}</td><td>${value.id_Goc}</td><td>${value.benxacnhantruoc}</td><td>${value.soPO}</td><td>${value.mahang}</td><td>${value.good_Code}</td><td>${value.tentienganh}</td><td>${value.tentiengviet}</td><td id="soluong_${value.pO_Detail_Id}">${value.soluong}</td>`
                + `<td><input type='number' style="background-color:lightyellow" class="form-control" id="luongvethucte_${value.pO_Detail_Id}" onblur="Check_luongvethucte('${value.pO_Detail_Id}')" value='${value.luongvekho}'></input></td><td>${value.luongvethucte}</td><td>${value.luongvekhoNgaynhap}</td><td>${value.luongvekhoNguoinhap}</td><td>${value.dovi}</td><td>${value.dongia}</td><td>${value.dieukiengiaohang}</td>`
                + `<td>${value.diadiemgiaohang}</td><td>${value.phuongthucvanchuyen}</td><td>${value.sotien}</td><td>${value.vat}</td><td>${value.maphongyeucau}</td>`
                + `<td>${value.tenphongyeucau}</td><td>${value.ngaygiaohangdukien}</td><td>${value.noigiaodukien}</td>`
                + `<td>${value.thoigianthanhtoan}</td><td>${value.loaitien}</td>`
                + `<td>${value.tygia}</td><td>${value.doisangUSD}</td><td>${value.danhmuc}</td>`
                + `<td>${value.tenNCC} </td><td>${value.id_LichsuNhap} </td><td>${value.luongvekhoKhonhap}</td><td>${value.code_Request}</td><td>${value.invoice}</td><td>${value.tinhtranghaiquanPO}</td></tr>`;
        });
    }
    catch (error) {
        console.error('Error : ', error);
    }
}
function Check_luongvethucte(id) {
    let soluong = document.getElementById("soluong_" + id).innerHTML;
    let luongthucte = document.getElementById("luongvethucte_" + id).value;

    if (parseFloat(luongthucte) > parseFloat(soluong)) {
        document.getElementById("luongvethucte_" + id).value = soluong;
    }
    if (parseFloat(luongthucte) < 0) {
        alert("Không được nhập âm !");
        document.getElementById("luongvethucte_" + id).value = "";
    }
}

async function _Load_PO() {
    let us = document.getElementById('us').innerHTML;
    const params = new URLSearchParams();
    params.append('us', us);

    try {
        // Sử dụng await đồng bộ hóa request để try...catch có thể hoạt động đúng
        const response = await fetch('/Delivery/LoadDataPo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        });

        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const result = await response.json();
        let opt = document.getElementById('show_kho_iv');
        console.log(result);
        // Sử dụng mảng để gom các dòng HTML lại, sau đó nối chuỗi 1 lần
        let htmlRows = result.map((value) => {
            let solg = parseFloat(value.soluong) || 0;
            let lgvekho = parseFloat(value.luongvekho) || 0;
            let rowClass = "";

            // Lưu ý: Đảm bảo kiểu dữ liệu khi so sánh id để class text-primary hoạt động đúng
          
            let benXacNhan = String(value.benxacnhantruoc || "").trim().toUpperCase();

            // 3. Xử lý Id_Goc an toàn (chấp nhận cả giá trị số 0)
          

            // Chạy lại logic If...Else
            //if (solg > lgvekho && benXacNhan === "STOCK" && (String(value.id_Goc).trim() !== "" )) {
            //    rowClass = "text-danger";   // Màu đỏ
            //}
            if (String(value.luongvekho) == "") {
                rowClass = "text-danger";
            }
            else if (solg > lgvekho) {
                rowClass = "text-primary";  // Màu xanh dương
            }
            else {
                rowClass = "text-dark";     // Màu đen
            }

            return `<tr class="${rowClass}">
                <td class="text-center"><input type="checkbox" class="item" value="${value.pO_Detail_Id}" /></td>
                <td>${value.pO_Detail_Id}</td>
                <td>${value.id_Goc}</td>
                <td>${value.benxacnhantruoc}</td>
                <td>${value.soPO}</td>
                <td>${value.mahang}</td>
                <td>${value.good_Code}</td>
                <td>${value.tentienganh}</td>
                <td>${value.tentiengviet}</td>
                <td id="soluong_${value.pO_Detail_Id}">${value.soluong}</td>
                <td><input type='number' style="background-color:lightyellow" class="form-control" id="luongvethucte_${value.pO_Detail_Id}" onblur="Check_luongvethucte('${value.pO_Detail_Id}')" value='${value.luongvekho}'></td>
                <td>${value.luongvethucte}</td>
                <td>${value.luongvekhoNgaynhap}</td>
                <td>${value.luongvekhoNguoinhap}</td>
                <td>${value.dovi}</td>
                <td>${value.dongia}</td>
                <td>${value.dieukiengiaohang}</td>
                <td>${value.diadiemgiaohang}</td>
                <td>${value.phuongthucvanchuyen}</td>
                <td>${value.sotien}</td>
                <td>${value.vat}</td>
                <td>${value.maphongyeucau}</td>
                <td>${value.tenphongyeucau}</td>
                <td>${value.ngaygiaohangdukien}</td>
                <td>${value.noigiaodukien}</td>
                <td>${value.thoigianthanhtoan}</td>
                <td>${value.loaitien}</td>
                <td>${value.tygia}</td>
                <td>${value.doisangUSD}</td>
                <td>${value.danhmuc}</td>
                <td>${value.tenNCC}</td>
                <td>${value.id_LichsuNhap}</td>
                <td>${value.luongvekhoKhonhap}</td>
                <td>${value.code_Request}</td>
                <td>${value.invoice}</td>
                <td>${value.tinhtranghaiquanPO}</td>
                
            </tr>`;
        });

        // Chỉ cập nhật giao diện đúng 1 lần
        opt.innerHTML = htmlRows.join('');

    } catch (error) {
        console.error('Error fetching PO Data: ', error);
    }
}
function ResetWarehouse() {
    const url = '/Delivery/ResetImportRow';

    let table = document.getElementById('show_kho_iv');
    let group_code = document.getElementById("IdDept").value;
    let UserName = document.getElementById("us").innerHTML;
    var ngaynhap = document.getElementById("idTimeDelivery").value;

    let checkedCount = 0;
    let checkedRow = null;

    // 1. Quét để đếm số dòng được chọn và lưu lại dòng đó
    for (let i = 0; i < table.rows.length; i++) {
        let row = table.rows[i];
        let cbxSelect = row.cells[0].querySelector('input[type="checkbox"]');

        if (cbxSelect && cbxSelect.checked) {
            checkedCount++;
            checkedRow = row;
        }
    }

    // 2. Chỉ cho phép xử lý nếu chọn đúng 1 dòng
    if (checkedCount !== 1) {
        alert("Vui lòng chọn đúng 1 dòng hàng để reset!");
        return;
    }
    var invoice = checkedRow.cells[34].innerHTML;
    var tinhtrang = checkedRow.cells[35].innerHTML;
    if (String(invoice) == "") {
        // 3. Thực thi gọi API cho dòng duy nhất được chọn
        let txtLuongVeKho = checkedRow.cells[11].querySelector('input[type="number"]');

        let payload = {
            PO_Detail_Id: checkedRow.cells[4].innerHTML,
            Id_nhapkho: checkedRow.cells[1].innerHTML,
            benXacNhanTruoc: 'STOCK',
            luongvethuctekho: txtLuongVeKho ? txtLuongVeKho.value : '',
            NgayNhap: ngaynhap,
            Mahang: checkedRow.cells[5].innerHTML,
            Soluong: checkedRow.cells[10].innerHTML,
            Group_Code: group_code,
            UserName: UserName,
            Id_Lichsu: checkedRow.cells[31].innerHTML,
            Donvi: checkedRow.cells[14].innerHTML,
            Id_Goc: checkedRow.cells[2].innerHTML,
        };

        const options = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        };

        fetch(url, options)
            .then(response => response.json())
            .then(result => {
                alert("Reset hàng PO " + payload.PO_Detail_Id + " (Mã " + payload.Id_nhapkho + "): " + result);
                SearchPoDel();
            })
            .catch(err => console.error(err));
    }
    else {
        alert("Đã khai báo hải quan, không thể reset");
    }
 
}
