function updateCartCount() {
    const cartData = localStorage.getItem('cart');
    let totalQuantity = 0;

    if (cartData) {
        const cart = JSON.parse(cartData);

        //if (Array.isArray(cart)) {
        //    totalQuantity = cart.reduce((sum, item) => {
        //        return sum + (typeof item.quantity === 'number' && !isNaN(item.quantity) ? item.quantity : 0);
        //    }, 0);
        //}
        if (Array.isArray(cart)) {
            totalQuantity = cart.length;
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

updateCartCount();