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
  const quantity = document.getElementById("quantity");
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
  } else {
    document.getElementById("productPrice").value =
      "Số lượng sản phẩm không hợp lệ";
  }
}

document.addEventListener("DOMContentLoaded", () => {
  const productQuantity = document.getElementById("productQuantity");
  if (!productQuantity) {
    return;
  }
  productQuantity.addEventListener("change", () => {
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
        document.getElementById("paymentModelBtn").click();
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
