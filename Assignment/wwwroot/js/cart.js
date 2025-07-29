let orderId = "";

let connection = null;
window.addEventListener("load", function () {
  initConnection();
});
window.addEventListener("beforeunload", function () {
  if (connection) {
    connection.stop();
  }
});

function initConnection() {
  connection = new signalR.HubConnectionBuilder()
    .withUrl("/realtime-hub")
    .withAutomaticReconnect()
    .build();

  connection.onreconnected(() => {
    // xử lý sự kiện khi kết nối lại thành công
  });

  connection.onclose(() => {
    // xử lý sự kiện khi mất kết nối
  });
}

async function connectPaymentRealtime() {
  try {
    await connection.start();

    if (!connection) {
      return;
    }

    await connection.invoke("ConnectPaymentTracking", orderId);

    connection.off("Paid");

    connection.on("Paid", (data) => {
      if (data.ok) {
        document.getElementById("status_payment").innerHTML = `
               
                                        <div class="status-section">
                                        Trạng thái: Đã thanh toán&nbsp;
                                        <span style="color: green; font-size: 1.2em;"><i class="fa-solid fa-circle-check"></i></span>
                                    </div>
                `;
        showSuccessToast("Thanh toán thành công!");
      }
    });
  } catch (e) {
    console.log(e);
  }
}

async function leavePaymentRealtime() {
  try {
    await connection.stop();
  } catch (e) {
    console.log(e);
  }
}

document.addEventListener("DOMContentLoaded", function () {
  var paymentModal = document.getElementById("paymentModal");
  paymentModal.addEventListener("hidden.bs.modal", function () {
    leavePaymentRealtime();
  });
});

document.addEventListener("DOMContentLoaded", () => {
  LoadData();
});

