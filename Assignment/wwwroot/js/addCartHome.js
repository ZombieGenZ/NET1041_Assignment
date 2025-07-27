function addToCart(id) {
    const quantity = 1;

    if (quantity <= 0) {
        showWarningToast("Số lượng sản phẩm phải lớn hơn 0", 4000);
        return;
    }

    const dataToCart = {
        id: id,
        quantity: parseInt(quantity, 10),
    };

    const localCartData = localStorage.getItem("cart");
    let cart = localCartData ? JSON.parse(localCartData) : [];

    const existingItemIndex = cart.findIndex((item) => item.id === dataToCart.id);

    if (existingItemIndex !== -1) {
        cart[existingItemIndex].quantity += dataToCart.quantity;
    } else {
        cart.push(dataToCart);
    }

    localStorage.setItem("cart", JSON.stringify(cart));

    showSuccessToast("Thêm sản phẩm vào giỏ hàng thành công", 4000);
    updateCartCount();
}

function updateCartCount() {
    const cartData = localStorage.getItem('cart');
    let totalQuantity = 0;

    if (cartData) {
        const cart = JSON.parse(cartData);

        if (Array.isArray(cart)) {
            totalQuantity = cart.reduce((sum, item) => {
                return sum + (typeof item.quantity === 'number' && !isNaN(item.quantity) ? item.quantity : 0);
            }, 0);
        }
    }

    const cart1 = document.getElementById('cart-count');
    const cart2 = document.getElementById('cart-count-2');
    if (cart1) {
        cart1.textContent = new Intl.NumberFormat('vi-VN').format(totalQuantity);
    } else if (cart2) {
        cart2.textContent = new Intl.NumberFormat('vi-VN').format(totalQuantity);
    }
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
        close: true,
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
        close: true,
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
        close: true,
    }).showToast();
}