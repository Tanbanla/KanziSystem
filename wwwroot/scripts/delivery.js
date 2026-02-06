async function loadListWarehouse() {
    const url = '/Delivery/GetWareHouse';
    const options =
    {
        method: 'POST',
        headers:
        {
            'Content-Type': 'application/json'
        }
    };

    try {
        const response = await fetch(url, options);
        const result = await response.json();
        console.log('Thành công:', result);
        const opt = document.getElementById('IdWarehouse');

        result.forEach((wh) => {
            opt.innerHTML += `<option>${wh}</option>`;
        });
    }
    catch (error) {
        console.error('Error:', error);
    }
}

async function SearchPoDelivery() {
    let GetPO = document.getElementById('poNumber').value;
    let GetDept = document.getElementById('IdDept').value;
    const url = '/Delivery/SearchDataPo';
    const payload =
    {
        PoNumber: GetPO,
        Department: GetDept
    };
    const options =
    {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    };

    try {
        const response = await fetch(url, options);
        const result = await response.json();
        console.log('SearchPoDelivery Data : ', result);
        const opt = document.getElementById('show_kho');

        result.forEach((value) => {
            opt.innerHTML += `<tr><td><input type="checkbox" value="${value.pO_Detail_Id}"/></td><td>${value.pO_Detail_Id}</td><td>${value.id_Goc}</td><td>${value.benxacnhantruoc}</td><td>${value.soPO}</td><td>${value.mahang}</td><td>${value.good_Code}</td><td>${value.tentienganh}</td><td>${value.tentiengviet}</td><td>${value.soluong}</td>`
                + `<td><input type='text' value='${value.luongvethucte}'></input></td><td>${value.luongvekho}</td><td>${value.luongvekhoNgaynhap}</td><td>${value.luongvekhoNguoinhap}</td><td>${value.dovi}</td><td>${value.dongia}</td><td>${value.dieukiengiaohang}</td>`
                + `<td>${value.diadiemgiaohang}</td><td>${value.phuongthucvanchuyen}</td><td>${value.sotien}</td><td>${value.vat}</td><td>${value.maphongyeucau}</td>`
                + `<td>${value.tenphongyeucau}</td><td>${value.ngaygiaohangdukien}</td><td>${value.noigiaodukien}</td>`
                + `<td>${value.thoigianthanhtoan}</td><td>${value.loaitien}</td>`
                + `<td>${value.tygia}</td><td>${value.doisangUSD}</td><td>${value.danhmuc}</td>`
                + `<td>${value.code_Request}</td><td>${value.tinhtrangtokhai}</td>`
                + `<td>${value.tenNCC} </td><td>${value.id_LichsuNhap} </td><td>${value.luongvekhoKhonhap}</td><td>${value.invoice}</td><td>${value.tinhtranghaiquanPO}</td></tr>`;
        });

    } catch (error) {
        console.error('Error : ', error);
    }
}

async function ImportWarehouse() {

    const url = '/Delivery/ImportWarehouse';
    const table = document.getElementById('show_kho');
    //Scan table with row selected will import to warehouse
    const dateInput = document.getElementById('idTimeDelivery').value;
    const warehouseName = document.getElementById('IdWarehouse').value;

    if(dateInput == '' || warehouseName == '--')
    {
        alert("Bạn cần nhập đầy đủ : ngày nhận và kho nhận");
        return;
    }

    const [year, month, day] = dateInput.split('-');
    const formattedDate = `${month}/${day}/${year}`;

    for (let i = 0; i < table.rows.length; i++) {
        let row = table.rows[i];
        const cbxSelect = row.cells[0].querySelector('input[type="checkbox"]');
        const txtLuongVeKho = row.cells[10].querySelector('input[type="text"]');
        if (cbxSelect && cbxSelect.checked) {
            const payload =
            {
                PO_Detail_Id: row.cells[4].innerHTML,
                benXacNhanTruoc: 'STOCK',
                luongvethuctekho: txtLuongVeKho.value,
                NgayNhap: formattedDate,
                KhoNhan: warehouseName
            };
            const options =
            {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            };

            try {
                const response = await fetch(url, options);
                const result = await response.json();
                alert("Nhận hàng PO " + row.cells[4].innerHTML + " (Mã " + row.cells[1].innerHTML + "): " + result);
            }
            catch (error) {
                console.error('Error : ', error);
            }
        }
    }
    alert("Hoàn thành xử lý nhận hàng");
}
