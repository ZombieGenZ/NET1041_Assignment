document.addEventListener("DOMContentLoaded", () => {
  LoadData();

  const discountTypeSelect = document.getElementById("discountType");
  const discount = document.getElementById("discount");
  const max = document.getElementById("div_max");
  if (discountTypeSelect && discount) {
    discountTypeSelect.addEventListener("change", function () {
      if (this.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (this.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    });
    if (discountTypeSelect.value === "0") {
      max.style.display = "none";
      discount.placeholder = "ví dụ: 100,000";
    } else if (discountTypeSelect.value === "1") {
      max.style.display = "block";
      discount.placeholder = "ví dụ: 100%";
    }
  }

  const isLifeTimeCheckbox = document.getElementById("isLifeTime");
  const divEndTime = document.getElementById("div_endTime");
  if (isLifeTimeCheckbox && divEndTime) {
    isLifeTimeCheckbox.addEventListener("change", function () {
      if (this.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    });
    if (isLifeTimeCheckbox.checked) {
      divEndTime.style.display = "none";
    } else {
      divEndTime.style.display = "block";
    }
  }

  const unlimitedPercentageDiscountCheckbox = document.getElementById(
    "unlimitedPercentageDiscount"
  );
  const maximumPercentageReduction = document.getElementById(
    "div_maximumPercentageReduction"
  );
  if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
    unlimitedPercentageDiscountCheckbox.addEventListener("change", function () {
      if (this.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    });
    if (unlimitedPercentageDiscountCheckbox.checked) {
      maximumPercentageReduction.style.display = "none";
    } else {
      maximumPercentageReduction.style.display = "block";
    }
  }
});

function LoadData(search) {
  if (!search || search.trim() === "") {
    document.getElementById("search").value = "";
  }
  const listDom = document.getElementById("RedeemData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="9">Không có dử liệu nào phù hợp</td></tr>';
  fetch(`/api/redeems${!search ? "" : `/?${search}`}`)
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
        for (var item of data) {
          list += `
                        <tr>
                            <td>${item.id}</td>
                            <td>${item.name}</td>
                           
                            <td>${
                              item.discountType === 0
                                ? item.discount.toLocaleString("vi-VN", {
                                    style: "currency",
                                    currency: "VND",
                                  })
                                : `${item.discount}%`
                            }</td>
                            <td>${
                              item.isLifeTime
                                ? "Vĩnh viễn"
                                : convertDurationToStringDescription(
                                    item.endTime
                                  )
                            }</td>
                            <td>${item.minimumRequirements.toLocaleString("vi-VN", {
                                style: "currency",
                                currency: "VND",
                            })}</td>
                            <td>${
                              item.unlimitedPercentageDiscount
                                ? "Không giới hạn"
                                : !item.maximumPercentageReduction
                                ? ""
                                : item.maximumPercentageReduction.toLocaleString(
                                    "vi-VN",
                                    {
                                      style: "currency",
                                      currency: "VND",
                                    }
                                  )
                            }</td>
                            <td>${item.price.toLocaleString("vi-VN")} điểm</td>
                            <td>${convertNumberToVietnameseWord(
                              item.rankRequirement
                            )}</td>
                            <td class="text-end button-cell">
                                <button class="btn btn-warning" onclick='startUpdate(${
                                  item.id
                                }, "${item.name}", "${item.description}", ${
            item.discountType
          }, ${item.discount}, ${item.isLifeTime}, ${
            !item.endTime ? null : `"${item.endTime}"`
          }, ${item.minimumRequirements}, ${
            item.unlimitedPercentageDiscount
          }, ${item.maximumPercentageReduction}, ${item.price}, ${
            item.rankRequirement
              }, ${item.isPublish})' data-bs-toggle="modal" data-bs-target="#createAndUpdateModal">Sửa</button>
                                <button class="btn btn-danger" onClick='startDelete(${
                                  item.id
                                })' data-bs-toggle="modal" data-bs-target="#deleteModal">Xóa</button>
                            </td>
                        </tr>
                    `;
        }
        listDom.innerHTML = list;
      } else {
        listDom.innerHTML =
          '<tr><td class="text-center" colspan="9">Không có dử liệu nào phù hợp</td></tr>';
      }
    });
}

function Search() {
    const searchValue = document.getElementById("search").value.trim();

    const customEncodeUriComponent = (value) => {
        return encodeURIComponent(value).replace(/%20/g, "+");
    };

    const params = [];
    if (searchValue) params.push(`text=${customEncodeUriComponent(searchValue)}`);

    const queryString = params.length > 0 ? params.join("&") : "";
    if (queryString) {
        LoadData(queryString);
    } else {
        LoadData();
    }
}

function startCreate() {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Tạo mã vật phẩm đổi thưởng mới";
  document.getElementById("createAndUpdateButton").textContent =
    "Tạo mã vật phẩm đổi thưởng";
  document.getElementById("createAndUpdateButton").classList.add("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", create);

  {
    const discountTypeSelect = document.getElementById("discountType");
    const discount = document.getElementById("discount");
    const max = document.getElementById("div_max");
    if (discountTypeSelect && discount) {
      if (discountTypeSelect.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (discountTypeSelect.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    }

    const isLifeTimeCheckbox = document.getElementById("isLifeTime");
    const divEndTime = document.getElementById("div_endTime");
    if (isLifeTimeCheckbox && divEndTime) {
      if (isLifeTimeCheckbox.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    }

    const unlimitedPercentageDiscountCheckbox = document.getElementById(
      "unlimitedPercentageDiscount"
    );
    const maximumPercentageReduction = document.getElementById(
      "div_maximumPercentageReduction"
    );
    if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
      if (unlimitedPercentageDiscountCheckbox.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    }
  }
}

function create() {
  const name = document.getElementById("name").value;
  const description = document.getElementById("description").value;
  const discountType = document.getElementById("discountType").value;
  const discount = document.getElementById("discount").value;
  const isLifeTime = document.getElementById("isLifeTime").checked;
  const endTimeValue = document.getElementById("endTimeValue").value;
  const endTimeType = document.getElementById("endTimeType").value;
  const minimumRequirements = document.getElementById(
    "minimumRequirements"
  ).value;
  const unlimitedPercentageDiscount = document.getElementById(
    "unlimitedPercentageDiscount"
  ).checked;
  const maximumPercentageReduction = document.getElementById(
    "maximumPercentageReduction"
  ).value;
  const price = document.getElementById("price").value;
  const rankRequirement = document.getElementById("rankRequirement").value;
    const isPublish = document.getElementById("publish").checked;

  const endTime = endTimeValue + endTimeType;

  if (!name || !description) {
    const errMsg = [];
    if (!name) {
      errMsg.push("tên sản phẩm");
    }
    if (!description) {
      errMsg.push("mô tả sản phẩm");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (discountType == "1") {
    if (discount < 0 || discount > 100) {
      showWarningToast(`% giảm giá chỉ có thể từ 0 đến 100%`, 4000);
      return;
    }
  } else {
    if (discount <= 0) {
      showWarningToast(`Số tiền giảm giá không được bé hơn hoặc bằng 0`, 4000);
      return;
    }
  }

  if (!isLifeTime) {
    if (!isValidDurationString(endTime)) {
      showWarningToast(`Thời hạn không hợp lệ`, 4000);
      return;
    }
  }

  if (
    isNaN(Number(minimumRequirements)) ||
    (!isNaN(Number(minimumRequirements)) && Number(minimumRequirements) < 0)
  ) {
    showWarningToast(`Yêu cầu tối thiểu phải lớn hơn 0`, 4000);
    return;
  }

  if (discountType == "1") {
    if (!unlimitedPercentageDiscount) {
      if (
        isNaN(Number(maximumPercentageReduction)) ||
        (!isNaN(Number(maximumPercentageReduction)) &&
          Number(maximumPercentageReduction) < 1)
      ) {
        showWarningToast(`Số tiền giảm tối đa phải lớn hơn 0`, 4000);
        return;
      }
    }
  }

  if (isNaN(Number(price)) || (!isNaN(Number(price)) && Number(price) < 0)) {
    showWarningToast(`Giá quy đổi phải lớn hơn hoặc bằng 0`, 4000);
    return;
  }

  fetch("/api/redeems", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: name,
      Description: description,
      Discount: Number(discount),
      DiscountType: Number(discountType),
      IsLifeTime: isLifeTime,
      EndTime: isLifeTime ? null : endTime,
      MinimumRequirements: Number(minimumRequirements),
      UnlimitedPercentageDiscount: unlimitedPercentageDiscount,
      MaximumPercentageReduction: Number(maximumPercentageReduction),
      Price: Number(price),
        RankRequirement: Number(rankRequirement),
        IsPublish: isPublish
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo vật phẩm đổi thưởng. Vui lòng thử lại sau.",
            4000
          );
        }

        return response.json();
      }
      return response.json();
    })
    .then((data) => {
      if (data.code == "INPUT_DATA_ERROR") {
        showWarningToast(data.message, 4000);
        return;
      }

      if (data.code == "CREATE_REDEEM_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById(
          "createAndUpdateModal"
        );
        const modal = bootstrap.Modal.getInstance(createAndUpdateModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function startUpdate(
  id,
  name,
  description,
  discountType,
  discount,
  isLifeTime,
  endTime,
  minimumRequirements,
  unlimitedPercentageDiscount,
  maximumPercentageReduction,
  price,
    rankRequirement,
    isPublish
) {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Cập nhật vật phẩm đổi thưởng";
  document.getElementById("createAndUpdateButton").textContent =
    "Cập nhật vật phẩm đổi thưởng";
  document.getElementById("createAndUpdateButton").classList.add("btn-warning");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", update);

  selectedId = id;
  document.getElementById("name").value = name;
  document.getElementById("description").value = description;
  document.getElementById("discountType").value = discountType;
  document.getElementById("discount").value = discount;
  document.getElementById("isLifeTime").checked = isLifeTime;
  document.getElementById("endTimeValue").value = getDurationValue(endTime);
  document.getElementById("endTimeType").value = getDurationType(endTime);
  document.getElementById("minimumRequirements").value = minimumRequirements;
  document.getElementById("unlimitedPercentageDiscount").checked =
    unlimitedPercentageDiscount;
  document.getElementById("maximumPercentageReduction").value =
    maximumPercentageReduction;
  document.getElementById("price").value = price;
  document.getElementById("rankRequirement").value = rankRequirement;
    document.getElementById("publish").checked = isPublish;

  {
    const discountTypeSelect = document.getElementById("discountType");
    const discount = document.getElementById("discount");
    const max = document.getElementById("div_max");
    if (discountTypeSelect && discount) {
      if (discountTypeSelect.value === "0") {
        max.style.display = "none";
        discount.placeholder = "ví dụ: 100,000";
      } else if (discountTypeSelect.value === "1") {
        max.style.display = "block";
        discount.placeholder = "ví dụ: 100%";
      }
    }

    const isLifeTimeCheckbox = document.getElementById("isLifeTime");
    const divEndTime = document.getElementById("div_endTime");
    if (isLifeTimeCheckbox && divEndTime) {
      if (isLifeTimeCheckbox.checked) {
        divEndTime.style.display = "none";
      } else {
        divEndTime.style.display = "block";
      }
    }

    const unlimitedPercentageDiscountCheckbox = document.getElementById(
      "unlimitedPercentageDiscount"
    );
    const maximumPercentageReduction = document.getElementById(
      "div_maximumPercentageReduction"
    );
    if (unlimitedPercentageDiscountCheckbox && maximumPercentageReduction) {
      if (unlimitedPercentageDiscountCheckbox.checked) {
        maximumPercentageReduction.style.display = "none";
      } else {
        maximumPercentageReduction.style.display = "block";
      }
    }
  }
}

function update() {
  const name = document.getElementById("name").value;
  const description = document.getElementById("description").value;
  const discountType = document.getElementById("discountType").value;
  const discount = document.getElementById("discount").value;
  const isLifeTime = document.getElementById("isLifeTime").checked;
  const endTimeValue = document.getElementById("endTimeValue").value;
  const endTimeType = document.getElementById("endTimeType").value;
  const minimumRequirements = document.getElementById(
    "minimumRequirements"
  ).value;
  const unlimitedPercentageDiscount = document.getElementById(
    "unlimitedPercentageDiscount"
  ).checked;
  const maximumPercentageReduction = document.getElementById(
    "maximumPercentageReduction"
  ).value;
  const price = document.getElementById("price").value;
    const rankRequirement = document.getElementById("rankRequirement").value;
    const isPublish = document.getElementById("publish").checked;

  const endTime = endTimeValue + endTimeType;

  if (!name || !description) {
    const errMsg = [];
    if (!name) {
      errMsg.push("tên sản phẩm");
    }
    if (!description) {
      errMsg.push("mô tả sản phẩm");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (discountType == "1") {
    if (discount < 0 || discount > 100) {
      showWarningToast(`% giảm giá chỉ có thể từ 0 đến 100%`, 4000);
      return;
    }
  } else {
    if (discount <= 0) {
      showWarningToast(`Số tiền giảm giá không được bé hơn hoặc bằng 0`, 4000);
      return;
    }
  }

  if (!isLifeTime) {
    if (!isValidDurationString(endTime)) {
      showWarningToast(`Thời hạn không hợp lệ`, 4000);
      return;
    }
  }

  if (
    isNaN(Number(minimumRequirements)) ||
    (!isNaN(Number(minimumRequirements)) && Number(minimumRequirements) < 0)
  ) {
    showWarningToast(`Yêu cầu tối thiểu phải lớn hơn 0`, 4000);
    return;
  }

  if (discountType == "1") {
    if (!unlimitedPercentageDiscount) {
      if (
        isNaN(Number(maximumPercentageReduction)) ||
        (!isNaN(Number(maximumPercentageReduction)) &&
          Number(maximumPercentageReduction) < 1)
      ) {
        showWarningToast(`Số tiền giảm tối đa phải lớn hơn 0`, 4000);
        return;
      }
    }
  }

  if (isNaN(Number(price)) || (!isNaN(Number(price)) && Number(price) < 0)) {
    showWarningToast(`Giá quy đổi phải lớn hơn hoặc bằng 0`, 4000);
    return;
  }

  fetch(`/api/redeems/${selectedId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: name,
      Description: description,
      Discount: Number(discount),
      DiscountType: Number(discountType),
      IsLifeTime: isLifeTime,
      EndTime: isLifeTime ? null : endTime,
      MinimumRequirements: Number(minimumRequirements),
      UnlimitedPercentageDiscount: unlimitedPercentageDiscount,
      MaximumPercentageReduction: Number(maximumPercentageReduction),
      Price: Number(price),
        RankRequirement: Number(rankRequirement),
        IsPublish: isPublish
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo vật phẩm đổi thưởng. Vui lòng thử lại sau.",
            4000
          );
        }

        return response.json();
      }
      return response.json();
    })
    .then((data) => {
      if (data.code == "INPUT_DATA_ERROR") {
        showWarningToast(data.message, 4000);
        return;
      }

      if (data.code == "UPDATE_REDEEM_SUCCESS") {
        LoadData();
        clearInput();
        const createAndUpdateModal = document.getElementById(
          "createAndUpdateModal"
        );
        const modal = bootstrap.Modal.getInstance(createAndUpdateModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
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
  fetch(`/api/redeems/${selectedId}`, {
    method: "DELETE",
    headers: {
      "Content-Type": "application/json",
    },
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi xóa mã giảm giá. Vui lòng thử lại sau.",
            4000
          );
        }

        return response.json();
      }
      return response.json();
    })
    .then((data) => {
      if (data.code == "INPUT_DATA_ERROR") {
        showWarningToast(data.message, 4000);
        return;
      }

      if (data.code == "DELETE_REDEEM_SUCCESS") {
        LoadData();
        const deleteModal = document.getElementById("deleteModal");
        const modal = bootstrap.Modal.getInstance(deleteModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function clearEvent() {
  document
    .getElementById("createAndUpdateButton")
    .removeEventListener("click", create);
  document
    .getElementById("createAndUpdateButton")
    .removeEventListener("click", update);
}

function clearClass() {
  document
    .getElementById("createAndUpdateButton")
    .classList.remove("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .classList.remove("btn-warning");
}

function clearInput() {
  document.getElementById("name").value = "";
  document.getElementById("description").value = "";
  document.getElementById("discountType").value = "0";
  document.getElementById("discount").value = 0;
  document.getElementById("isLifeTime").checked = false;
  document.getElementById("endTimeValue").value = 1;
  document.getElementById("endTimeType").value = "mo";
  document.getElementById("minimumRequirements").value = 0;
  document.getElementById("unlimitedPercentageDiscount").checked = false;
  document.getElementById("maximumPercentageReduction").value = 0;
  document.getElementById("price").value = 0;
  document.getElementById("rankRequirement").value = "0";
    document.getElementById("publish").checked = true;
}

function getDurationValue(input) {
  if (!isValidDurationString(input)) {
    return 0;
  }
  const match = input.match(durationRegex);
  return parseInt(match[1]);
}

function getDurationType(input) {
  if (!isValidDurationString(input)) {
    return "Chuổi không hợp lệ";
  }
  const match = input.match(durationRegex);
  return match[2].toLowerCase(); // Trả về loại đã được chuyển về chữ thường
}

function convertNumberToVietnameseWord(number) {
  switch (number) {
    case 1:
      return "Đồng";
    case 2:
      return "Bạc";
    case 3:
      return "Vàng";
    case 4:
      return "Kim cương";
    default:
      return "Không cần";
  }
}

const durationRegex = /^(\d+)([smhdw]|mo|y)$/i;

function isValidDurationString(input) {
  if (input === null || input === undefined || input.trim() === "") {
    return false;
  }
  return durationRegex.test(input);
}

function convertDurationToStringDescription(input) {
  if (!isValidDurationString(input)) {
    return "Chuổi không hợp lệ";
  }

  const match = input.match(durationRegex);
  const value = parseInt(match[1]);
  const type = match[2].toLowerCase();

  let unit;
  switch (type) {
    case "s":
      unit = "giây";
      break;
    case "m":
      unit = "phút";
      break;
    case "h":
      unit = "giờ";
      break;
    case "d":
      unit = "ngày";
      break;
    case "w":
      unit = "tuần";
      break;
    case "mo":
      unit = "tháng";
      break;
    case "y":
      unit = "năm";
      break;
    default:
      return "Chuổi không hợp lệ";
  }

  return `${value} ${unit}`;
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
