(function () {
    'use strict';
    var form = document.getElementById('materialForm');
    var submitBtn = document.getElementById('submitBtn');
    var btnText = document.getElementById('btnText');
    var btnSpinner = document.getElementById('btnSpinner');

    form.addEventListener('submit', function (event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        } else {
            // Show spinner and let form submit (or perform AJAX here)
            btnText.textContent = 'Saving...';
            btnSpinner.classList.remove('d-none');
            // If you want to prevent real submit for demo, uncomment:
            // event.preventDefault();
            const dto = {
                Material_Code: document.getElementById("MaterialCode").value, // Required
                Material_Name_VN: document.getElementById("MaterialNameVN").value, // Required
                Material_Name_EN: document.getElementById("MaterialNameEN").value,
                Material_Name_JP: document.getElementById("MaterialNameJP").value,
                Account_Code: document.getElementById("AccountCode").value,
                Account_Name_EN: document.getElementById("AccountNameEN").value,
                Account_Name_VN: document.getElementById("AccountNameVN").value,
                Unit: document.getElementById("Unit").value,
                Unit_Note: document.getElementById("UnitNote").value,
                Price: parseFloat(document.getElementById("Price").value) || null,
                Currency: document.getElementById("Currency").value,
                Group_Code: document.getElementById("GroupCode").value,
                GoodKind: document.getElementById("GoodKind").value,
                Category_VN: document.getElementById("Category_VN").value,
                Category_JP: document.getElementById("Category_JP").value,
                Category_EN: document.getElementById("Category_EN").value,
                Shape: document.getElementById("Shape").value,
                Material: document.getElementById("Material").value,
                Composition: document.getElementById("Composition").value,

                Composition: document.getElementById("Composition").value,
                Dimension: document.getElementById("Dimension").value,
                UsedFor: document.getElementById("UsedFor").value,
                Purpose: document.getElementById("Purpose").value,
            };

            fetch('/Material/Create_Material', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(dto)
            })
                .then(
                    response => { return response.json() }
                )
                .then(res => {

                    console.log("response", res)
                    if (res.success) {
                        // Close modal, reload page or update table
                        $('#createModal').modal('hide');
                        alert('Thêm mới thành công!');
                        location.reload();
                    }
                    else {
                        alert(res.message);
                    }
                })
                .catch((error) => {
                   
                });
        }
        form.classList.add('was-validated');
    }, false);
})();


document.getElementById('fileInput').addEventListener('change', handleFileSelect);
function handleFileSelect(event) {
    const file = event.target.files[0];
    if (!file) return;

    const allowedTypes = ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet', 'application/vnd.ms-excel'];
    if (!allowedTypes.includes(file.type)) {
        alert('Please select a valid Excel file (.xlsx or .xls).');
        return;
    }

    const formData = new FormData();
    formData.append('file', file);

    fetch('/Material/ImportMaterials', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message);
                location.reload(); // Or update the UI
            } else {
                alert('Import failed: ' + data.message);
            }
        })
        .catch(error => {
            alert('An error occurred during import.');
        });
}

function downloadSample() {
    const link = document.createElement('a');
    link.href = '/template/Template_Import_Material.xlsx'; 
    link.download = 'Template_Import_Material.xlsx';
    link.click();
}
