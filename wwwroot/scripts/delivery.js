function resett() {
    document.getElementById("show_kho_iv").innerHTML = "";
    document.getElementById("poNumber").value = "";
    _Load_PO()
}
function ImportWarehouse() {
    const url = '/Delivery/ImportWarehouse';

    let table = document.getElementById('show_kho_iv');
    //Scan table with row selected will import to warehouse
   // let dateInput = document.getElementById('idTimeDelivery').value;
    //let warehouseName = document.getElementById('IdWarehouse').value;

    //if (dateInput == '' || warehouseName == '--') {
    //    alert("Bạn cần nhập đầy đủ : ngày nhận và kho nhận");
    //    return;
    //}
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
                    alert("Nhận hàng PO " + row.cells[4].innerHTML + " (Mã " + row.cells[1].innerHTML + "): " + result);
                    SearchPoDel();
                })
                .catch(err => console.error(err));
        }
    }
}

async function SearchPoDel() {
    let UserName = document.getElementById("us").value;
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
        console.log('SearchPoDelivery Data : ', result);
        let opt = document.getElementById('show_kho_iv');
        opt.innerHTML = "";
        result.forEach((value) => {
            // 1. Chuyển đổi kiểu dữ liệu
            var solg = parseFloat(value.soluong) || 0;
            var lgvekho = parseFloat(value.luongvekho) || 0;

            // 2. Xác định class dựa trên điều kiện
            let rowClass = "";
            if (solg > lgvekho) {
                rowClass = "text-primary";
            }
            else if (solg < lgvekho) {
                rowClass = "text-danger";
            }
            opt.innerHTML += `<tr class="${rowClass}"><td class="text-center"><input type="checkbox" class="item" value="${value.pO_Detail_Id}" /></td><td>${value.pO_Detail_Id}</td><td>${value.id_Goc}</td><td>${value.benxacnhantruoc}</td><td>${value.soPO}</td><td>${value.code_Request}</td><td>${value.mahang}</td><td>${value.good_Code}</td><td>${value.tentienganh}</td><td>${value.tentiengviet}</td><td id="soluong_${value.pO_Detail_Id}">${value.soluong}</td>`
                + `<td><input type='number' style="background-color:lightyellow" class="form-control" id="luongvethucte_${value.pO_Detail_Id}" onblur="Check_luongvethucte('${value.pO_Detail_Id}')" value='${value.luongvekho}'></input></td><td>${value.luongvekhoNgaynhap}</td><td>${value.luongvekhoNguoinhap}</td><td>${value.dovi}</td><td>${value.dongia}</td><td>${value.dieukiengiaohang}</td>`
                + `<td>${value.diadiemgiaohang}</td><td>${value.phuongthucvanchuyen}</td><td>${value.sotien}</td><td>${value.vat}</td><td>${value.maphongyeucau}</td>`
                + `<td>${value.tenphongyeucau}</td><td>${value.ngaygiaohangdukien}</td><td>${value.noigiaodukien}</td>`
                + `<td>${value.thoigianthanhtoan}</td><td>${value.loaitien}</td>`
                + `<td>${value.tygia}</td><td>${value.doisangUSD}</td><td>${value.danhmuc}</td>`
                + `<td>${value.tenNCC} </td><td>${value.id_LichsuNhap} </td><td>${value.luongvekhoKhonhap}</td><td>${value.invoice}</td><td>${value.tinhtranghaiquanPO}</td></tr>`;
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
}

async function _Load_PO() {

    let us = document.getElementById('us').innerHTML;
    const params = new URLSearchParams();
    params.append('us', us);
    try {
        fetch('/Delivery/LoadDataPo', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: params.toString()
        })
            .then(response => response.json())
            .then(result => {
                let opt = document.getElementById('show_kho_iv');
                opt.innerHTML = "";
                result.forEach((value) => {

                    // 1. Chuyển đổi kiểu dữ liệu
                    var solg = parseFloat(value.soluong) || 0;
                    var lgvekho = parseFloat(value.luongvekho) || 0;

                    // 2. Xác định class dựa trên điều kiện
                    let rowClass = "";
                    if (solg > lgvekho) {
                        rowClass = "text-primary";
                    }
                    else if (solg < lgvekho) {
                        rowClass = "text-danger";
                    }
                    opt.innerHTML += `<tr class="${rowClass}"><td class="text-center"><input type="checkbox" class="item" value="${value.pO_Detail_Id}" /></td><td>${value.pO_Detail_Id}</td><td>${value.id_Goc}</td><td>${value.benxacnhantruoc}</td><td>${value.soPO}</td><td>${value.code_Request}</td><td>${value.mahang}</td><td>${value.good_Code}</td><td>${value.tentienganh}</td><td>${value.tentiengviet}</td>
                           <td id="soluong_${value.pO_Detail_Id}">${value.soluong}</td>`
                        + `<td><input type='number' style="background-color:lightyellow" class="form-control" id="luongvethucte_${value.pO_Detail_Id}" onblur="Check_luongvethucte('${value.pO_Detail_Id}')" value='${value.luongvekho}'></input></td>
                          <td>${value.luongvekhoNgaynhap}</td><td>${value.luongvekhoNguoinhap}</td><td>${value.dovi}</td><td>${value.dongia}</td><td>${value.dieukiengiaohang}</td>`
                        + `<td>${value.diadiemgiaohang}</td><td>${value.phuongthucvanchuyen}</td><td>${value.sotien}</td><td>${value.vat}</td><td>${value.maphongyeucau}</td>`
                        + `<td>${value.tenphongyeucau}</td><td>${value.ngaygiaohangdukien}</td><td>${value.noigiaodukien}</td>`
                        + `<td>${value.thoigianthanhtoan}</td><td>${value.loaitien}</td>`
                        + `<td>${value.tygia}</td><td>${value.doisangUSD}</td><td>${value.danhmuc}</td>`
                        + `<td>${value.tenNCC} </td><td>${value.id_LichsuNhap} </td><td>${value.luongvekhoKhonhap}</td><td>${value.invoice}</td><td>${value.tinhtranghaiquanPO}</td></tr>`;
                    
                   
                });
            })
            .catch(err => console.error(err)); 
    }
    catch (error) {
        console.error('Error : ', error);
    }
}
