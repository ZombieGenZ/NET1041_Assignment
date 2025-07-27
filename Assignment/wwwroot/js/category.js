document.addEventListener("DOMContentLoaded", () => {
  LoadData();
});

function LoadData(search) {
  if (!search || search.trim() === "") {
    document.getElementById("search").value = "";
  }
  const listDom = document.getElementById("CategoryData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="4">Không có dử liệu nào phù hợp</td></tr>';
  fetch(`/api/categories${!search ? "" : `/?${search}`}`)
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
                            <td>${item.description}</td>
                            <td>${item.index}</td>
                            <td class="text-end button-cell">
                                <button class="btn btn-warning" onclick='startUpdate(${item.id}, "${item.name}", "${item.description}", ${item.index})' data-bs-toggle="modal" data-bs-target="#createAndUpdateModal">Sửa</button>
                                <button class="btn btn-danger" onClick='startDelete(${item.id})' data-bs-toggle="modal" data-bs-target="#deleteModal">Xóa</button>
                            </td>
                        </tr>
                    `;
        }
        listDom.innerHTML = list;
      } else {
        listDom.innerHTML =
          '<tr><td class="text-center" colspan="4">Không có dử liệu nào phù hợp</td></tr>';
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
    "Tạo danh mục mới";
  document.getElementById("createAndUpdateButton").textContent = "Tạo danh mục";
  document.getElementById("createAndUpdateButton").classList.add("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", create);
}

function create() {
  const name = document.getElementById("name").value;
  const description = document.getElementById("description").value;
    const index = Number(document.getElementById("index").value);

  if (!name) {
      showWarningToast(`Vui lòng nhập tên danh mục`, 4000);
    return;
    }

    if (isNaN(index) || (!isNaN(index) && index < 1)) {
        showWarningToast(`Độ ưu tiên không hợp lệ`, 4000);
        return;
    }

  fetch("/api/categories", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: name,
        Description: description,
        Index: index
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo danh mục. Vui lòng thử lại sau.",
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

      if (data.code == "CREATE_CATEGORY_SUCCESS") {
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

let selectedId = 0;

function startUpdate(id, name, description, index) {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Cập nhật danh mục";
  document.getElementById("createAndUpdateButton").textContent =
    "Cập nhật danh mục";
  document.getElementById("createAndUpdateButton").classList.add("btn-warning");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", update);

  selectedId = id;
  document.getElementById("name").value = name;
  document.getElementById("description").value = description;
  document.getElementById("index").value = index;
}

function update() {
  const name = document.getElementById("name").value;
    const description = document.getElementById("description").value;
    const index = Number(document.getElementById("index").value);

    if (!name) {
        showWarningToast(`Vui lòng nhập tên danh mục`, 4000);
        return;
    }

    if (isNaN(index) || (!isNaN(index) && index < 1)) {
        showWarningToast(`Độ ưu tiên không hợp lệ`, 4000);
        return;
    }

  fetch(`/api/categories/${selectedId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Name: name,
        Description: description,
        Index: index
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi cập nhật danh mục. Vui lòng thử lại sau.",
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

      if (data.code == "UPDATE_CATEGORY_SUCCESS") {
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

function del() {
  fetch(`/api/categories/${selectedId}`, {
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
            "Lỗi khi xóa danh mục. Vui lòng thử lại sau.",
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

      if (data.code == "DELETE_CATEGORY_SUCCESS") {
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

function startDelete(id) {
  selectedId = id;
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
  document.getElementById("index").value = "1";
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
