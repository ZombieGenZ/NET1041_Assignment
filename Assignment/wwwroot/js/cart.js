document.addEventListener("DOMContentLoaded", () => {
  LoadData();
});

function LoadData() {
  const listDom = document.getElementById("CartData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="5">Không có dử liệu nào phù hợp</td></tr>';
  let localCartData = localStorage.getItem("cart");

  const items = [];
  const ids = JSON.parse(localCartData || "[]");
  const itemQuantities = new Map();
  for (let item of ids) {
    items.push(item.id);
    itemQuantities.set(item.id, item.quantity);
  }
  const body = JSON.stringify({
    ProductIds: items,
  });
  fetch(`/api/products/get-by-id`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: body,
  })
    .then((response) => {
      if (response.status === 422 || response.status === 401) {
        return response.json();
      }

      if (!response.ok) {
        return showErrorToast(
          "Lỗi khi tải dử liệu. Vui lòng thử lại sau.",
          4000
        );
      }

      return response.json();
    })
    .then((data) => {
      if (data.length > 0) {
        let list = "";
        let listData = [];
        let newCart = [];
        for (var item of data) {
            const quantity = itemQuantities.get(item.id) || 1;
            newCart.push({
                id: item.id,
                quantity: quantity,
            });
          list += `
                        <tr>
                            <td><a href="/products/${
                              item.path
                            }" class="text-decoration-none text-black" target="_blank">${
            item.name
          }</a></td>
                            <td>${
                              item.discount > 0
                                ? `${(
                                    item.price -
                                    (item.price * item.discount) / 100
                                  ).toLocaleString("vi-VN", {
                                    style: "currency",
                                    currency: "VND",
                                  })} <del class="text-secondary">${item.price.toLocaleString(
                                    "vi-VN",
                                    {
                                      style: "currency",
                                      currency: "VND",
                                    }
                                  )}</del>`
                                : `${item.price.toLocaleString("vi-VN", {
                                    style: "currency",
                                    currency: "VND",
                                  })}`
                            }</td>
                            <td>
                              <div class="d-flex gap-2">
                                <button class="btn btn-primary px-3" onclick="reduce(${
                                  item.id
                                }, ${item.price}, ${item.discount})">-</button>
                                <input type="number" class="form-control" id="quantity-${
                                  item.id
                                }" style="max-width: 250px" value="${quantity}">
                                <button class="btn btn-primary px-3" onclick="increase(${
                                  item.id
                                }, ${item.price}, ${item.discount})">+</button>
                              </div></td>
                            <td><span id="totalPrice-${item.id}">${
            item.discount > 0
              ? `${(
                  (item.price - (item.price * item.discount) / 100) *
                  quantity
                ).toLocaleString("vi-VN", {
                  style: "currency",
                  currency: "VND",
                })} <del class="text-secondary">${(
                  item.price * quantity
                ).toLocaleString("vi-VN", {
                  style: "currency",
                  currency: "VND",
                })}</del>`
              : `${(item.price * quantity).toLocaleString("vi-VN", {
                  style: "currency",
                  currency: "VND",
                })}`
          }</span></td>
                            <td class="text-end button-cell">
                                <button class="btn btn-success" onclick='startBuyNow(${
                                  item.id
                                }, "${item.name}", ${
            item.price
          })' data-bs-toggle="modal" data-bs-target="#buyModal">Mua</button>
                                <button class="btn btn-danger" onclick="startDelete(${
                                  item.id
                                })" data-bs-toggle="modal" data-bs-target="#deleteModal">Xóa</button>
                            </td>
                        </tr>
                    `;
            listData.push(item);
          }
          localStorage.setItem("cart", JSON.stringify(newCart));
          updateCartCount();
          document.getElementById("buyAll").innerHTML = `<button class="btn btn-success float-end" data-bs-toggle="modal" data-bs-target="#buyAllModal">Thanh toán tất cả :D</button>`
        listDom.innerHTML = list;
        for (var item2 of listData) {
          addQuantityChangeListener(item2);
        }
      } else {
        localStorage.setItem("cart", JSON.stringify("[]"));
        listDom.innerHTML =
          '<tr><td class="text-center" colspan="5">Không có dử liệu nào phù hợp</td></tr>';
      }
    });
}