let listAll = [];

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
                        <td><input class="form-check-input" type="checkbox" id="check-${
                          item.id
                        }" onclick="checkItem()"></td>
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
                                }, "${item.name}", ${item.price}, ${
            item.discount
          })' data-bs-toggle="modal" data-bs-target="#buyModal">Mua</button>
                                <button class="btn btn-danger" onclick="startDelete(${
                                  item.id
                                })" data-bs-toggle="modal" data-bs-target="#deleteModal">Xóa</button>
                            </td>
                        </tr>
                    `;
          listData.push(item);
          listAll.push(item);
        }
        localStorage.setItem("cart", JSON.stringify(newCart));
        updateCartCount();
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

function updateBuyAllButtonVisibility() {
  const checkboxes = document.querySelectorAll(
    'input[type="checkbox"][id^="check-"]'
  );
  const buyAllBtnContainer = document.getElementById("buyAll");
  const checkedCount = Array.from(checkboxes).filter((cb) => cb.checked).length;

  if (checkedCount > 0) {
    buyAllBtnContainer.innerHTML = `<button class="btn btn-success float-end" data-bs-toggle="modal" data-bs-target="#buyModal" onclick="startBuyAll()">Thanh toán ${checkedCount} sản phẩm</button>`;
  } else {
    buyAllBtnContainer.innerHTML = "";
  }
}

function checkAll() {
  document.getElementById("checkAll").addEventListener("change", (e) => {
    const checkboxes = document.querySelectorAll(
      'input[type="checkbox"][id^="check-"]'
    );
    checkboxes.forEach((checkbox) => {
      checkbox.checked = e.target.checked;
    });
    updateBuyAllButtonVisibility();
  });
}

function checkItem() {
  const checkboxes = document.querySelectorAll(
    'input[type="checkbox"][id^="check-"]'
  );
  checkboxes.forEach((checkbox) => {
    checkbox.addEventListener("change", () => {
      const allChecked = Array.from(checkboxes).every(
        (cb) => cb.checked === true
      );
      document.getElementById("checkAll").checked = allChecked;
      updateBuyAllButtonVisibility();
    });
  });
}

function getAllCheckedItems() {
  const checkedItems = [];
  const checkboxes = document.querySelectorAll(
    'input[type="checkbox"][id^="check-"]'
  );
  checkboxes.forEach((checkbox) => {
    if (checkbox.checked) {
      const itemId = checkbox.id.replace("check-", "");
      const item = listAll.find((item) => item.id === Number(itemId));
      if (item) {
        const quantityInput = document.getElementById(`quantity-${itemId}`);
        const quantity = quantityInput ? Number(quantityInput.value) : 1;
        checkedItems.push({
          ...item,
          quantity: quantity,
        });
      }
    }
  });
  return checkedItems;
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
function startBuyNow(id, name, price, discount) {
  selectedId = id;
  selectedPrice = price - (price * discount) / 100;
  let priceHtml = "";
  let quantityHtml = "";
  document.getElementById("buyBtn").removeEventListener("click", buyNow);
  document.getElementById("buyBtn").removeEventListener("click", buyAll);
  document.getElementById("buyBtn").addEventListener("click", buyNow);
  const quantity = document.getElementById(`quantity-${id}`);
  if (!quantity) {
    priceHtml = (price - (price * discount) / 100).toLocaleString("vi-VN", {
      style: "currency",
      currency: "VND",
    });
    quantityHtml = 1;

    document.getElementById("productInfo").innerHTML = `
                            <h1 class="h1">Thông tin sản phẩm</h1>
                            <div class="mb-3">
                                <label for="productName" class="form-label">Tên sản phẩm</label>
                                <input type="text" class="form-control" value="${name}" id="productName" readonly="">
                            </div>
                            <div class="mb-3">
                                <label for="productQuantity" class="form-label">Số lượng sản phẩm</label>
                                <input type="number" class="form-control" value="${quantityHtml}" id="productQuantity" placeholder="ví dụ: 999" value="1">
                            </div>
                            <div class="mb-3">
                                <label for="productPrice" class="form-label">Tổng hóa đơn</label>
                                <input type="text" class="form-control" value="${priceHtml}" id="productPrice" readonly="">
                            </div>
  `;
    document
      .getElementById("productQuantity")
      .addEventListener("change", () => {
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
    return;
  }

  if (!isNaN(Number(quantity.value)) && Number(quantity.value) > 0) {
    priceHtml = (
      Number(quantity.value) *
      (price - (price * discount) / 100)
    ).toLocaleString("vi-VN", {
      style: "currency",
      currency: "VND",
    });
    quantityHtml = quantity.value;

    document.getElementById("productInfo").innerHTML = `
                            <h1 class="h1">Thông tin sản phẩm</h1>
                            <div class="mb-3">
                                <label for="productName" class="form-label">Tên sản phẩm</label>
                                <input type="text" class="form-control" value="${name}" id="productName" readonly="">
                            </div>
                            <div class="mb-3">
                                <label for="productQuantity" class="form-label">Số lượng sản phẩm</label>
                                <input type="number" class="form-control" value="${quantityHtml}" id="productQuantity" placeholder="ví dụ: 999" value="1">
                            </div>
                            <div class="mb-3">
                                <label for="productPrice" class="form-label">Tổng hóa đơn</label>
                                <input type="text" class="form-control" value="${priceHtml}" id="productPrice" readonly="">
                            </div>
  `;
    document
      .getElementById("productQuantity")
      .addEventListener("change", () => {
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
  } else {
    priceHtml = "Số lượng sản phẩm không hợp lệ";
    quantityHtml = 1;
    document.getElementById("productInfo").innerHTML = `
                            <h1 class="h1">Thông tin sản phẩm</h1>
                            <div class="mb-3">
                                <label for="productName" class="form-label">Tên sản phẩm</label>
                                <input type="text" class="form-control" value="${name}" id="productName" readonly="">
                            </div>
                            <div class="mb-3">
                                <label for="productQuantity" class="form-label">Số lượng sản phẩm</label>
                                <input type="number" class="form-control" value="${quantityHtml}" id="productQuantity" placeholder="ví dụ: 999" value="1">
                            </div>
                            <div class="mb-3">
                                <label for="productPrice" class="form-label">Tổng hóa đơn</label>
                                <input type="text" class="form-control" value="${priceHtml}" id="productPrice" readonly="">
                            </div>
  `;
    document
      .getElementById("productQuantity")
      .addEventListener("change", () => {
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
  }
}

function buyNow() {
  const name = document.getElementById("name").value;
  const email = document.getElementById("email").value;
  const phone = document.getElementById("phone").value;
  const quantity = document.getElementById("productQuantity").value;
  const voucher = document.getElementById("voucher").value;

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

  if (!isValidLongitude(selectedLng) || !isValidLatitude(selectedLat)) {
    showWarningToast(`Vị trí nhận hàng không hợp lệ`, 4000);
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
      Longitude: Number(getSelectedLocation()?.lng),
      Latitude: Number(getSelectedLocation()?.lat),
      Voucher: voucher ? voucher : null,
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
        LoadData();
        document.getElementById(
          "paymentQrCode"
        ).src = `https://qr.sepay.vn/img?acc=${data.accountNo}&bank=${data.bankId}&amount=${data.totalBill}&des=${data.orderId}&template=compact`;
        document.getElementById("bank").textContent = data.bankId;
        document.getElementById("account_name").textContent = data.accountName;
        document.getElementById("account_no").textContent = data.accountNo;
        document.getElementById(
          "bill"
        ).textContent = `${data.totalBill.toLocaleString("vi-VN")} ₫`;
        document.getElementById("orderId").textContent = data.orderId;
        document.getElementById("orderId2").textContent = data.orderId;
        document.getElementById(
          "totalPrice"
        ).textContent = `${data.totalPrice.toLocaleString("vi-VN")}₫`;
        if (data.discount > 0) {
          document.getElementById(
            "discount"
          ).innerHTML = `<p>Giảm giá <span class="float-end text-danger">-${data.discount.toLocaleString(
            "vi-VN"
          )}₫</span></p>`;
        }
        document.getElementById(
          "totalBill"
        ).textContent = `${data.totalBill.toLocaleString("vi-VN")}₫`;
        document.getElementById("fee").textContent = `${data.fee.toLocaleString(
          "vi-VN"
        )}₫`;
        document.getElementById("vat").textContent = `${data.vat.toLocaleString(
          "vi-VN"
        )}₫`;
        let listProduct = ``;
        for (const item of data.productList) {
          listProduct += `<li class="list-group-item">${
            item.name
          } <span class="float-end">${
            item.discount > 0
              ? `<span>${(
                  item.quantity *
                  (item.price - (item.price / 100) * item.discount)
                ).toLocaleString(
                  "vi-VN"
                )} ₫</span> <del class="fs-6 text-secondary">${(
                  item.quantity * item.price
                ).toLocaleString("vi-VN")} ₫</del>`
              : `${(item.quantity * item.price).toLocaleString("vi-VN")} ₫`
          }</span></li>`;
        }
        document.getElementById("listProduct").innerHTML = listProduct;
        document.getElementById("status_payment").innerHTML = `
                                    <div class="status-section">
                                        Trạng thái: Chờ thanh toán...
                                        <div id="status" class="spinner-border spinner-border-sm text-secondary" role="status">
                                            <span class="visually-hidden">Loading...</span>
                                        </div>
                                    </div>
                                   
          `;
        orderId = data.orderId;
        connectPaymentRealtime();
        document.getElementById("paymentModelBtn").click();
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

