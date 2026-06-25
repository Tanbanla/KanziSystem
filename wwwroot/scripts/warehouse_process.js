function _load_wh() {
    const url = '/ipcs/WAREHOUSE/Load_Warehouse';
    const options = {
        method: 'POST',
    };
    fetch(url, options)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById("").innerHTML = "";
            data.forEach(wh => {
           
                document.getElementById("").innerHTML += ``;
            });
            console.log(data);
        })
        .catch(error => {
            console.error('There was a problem with the fetch operation:', error);
        });
}