function addQuantityChangeListener(item) {
  document
    .getElementById(`quantity-${item.id}`)
    .addEventListener("change", (e) => {
      document.getElementById(`totalPrice-${item.id}`).innerHTML = `${
        item.discount > 0
          ? `${(
              (item.price - (item.price * item.discount) / 100) *
              e.target.value
            ).toLocaleString("vi-VN", {
              style: "currency",
              currency: "VND",
            })} <del class="text-secondary">${(
              item.price * e.target.value
            ).toLocaleString("vi-VN", {
              style: "currency",
              currency: "VND",
            })}</del>`
          : `${(item.price * e.target.value).toLocaleString("vi-VN", {
              style: "currency",
              currency: "VND",
            })}`
      }`;
    });
}

document.addEventListener("DOMContentLoaded", () => {
  document.getElementById("productQuantity").addEventListener("change", () => {
    let quantity = document.getElementById("productQuantity").value;
    if (isNaN(Number(quantity)) || quantity < 1) {
      document.getElementById("productPrice").value =
        "Số lượng sản phẩm không hợp lệ";
    } else {
      document.getElementById("productPrice").value = (
        selectedPrice * Number(quantity)
      ).toLocaleString("vi-VN", {
        style: "currency",
        currency: "VND",
      });
    }
  });
});

function increase(id, price, discount) {
  document.getElementById(`quantity-${id}`).value++;

  const quantityDom = document.getElementById(`quantity-${id}`).value;
  const quantity = Number(quantityDom);

  document.getElementById(`totalPrice-${id}`).innerHTML = `${
    discount > 0
      ? `${((price - (price * discount) / 100) * quantity).toLocaleString(
          "vi-VN",
          {
            style: "currency",
            currency: "VND",
          }
        )} <del class="text-secondary">${(price * quantity).toLocaleString(
          "vi-VN",
          {
            style: "currency",
            currency: "VND",
          }
        )}</del>`
      : `${(price * quantity).toLocaleString("vi-VN", {
          style: "currency",
          currency: "VND",
        })}`
  }`;

  const localCartData = localStorage.getItem("cart");
  let cart = localCartData ? JSON.parse(localCartData) : [];

  const existingItemIndex = cart.findIndex((item) => item.id === id);

  if (existingItemIndex !== -1) {
    cart[existingItemIndex].quantity++;
  }

  localStorage.setItem("cart", JSON.stringify(cart));
}

function reduce(id, price, discount) {
  const quantity = document.getElementById(`quantity-${id}`).value;
  if (quantity > 1) {
    document.getElementById(`quantity-${id}`).value = quantity - 1;

    const quantityDom = document.getElementById(`quantity-${id}`).value;
    const quantity2 = Number(quantityDom);

    document.getElementById(`totalPrice-${id}`).innerHTML = `${
      discount > 0
        ? `${((price - (price * discount) / 100) * quantity2).toLocaleString(
            "vi-VN",
            {
              style: "currency",
              currency: "VND",
            }
          )} <del class="text-secondary">${(price * quantity2).toLocaleString(
            "vi-VN",
            {
              style: "currency",
              currency: "VND",
            }
          )}</del>`
        : `${(price * quantity2).toLocaleString("vi-VN", {
            style: "currency",
            currency: "VND",
          })}`
    }`;

    const localCartData = localStorage.getItem("cart");
    let cart = localCartData ? JSON.parse(localCartData) : [];

    const existingItemIndex = cart.findIndex((item) => item.id === id);

    if (existingItemIndex !== -1) {
      cart[existingItemIndex].quantity--;
    }

    localStorage.setItem("cart", JSON.stringify(cart));
  }
}