function startBuyAll() {
  document.getElementById("buyBtn").removeEventListener("click", buyNow);
  document.getElementById("buyBtn").removeEventListener("click", buyAll);
  document.getElementById("buyBtn").addEventListener("click", buyAll);
  document.getElementById("productInfo").innerHTML = ``;
}

function buyAll() {
  const name = document.getElementById("name").value;
  const email = document.getElementById("email").value;
  const phone = document.getElementById("phone").value;
  const voucher = document.getElementById("voucher")
    ? document.getElementById("voucher").value
    : null;

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

  const checkedItems = getAllCheckedItems();
  if (checkedItems.length === 0) {
    showWarningToast("Vui lòng chọn ít nhất một sản phẩm để thanh toán", 4000);
    return;
  }

  for (const item of checkedItems) {
    if (isNaN(item.quantity) || item.quantity < 1) {
      showWarningToast(`Số lượng sản phẩm "${item.name}" không hợp lệ`, 4000);
      return;
    }
  }

  if (!isValidLongitude(selectedLng) || !isValidLatitude(selectedLat)) {
    showWarningToast(`Vị trí nhận hàng không hợp lệ`, 4000);
    return;
  }

  const items = checkedItems.map((i) => ({
    ProductId: i.id,
    Quantity: Number(i.quantity),
  }));

  const body = JSON.stringify({
    Name: name,
    Email: email ? email : null,
    Phone: phone,
    Items: items,
    Longitude: Number(getSelectedLocation()?.lng),
    Latitude: Number(getSelectedLocation()?.lat),
    Voucher: voucher ? voucher : null,
  });

  fetch("/api/orders", {
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
        let localCartData = localStorage.getItem("cart");
        let cart = localCartData ? JSON.parse(localCartData) : [];
        for (const item of checkedItems) {
          const idx = cart.findIndex((x) => x.id === item.id);
          if (idx !== -1) cart.splice(idx, 1);
        }
        localStorage.setItem("cart", JSON.stringify(cart));
        LoadData();
        document.getElementById(
          "paymentQrCode"
        ).src = `https://qr.sepay.vn/img?acc=${data.accountNo}&bank=${data.bankId}&amount=${data.totalBill}&des=${data.orderId}&template=compact`;
        document.getElementById("bank").textContent = data.bankId;
        document.getElementById("account_name").textContent = data.accountName;
        document.getElementById("account_no").textContent = data.accountNo;
        document.getElementById(
          "bill"
        ).textContent = `${data.totalBill.toLocaleString("vi-VN")} ₫`;
        document.getElementById("orderId").textContent = data.orderId;
        document.getElementById("orderId2").textContent = data.orderId;
        document.getElementById(
          "totalPrice"
        ).textContent = `${data.totalPrice.toLocaleString("vi-VN")}₫`;
        if (data.discount > 0) {
          document.getElementById(
            "discount"
          ).innerHTML = `<p>Giảm giá <span class="float-end text-danger">-${data.discount.toLocaleString(
            "vi-VN"
          )}₫</span></p>`;
        }
        document.getElementById(
          "totalBill"
        ).textContent = `${data.totalBill.toLocaleString("vi-VN")}₫`;
        document.getElementById("fee").textContent = `${data.fee.toLocaleString(
          "vi-VN"
        )}₫`;
        document.getElementById("vat").textContent = `${data.vat.toLocaleString(
          "vi-VN"
        )}₫`;
        let listProduct = ``;
        for (const item of data.productList) {
          listProduct += `<li class="list-group-item">${
            item.name
          } <span class="float-end">${
            item.discount > 0
              ? `<span>${(
                  item.quantity *
                  (item.price - (item.price / 100) * item.discount)
                ).toLocaleString(
                  "vi-VN"
                )} ₫</span> <del class="fs-6 text-secondary">${(
                  item.quantity * item.price
                ).toLocaleString("vi-VN")} ₫</del>`
              : `${(item.quantity * item.price).toLocaleString("vi-VN")} ₫`
          }</span></li>`;
        }
        document.getElementById("listProduct").innerHTML = listProduct;
        document.getElementById("status_payment").innerHTML = `
                                    <div class="status-section">
                                        Trạng thái: Chờ thanh toán...
                                        <div id="status" class="spinner-border spinner-border-sm text-secondary" role="status">
                                            <span class="visually-hidden">Loading...</span>
                                        </div>
                                    </div>
                                   
          `;
        orderId = data.orderId;
        connectPaymentRealtime();
        document.getElementById("paymentModelBtn").click();
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

const map = new maplibregl.Map({
  container: "map",
  style: {
    version: 8,
    sources: {
      osm: {
        type: "raster",
        tiles: ["https://a.tile.openstreetmap.org/{z}/{x}/{y}.png"],
        tileSize: 256,
      },
    },
    layers: [
      {
        id: "osm",
        type: "raster",
        source: "osm",
        minzoom: 0,
        maxzoom: 19,
      },
    ],
  },
  center: [105.8542, 21.0285],
  zoom: 13,
});

let marker;
let currentLocationMarker;
let selectedLat = null;
let selectedLng = null;
let currentLat = null;
let currentLng = null;

map.on("click", (e) => {
  selectedLat = e.lngLat.lat;
  selectedLng = e.lngLat.lng;

  if (marker) marker.remove();

  marker = new maplibregl.Marker()
    .setLngLat([selectedLng, selectedLat])
    .addTo(map);
});

map.on("load", () => {
  if ("geolocation" in navigator) {
    navigator.geolocation.getCurrentPosition((position) => {
      const lng = position.coords.longitude;
      const lat = position.coords.latitude;

      currentLat = lat;
      currentLng = lng;

      map.flyTo({
        center: [lng, lat],
        zoom: 15,
        essential: true,
      });

      currentLocationMarker = new maplibregl.Marker({
        color: "#3333FF",
      })
        .setLngLat([lng, lat])
        .addTo(map);
    });
  }
});

/**
 * Đặt vị trí trên bản đồ (marker màu xanh)
 * @param {number} lat - Vĩ độ
 * @param {number} lng - Kinh độ
 * @param {Object} options - Tùy chọn
 */
function setLocation(lat, lng, options = {}) {
  if (!isValidLatitude(lat) || !isValidLongitude(lng)) {
    console.error("Tọa độ không hợp lệ!");
    return false;
  }

  selectedLat = lat;
  selectedLng = lng;

  if (marker) marker.remove();

  if (currentLocationMarker) {
    currentLocationMarker.remove();
    currentLocationMarker = null;
  }

  marker = new maplibregl.Marker({
    color: "#3333FF",
  })
    .setLngLat([lng, lat])
    .addTo(map);

  if (options.flyTo !== false) {
    map.flyTo({
      center: [lng, lat],
      zoom: options.zoom || 15,
      essential: true,
    });
  }

  return true;
}

/**
 * Lấy tọa độ đã chọn
 * @returns {Object|null} {lat, lng} hoặc null nếu chưa chọn
 */
function getSelectedLocation() {
  if (selectedLat !== null && selectedLng !== null) {
    return {
      lat: selectedLat,
      lng: selectedLng,
    };
  }
  return null;
}

/**
 * Reset về trạng thái ban đầu (như lúc load)
 */
function resetToCurrentLocation() {
  if (currentLat !== null && currentLng !== null) {
    if (marker) {
      marker.remove();
      marker = null;
    }

    selectedLat = null;
    selectedLng = null;

    map.flyTo({
      center: [currentLng, currentLat],
      zoom: 15,
      essential: true,
    });

    if (!currentLocationMarker) {
      currentLocationMarker = new maplibregl.Marker({
        color: "#3333FF",
      })
        .setLngLat([currentLng, currentLat])
        .addTo(map);
    }
  }
}

/**
 * Reset hoàn toàn (xóa tất cả marker và về vị trí mặc định)
 */
function resetToDefault() {
  // Xóa tất cả marker
  if (marker) {
    marker.remove();
    marker = null;
  }
  if (currentLocationMarker) {
    currentLocationMarker.remove();
    currentLocationMarker = null;
  }

  selectedLat = null;
  selectedLng = null;

  map.flyTo({
    center: [105.8542, 21.0285],
    zoom: 13,
    essential: true,
  });
}

/**
 * Validate vĩ độ
 * @param {number} lat
 * @returns {boolean}
 */
function isValidLatitude(lat) {
  return (
    lat !== null &&
    lat !== undefined &&
    !isNaN(Number(lat)) &&
    lat >= -90 &&
    lat <= 90
  );
}

/**
 * Validate kinh độ
 * @param {number} lng
 * @returns {boolean}
 */
function isValidLongitude(lng) {
  return (
    lng !== null &&
    lng !== undefined &&
    !isNaN(Number(lng)) &&
    lng >= -180 &&
    lng <= 180
  );
}

/**
 * Đặt nhiều marker cùng lúc
 * @param {Array} locations - Mảng các {lat, lng, color?, popup?}
 */
function setMultipleLocations(locations) {
  const markers = [];

  locations.forEach((location, index) => {
    if (isValidLatitude(location.lat) && isValidLongitude(location.lng)) {
      const marker = new maplibregl.Marker({
        color: location.color || "#FF0000",
      })
        .setLngLat([location.lng, location.lat])
        .addTo(map);

      if (location.popup) {
        const popup = new maplibregl.Popup({ offset: 25 }).setText(
          location.popup
        );
        marker.setPopup(popup);
      }

      markers.push(marker);
    }
  });

  return markers;
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
