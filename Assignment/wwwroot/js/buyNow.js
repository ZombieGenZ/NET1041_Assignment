let selectedId = 0;
let selectedPrice = 0;
function startBuyNow(id, name, price) {
    selectedId = id;
    selectedPrice = price;
    document.getElementById("productName").value = name;
    document.getElementById("productPrice").value = price.toLocaleString("vi-VN", {
        style: "currency",
        currency: "VND",
    });
    const quantity = document.getElementById("quantity");
    if (!quantity) {
        document.getElementById("productPrice").value = price.toLocaleString("vi-VN", {
            style: "currency",
            currency: "VND",
        });
        return;
    }
    if (!isNaN(Number(quantity.value)) && Number(quantity.value) > 0) {
        document.getElementById("productPrice").value = (price * Number(quantity.value)).toLocaleString("vi-VN",
            {
                style: "currency",
                currency: "VND",
            });
    } else {
        document.getElementById("productPrice").value = "Số lượng sản phẩm không hợp lệ";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    const productQuantity = document.getElementById("productQuantity");
    if (!productQuantity) {
        return;
    }
    productQuantity.addEventListener("change",
        () => {
            let quantity = document.getElementById("productQuantity").value;
            if (isNaN(Number(quantity)) || quantity < 1) {
                document.getElementById("productPrice").value = "Số lượng sản phẩm không hợp lệ";
            } else {
                document.getElementById("productPrice").value = (selectedPrice * Number(quantity)).toLocaleString("vi-VN", {
                    style: "currency",
                    currency: "VND",
                });
            }
        });
});

function buyNow() {
    const name = document.getElementById("name").value;
    const email = document.getElementById("email").value;
    const phone = document.getElementById("phone").value;
    const quantity = document.getElementById("productQuantity").value;

    if (!name || !phone || !quantity) {
        let errMsg = [];
        if (!name) errMsg.push("tên người nhận");
        if (!phone) errMsg.push("số điện thoại người nhận");
        showWarningToast(`Vui lòng điền đầy đủ ${errMsg.join(", ")}`, 4000);
        return;
    }

    if (email && !isValidEmail(email)) {
        showWarningToast(`Địa chỉ email người nhận không hợp lệ`, 4000);
        return;
    }

    if (!isSimpleVietnamesePhoneNumber(phone)) {
        showWarningToast(`Số điện thoại người nhận không hợp lệ`, 4000);
        return;
    }

    if (isNaN(quantity) || quantity < 1) {
        showWarningToast(`Số lượng sản phẩm không hợp lệ`, 4000);
        return;
    }

    fetch(`/api/orders`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    Name: name,
                    Email: email ? email : null,
                    Phone: phone,
                    Items: [
                        {
                            ProductId: selectedId,
                            Quantity: Number(quantity)
                        }
                    ]
                })
            })
        .then(response => {
            if (response.status === 422 || response.status === 401) {
                return response.json();
            }

            if (!response.ok) {
                return showErrorToast("Lỗi khi mua hàng. Vui lòng thử lại sau.", 4000);
            }

            return response.json();
        })
        .then(data => {
            if (data.code === "INPUT_DATA_ERROR") {
                showWarningToast(data.message, 4000);
                return;
            }

            if (data.code === "ORDER_SUCCESS") {
                showSuccessToast(data.message, 4000);
                const buyModal = document.getElementById(
                    "buyModal"
                );
                const modal = bootstrap.Modal.getInstance(buyModal);
                if (modal) {
                    modal.hide();
                } else {
                    new bootstrap.Modal(createModal).hide();
                }
                return;
            } else {
                showErrorToast(data.message, 4000);
            }
        });
}

function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isSimpleVietnamesePhoneNumber(phoneNumber) {
    const regex = /^0\d{9}$/;

    return regex.test(phoneNumber);
}

/**
* Hiển thị thông báo Thành công (màu xanh lá gradient).
* @param {string} message - Nội dung thông báo.
* @param {number} [duration=3000] - Thời gian hiển thị (mili giây).
*/
function showSuccessToast(message, duration = 3000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #00b099, #96c93d)", // Màu xanh lá gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}

/**
 * Hiển thị thông báo Cảnh báo (màu cam/vàng gradient).
 * @param {string} message - Nội dung thông báo.
 * @param {number} [duration=5000] - Thời gian hiển thị (mili giây).
 */
function showWarningToast(message, duration = 5000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #ffc400, #ff8c00)", // Màu cam/vàng gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}

/**
 * Hiển thị thông báo Lỗi (màu đỏ gradient).
 * @param {string} message - Nội dung thông báo.
 * @param {number} [duration=7000] - Thời gian hiển thị (mili giây).
 */
function showErrorToast(message, duration = 7000) {
    Toastify({
        text: message,
        duration: duration,
        gravity: "top",
        position: "right",
        backgroundColor: "linear-gradient(to right, #ff5f6d, #ffc371)", // Màu đỏ/cam gradient
        stopOnFocus: true,
        close: true
    }).showToast();
}