let selectedId = 0;
let selectedPrice = 0;
function startBuyNow(id, name, price) {
  selectedId = id;
  selectedPrice = price;
  document.getElementById("productName").value = name;
  document.getElementById("productPrice").value = price.toLocaleString(
    "vi-VN",
    {
      style: "currency",
      currency: "VND",
    }
  );
  const quantity = document.getElementById(`quantity-${id}`);
  if (!quantity) {
    document.getElementById("productPrice").value = price.toLocaleString(
      "vi-VN",
      {
        style: "currency",
        currency: "VND",
      }
    );
    return;
  }

  if (!isNaN(Number(quantity.value)) && Number(quantity.value) > 0) {
    document.getElementById("productPrice").value = (
      price * Number(quantity.value)
    ).toLocaleString("vi-VN", {
      style: "currency",
      currency: "VND",
    });
    document.getElementById("productQuantity").value = quantity.value;
  } else {
    document.getElementById("productPrice").value =
      "Số lượng sản phẩm không hợp lệ";
  }
}

document.addEventListener("DOMContentLoaded", () => {
  document.getElementById("productQuantity").addEventListener("change", () => {
    let quantity = document.getElementById("productQuantity").value;
    if (isNaN(Number(quantity)) || quantity < 1) {
      document.getElementById("productPrice").value =
        "Số lượng sản phẩm không hợp lệ";
    } else {
      document.getElementById("productPrice").value = (
        selectedPrice * Number(quantity)
      ).toLocaleString("vi-VN", {
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

  fetch(`/api/orders`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: name,
      Email: email ? email : null,
      Phone: phone,
      Items: [
        {
          ProductId: selectedId,
          Quantity: Number(quantity),
        },
      ],
    }),
  })
    .then((response) => {
      if (response.status === 422 || response.status === 401) {
        return response.json();
      }

      if (!response.ok) {
        return showErrorToast("Lỗi khi mua hàng. Vui lòng thử lại sau.", 4000);
      }

      return response.json();
    })
    .then((data) => {
      if (data.code === "INPUT_DATA_ERROR") {
        showWarningToast(data.message, 4000);
        return;
      }

      if (data.code === "ORDER_SUCCESS") {
        showSuccessToast(data.message, 4000);
        const buyModal = document.getElementById("buyModal");
        const modal = bootstrap.Modal.getInstance(buyModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        const localCartData = localStorage.getItem("cart");
        let cart = localCartData ? JSON.parse(localCartData) : [];

        const existingItemIndex = cart.findIndex(
          (item) => item.id === selectedId
        );

        if (existingItemIndex !== -1) {
          cart.splice(existingItemIndex, 1);
        }

        localStorage.setItem("cart", JSON.stringify(cart));
        setTimeout(() => location.reload(), 1500);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function startDelete(id) {
  selectedId = id;
}

function del() {
  const localCartData = localStorage.getItem("cart");
  let cart = localCartData ? JSON.parse(localCartData) : [];

  const existingItemIndex = cart.findIndex((item) => item.id === selectedId);

  if (existingItemIndex !== -1) {
    cart.splice(existingItemIndex, 1);
  }

  localStorage.setItem("cart", JSON.stringify(cart));
  location.reload();
}

function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

function isSimpleVietnamesePhoneNumber(phoneNumber) {
  const regex = /^0\d{9}$/;

  return regex.test(phoneNumber);
}

function buyAll() {
    const name = document.getElementById("buyall_name").value;
    const email = document.getElementById("buyall_email").value;
    const phone = document.getElementById("buyall_phone").value;

    if (!name || !phone) {
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

    let localCartData = localStorage.getItem("cart");
    const ids = JSON.parse(localCartData || "[]");
    const items = [];
    for (var i of ids) {
        items.push({
            ProductId: i.id,
            Quantity: i.quantity
        });
    }
    const body = JSON.stringify({
        Name: name,
        Email: email,
        Phone: phone,
        Items: items,
    });

    fetch("/api/orders", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: body,
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
                "buyAllModal"
            );
            const modal = bootstrap.Modal.getInstance(buyModal);
            if (modal) {
                modal.hide();
            } else {
                new bootstrap.Modal(createModal).hide();
            }
            localStorage.removeItem("cart");
            setTimeout(() => location.reload(), 1000);
            return;
        } else {
            showErrorToast(data.message, 4000);
        }
    });
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
