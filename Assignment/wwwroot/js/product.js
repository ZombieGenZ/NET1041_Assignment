document.addEventListener("DOMContentLoaded", () => {
  LoadData();

  document.getElementById("img").addEventListener("change", function (event) {
    const file = event.target.files[0];
    const previewContainer = document.getElementById("image-preview");
    const previewImg = document.getElementById("preview-img");

    if (file && file.type.startsWith("image/")) {
      const reader = new FileReader();
      reader.onload = function (e) {
        previewImg.src = e.target.result;
        previewContainer.classList.remove("d-none");
        previewContainer.classList.add("d-flex");
      };
      reader.readAsDataURL(file);
    } else {
      previewContainer.classList.add("d-none");
      previewContainer.classList.remove("d-flex");
      previewImg.src = "";
    }
  });

  const listDom = document.getElementById("categoryList");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<option value="" selected disabled>Chọn danh mục</option>';
  fetch("/api/categories")
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
        list += `<option value="" selected disabled>Chọn danh mục</option>`;
        for (var item of data) {
          list += `<option value="${item.id}">${item.name}</option>`;
        }
        listDom.innerHTML = list;
      } else {
        listDom.innerHTML = `<option value="" selected disabled>Chọn danh mục</option>`;
      }
    });
});

function LoadData(search) {
  if (!search || search.trim() === "") {
    document.getElementById("search").value = "";
  }
  const listDom = document.getElementById("ProductData");
  listDom.innerHTML = "";
  listDom.innerHTML =
    '<tr><td class="text-center" colspan="7">Không có dử liệu nào phù hợp</td></tr>';
  fetch(`/api/products${!search ? "" : `/?${search}`}`)
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
                            <td>${item.stock.toLocaleString("vi-VN")}</td>
                            <td>${item.sold.toLocaleString("vi-VN")}</td>
                            <td>${item.discount}%</td>
                            <td class="text-end button-cell">
                                <button class="btn btn-info" onclick='startShow("${
                                  item.name
                                }", \`${item.description}\`, ${item.price}, ${
            item.stock
          }, ${item.sold}, ${item.discount}, "${item.productImageUrl}", ${
            item.preparationTime
          }, ${item.calories}, "${item.ingredients}", ${item.isSpicy}, ${
            item.isVegetarian
          }, ${item.totalEvaluate}, ${
            item.averageEvaluate
          })' data-bs-toggle="modal" data-bs-target="#showModal">Xem</button>
                                <button class="btn btn-warning" onclick='startUpdate(${
                                  item.id
                                }, "${item.name}", \`${item.description}\`, ${
            item.categoryId
          }, ${item.price}, ${item.stock}, ${item.discount}, ${
            item.isPublish
          }, ${item.preparationTime}, ${item.calories}, "${
            item.ingredients
          }", ${item.isSpicy}, ${item.isVegetarian})' data-bs-toggle="modal" data-bs-target="#createAndUpdateModal">Sửa</button>
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
          '<tr><td class="text-center" colspan="7">Không có dử liệu nào phù hợp</td></tr>';
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

let searchAdvanced = false;

function startShow(
  name,
  description,
  price,
  stock,
  sold,
  discount,
  imgPath,
  preparationTime,
  calories,
  ingredients,
  isSpicy,
  isVegetarian,
  totalEvaluate,
  averageEvaluate
) {
  document.getElementById("show_img").src = imgPath;
  document.getElementById("show_img").alt = name;
  document.getElementById("show_title").textContent = name;
  document.getElementById("show_description").innerHTML = description;
  document.getElementById("show_price").innerHTML = `${
    discount <= 0
      ? `<span>${price.toLocaleString("vi-VN", {
          style: "currency",
          currency: "VND",
        })}</span>`
      : `<span class="badge bg-danger">${discount}%</span>
      <span>${(price - (price * discount) / 100).toLocaleString("vi-VN", {
        style: "currency",
        currency: "VND",
      })}</span> <del class="text-secondary"><span id="show_price">${price.toLocaleString(
          "vi-VN",
          {
            style: "currency",
            currency: "VND",
          }
        )}</span></del>`
  }`;
  document.getElementById("show_sold").innerHTML = sold.toLocaleString("vi-VN");
  document.getElementById("show_stock").innerHTML =
    stock.toLocaleString("vi-VN");
  document.getElementById("show_isVegetarian").innerHTML = isVegetarian
    ? "Món chay"
    : "Món mặn";
  document.getElementById("show_isSpicy").innerHTML = isSpicy
    ? "Cay"
    : "Không cay";
  document.getElementById("show_preparationTime").textContent = preparationTime;
  document.getElementById("show_calories").innerHTML = calories;
  document.getElementById("show_ingredients").innerHTML = ingredients;
  let htmlString = "";
  htmlString += `<span>${averageEvaluate}</span> `;
  for (let i = 1; i <= 5; i++) {
    if (totalEvaluate >= i) {
      htmlString += '<i class="fa-solid fa-star text-warning"></i>';
    } else if (totalEvaluate >= i - 0.5) {
      htmlString +=
        '<i class="fa-regular fa-star-half-stroke text-warning"></i>';
    } else {
      htmlString += '<i class="fa-regular fa-star text-warning"></i>';
    }
  }
  htmlString += `<span> | ${totalEvaluate} lượt đánh giá</span>`;
  document.getElementById("show_raiting").innerHTML = htmlString;
}

function startCreate() {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Tạo sản phẩm mới";
  document.getElementById("createAndUpdateButton").textContent = "Tạo sản phẩm";
  document.getElementById("createAndUpdateButton").classList.add("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", create);
  resetToCurrentLocation();
}

function create() {
  const name = document.getElementById("name").value;
  const description = window.editor.getData();
  const category = document.getElementById("categoryList").value;
  const price = document.getElementById("price").value;
  const stock = document.getElementById("stock").value;
  const discount = document.getElementById("discount").value;
  const img = document.getElementById("img").files[0];
  const publish = document.getElementById("publish").checked;
  const preparationTime = document.getElementById("preparationTime").value;
  const calories = document.getElementById("calories").value;
  const ingredients = document.getElementById("ingredients").value;
  const isSpicy = document.getElementById("isSpicy").checked;
  const isVegetarian = document.getElementById("isVegetarian").checked;

  if (!name || !description || !ingredients) {
    const errMsg = [];
    if (!name) {
      errMsg.push("tên sản phẩm");
    }
    if (!description) {
      errMsg.push("mô tả sản phẩm");
    }
    if (!ingredients) {
      errMsg.push("thành phần của sản phẩm");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (!img) {
    showWarningToast(`Vui lòng chọn ảnh minh họa cho sản phẩm`, 4000);
    return;
  }

  if (!img.type.startsWith("image/")) {
    showWarningToast(`Ảnh minh họa cho sản phẩm phải có là dạn ảnh`, 4000);
    return;
  }

  if (isNaN(price) || (!isNaN(price) && price < 0)) {
    showWarningToast(`Giá sản phẩm phải lớn hơn hoặc bằng 0`, 4000);
    return;
  }

  if (isNaN(stock) || (!isNaN(stock) && stock <= 0)) {
    showWarningToast(`Sản phẩm còn lại sản phẩm phải lớn hơn 0`, 4000);
    return;
  }

  if (
    isNaN(discount) ||
    (!isNaN(discount) && discount < 0) ||
    (!isNaN(discount) && discount > 100)
  ) {
    showWarningToast(
      `Giảm giá phải lớn hơn hoặc bằng 0% và nhỏ hơn hoặc bằng 100%`,
      4000
    );
    return;
  }

  if (
    isNaN(preparationTime) ||
    (!isNaN(preparationTime) && preparationTime < 0)
  ) {
    showWarningToast(`Thời gian chuẩn bị phải lớn hơn 0`, 4000);
    return;
  }

  if (isNaN(calories) || (!isNaN(calories) && calories < 0)) {
    showWarningToast(`Thời gian chuẩn bị phải lớn hơn 0`, 4000);
    return;
  }

  if (!category) {
    showWarningToast(`Vui lòng chọn danh mục cho sản phẩm`, 4000);
    return;
  }

  const formData = new FormData();
  formData.append("Name", name);
  formData.append("Description", description);
  formData.append("Price", price);
  formData.append("Stock", stock);
  formData.append("Discount", discount);
  formData.append("IsPublish", publish);
  formData.append("ProductImage", img);
  formData.append("CategoryId", category);
  formData.append("PreparationTime", preparationTime);
  formData.append("Calories", calories);
  formData.append("Ingredients", ingredients);
  formData.append("IsSpicy", isSpicy);
  formData.append("IsVegetarian", isVegetarian);

  fetch("/api/products", {
    method: "POST",
    body: formData,
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo sản phẩm. Vui lòng thử lại sau.",
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

      if (data.code == "CREATE_PRODUCT_SUCCESS") {
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

function startUpdate(
  id,
  name,
  description,
  category,
  price,
  stock,
  discount,
  isPublish,
  preparationTime,
  calories,
  ingredients,
  isSpicy,
  isVegetarian
) {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Cập nhật sản phẩm";
  document.getElementById("createAndUpdateButton").textContent =
    "Cập nhật sản phẩm";
  document.getElementById("createAndUpdateButton").classList.add("btn-warning");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", update);

  selectedId = id;
  document.getElementById("name").value = name;
  window.editor.setData(description);
  document.getElementById("categoryList").value = category;
  document.getElementById("price").value = price;
  document.getElementById("stock").value = stock;
  document.getElementById("discount").value = discount;
  document.getElementById("publish").checked = isPublish;
  document.getElementById("preparationTime").value = preparationTime;
  document.getElementById("calories").value = calories;
  document.getElementById("ingredients").value = ingredients;
  document.getElementById("isSpicy").checked = isSpicy;
  document.getElementById("isVegetarian").checked = isVegetarian;
}

function update() {
  const name = document.getElementById("name").value;
  const description = window.editor.getData();
  const category = document.getElementById("categoryList").value;
  const price = document.getElementById("price").value;
  const stock = document.getElementById("stock").value;
  const discount = document.getElementById("discount").value;
  const img = document.getElementById("img").files[0];
  const publish = document.getElementById("publish").checked;
  const preparationTime = document.getElementById("preparationTime").value;
  const calories = document.getElementById("calories").value;
  const ingredients = document.getElementById("ingredients").value;
  const isSpicy = document.getElementById("isSpicy").checked;
  const isVegetarian = document.getElementById("isVegetarian").checked;

  if (!name || !description || !ingredients) {
    const errMsg = [];
    if (!name) {
      errMsg.push("tên sản phẩm");
    }
    if (!description) {
      errMsg.push("mô tả sản phẩm");
    }
    if (!ingredients) {
      errMsg.push("thành phần của sản phẩm");
    }
    showWarningToast(`Vui lòng nhập ${errMsg.join(", ")}`, 4000);
    return;
  }

  if (img && !img.type.startsWith("image/")) {
    showWarningToast(`Ảnh minh họa cho sản phẩm phải có là dạn ảnh`, 4000);
    return;
  }

  if (isNaN(price) || (!isNaN(price) && price < 0)) {
    showWarningToast(`Giá sản phẩm phải lớn hơn hoặc bằng 0`, 4000);
    return;
  }

  if (isNaN(stock) || (!isNaN(stock) && stock <= 0)) {
    showWarningToast(`Sản phẩm còn lại sản phẩm phải lớn hơn 0`, 4000);
    return;
  }

  if (
    isNaN(discount) ||
    (!isNaN(discount) && discount < 0) ||
    (!isNaN(discount) && discount > 100)
  ) {
    showWarningToast(
      `Giảm giá phải lớn hơn hoặc bằng 0% và nhỏ hơn hoặc bằng 100%`,
      4000
    );
    return;
  }

  if (
    isNaN(preparationTime) ||
    (!isNaN(preparationTime) && preparationTime < 0)
  ) {
    showWarningToast(`Thời gian chuẩn bị phải lớn hơn 0`, 4000);
    return;
  }

  if (isNaN(calories) || (!isNaN(calories) && calories < 0)) {
    showWarningToast(`Thời gian chuẩn bị phải lớn hơn 0`, 4000);
    return;
  }

  if (!category) {
    showWarningToast(`Vui lòng chọn danh mục cho sản phẩm`, 4000);
    return;
  }

  const formData = new FormData();
  formData.append("Name", name);
  formData.append("Description", description);
  formData.append("Price", price);
  formData.append("Stock", stock);
  formData.append("Discount", discount);
  formData.append("IsPublish", publish);
  if (img) {
    formData.append("ProductImage", img);
  }
  formData.append("CategoryId", category);
  formData.append("PreparationTime", preparationTime);
  formData.append("Calories", calories);
  formData.append("Ingredients", ingredients);
  formData.append("IsSpicy", isSpicy);
  formData.append("IsVegetarian", isVegetarian);

  fetch(`/api/products/${selectedId}`, {
    method: "PUT",
    body: formData,
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi cập nhật sản phẩm. Vui lòng thử lại sau.",
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

      if (data.code == "UPDATE_PRODUCT_SUCCESS") {
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
  fetch(`/api/products/${selectedId}`, {
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
            "Lỗi khi xóa sản phẩm. Vui lòng thử lại sau.",
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

      if (data.code == "DELETE_PRODUCT_SUCCESS") {
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
  window.editor.setData("");
  document.getElementById("categoryList").value = "";
  document.getElementById("price").value = 0;
  document.getElementById("stock").value = 0;
  document.getElementById("discount").value = 0;
  document.getElementById("img").value = "";
  document.getElementById("publish").checked = true;
  document.getElementById("image-preview").classList.add("d-none");
  document.getElementById("image-preview").classList.remove("d-flex");
  document.getElementById("preview-img").src = "";
  document.getElementById("preparationTime").value = "1";
  document.getElementById("calories").value = "1";
  document.getElementById("ingredients").value = "";
  document.getElementById("isSpicy").checked = false;
  document.getElementById("isVegetarian").checked = false;
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

const {
  DecoupledEditor,
  Alignment,
  Autoformat,
  AutoImage,
  AutoLink,
  Autosave,
  BalloonToolbar,
  BlockQuote,
  BlockToolbar,
  Bold,
  Bookmark,
  Code,
  CodeBlock,
  Emoji,
  Essentials,
  FindAndReplace,
  FontBackgroundColor,
  FontColor,
  FontFamily,
  FontSize,
  GeneralHtmlSupport,
  Heading,
  HorizontalLine,
  HtmlComment,
  HtmlEmbed,
  ImageBlock,
  ImageCaption,
  ImageEditing,
  ImageInline,
  ImageInsert,
  ImageInsertViaUrl,
  ImageResize,
  ImageStyle,
  ImageTextAlternative,
  ImageToolbar,
  ImageUpload,
  ImageUtils,
  Indent,
  IndentBlock,
  Italic,
  List,
  ListProperties,
  MediaEmbed,
  Mention,
  PageBreak,
  Paragraph,
  PasteFromOffice,
  PlainTableOutput,
  RemoveFormat,
  ShowBlocks,
  SpecialCharacters,
  SpecialCharactersArrows,
  SpecialCharactersCurrency,
  SpecialCharactersEssentials,
  SpecialCharactersLatin,
  SpecialCharactersMathematical,
  SpecialCharactersText,
  Strikethrough,
  Subscript,
  Superscript,
  Table,
  TableCaption,
  TableCellProperties,
  TableColumnResize,
  TableLayout,
  TableProperties,
  TableToolbar,
  TextPartLanguage,
  TextTransformation,
  TodoList,
  Underline,
  WordCount,
} = window.CKEDITOR;

const LICENSE_KEY =
  "eyJhbGciOiJFUzI1NiJ9.eyJleHAiOjE3Nzk0OTQzOTksImp0aSI6Ijk0YzFjYTU4LWI2NDAtNGVlNi1iMmJmLTFjNzhiYzk1OTIxMyIsImxpY2Vuc2VkSG9zdHMiOlsiMTI3LjAuMC4xIiwibG9jYWxob3N0IiwiMTkyLjE2OC4qLioiLCIxMC4qLiouKiIsIjE3Mi4qLiouKiIsIioudGVzdCIsIioubG9jYWxob3N0IiwiKi5sb2NhbCJdLCJ1c2FnZUVuZHBvaW5kIjoiaHR0cHM6Ly9wcm94eS1ldmVudC5ja2VkaXRvci5jb20iLCJkaXN0cmlidXRpb25DaGFubmVsIjpbImNsb3VkIiwiZHJ1cGFsIl0sImxpY2Vuc2VUeXBlIjoiZGV2ZWxvcG1lbnQiLCJmZWF0dXJlcyI6WyJEUlVQIiwiRTJQIiwiRTJXIl0sInZjIjoiZjAxYjRiNjEifQ.ZiGa-zpyyfnAS7sXIOwEl6ntbj_lCrMTVRRC0uokKsIoGzSVKAc8mGXuhHaOHsY0pFty4z_jfvJQp0vNt3ggeg";

const editorConfig = {
  toolbar: {
    items: [
      "undo",
      "redo",
      "|",
      "heading",
      "|",
      "bold",
      "italic",
      "underline",
      "|",
      "insertImage",
      "|",
      "bulletedList",
      "numberedList",
      "todoList",
      "|",
      "fontSize",
      "fontFamily",
      "fontColor",
      "fontBackgroundColor",
      "alignment",
      "outdent",
      "indent",
      "insertTable",
      "blockQuote",
      "codeBlock",
      "showBlocks",
    ],
    shouldNotGroupWhenFull: true,
  },
  plugins: [
    Alignment,
    Autoformat,
    AutoImage,
    AutoLink,
    Autosave,
    BalloonToolbar,
    BlockQuote,
    BlockToolbar,
    Bold,
    Bookmark,
    Code,
    CodeBlock,
    Emoji,
    Essentials,
    FindAndReplace,
    FontBackgroundColor,
    FontColor,
    FontFamily,
    FontSize,
    GeneralHtmlSupport,
    Heading,
    HorizontalLine,
    HtmlComment,
    HtmlEmbed,
    ImageBlock,
    ImageCaption,
    ImageEditing,
    ImageInline,
    ImageInsert,
    ImageInsertViaUrl,
    ImageResize,
    ImageStyle,
    ImageTextAlternative,
    ImageToolbar,
    ImageUpload,
    ImageUtils,
    Indent,
    IndentBlock,
    Italic,
    List,
    ListProperties,
    MediaEmbed,
    Mention,
    PageBreak,
    Paragraph,
    PasteFromOffice,
    PlainTableOutput,
    RemoveFormat,
    ShowBlocks,
    SpecialCharacters,
    SpecialCharactersArrows,
    SpecialCharactersCurrency,
    SpecialCharactersEssentials,
    SpecialCharactersLatin,
    SpecialCharactersMathematical,
    SpecialCharactersText,
    Strikethrough,
    Subscript,
    Superscript,
    Table,
    TableCaption,
    TableCellProperties,
    TableColumnResize,
    TableLayout,
    TableProperties,
    TableToolbar,
    TextPartLanguage,
    TextTransformation,
    TodoList,
    Underline,
    WordCount,
  ],
  balloonToolbar: [
    "bold",
    "italic",
    "|",
    "insertImage",
    "|",
    "bulletedList",
    "numberedList",
  ],
  blockToolbar: [
    "fontSize",
    "fontColor",
    "fontBackgroundColor",
    "|",
    "bold",
    "italic",
    "|",
    "insertImage",
    "insertTable",
    "insertTableLayout",
    "|",
    "bulletedList",
    "numberedList",
    "outdent",
    "indent",
  ],
  fontFamily: {
    supportAllValues: true,
  },
  fontSize: {
    options: [10, 12, 14, "default", 18, 20, 22],
    supportAllValues: true,
  },
  heading: {
    options: [
      {
        model: "paragraph",
        title: "Đoạn văn",
        class: "ck-heading_paragraph",
      },
      {
        model: "heading1",
        view: "h1",
        title: "Tiêu đề 1",
        class: "ck-heading_heading1",
      },
      {
        model: "heading2",
        view: "h2",
        title: "Tiêu đề 2",
        class: "ck-heading_heading2",
      },
      {
        model: "heading3",
        view: "h3",
        title: "Tiêu đề 3",
        class: "ck-heading_heading3",
      },
      {
        model: "heading4",
        view: "h4",
        title: "Tiêu đề 4",
        class: "ck-heading_heading4",
      },
      {
        model: "heading5",
        view: "h5",
        title: "Tiêu đề 5",
        class: "ck-heading_heading5",
      },
      {
        model: "heading6",
        view: "h6",
        title: "Tiêu đề 6",
        class: "ck-heading_heading6",
      },
    ],
  },
  htmlSupport: {
    allow: [
      {
        name: /^.*$/,
        styles: true,
        attributes: true,
        classes: true,
      },
      {
        name: "img",
        classes: ["img-fluid"],
        attributes: true,
        styles: true,
      },
    ],
  },
  image: {
    toolbar: [
      "toggleImageCaption",
      "imageTextAlternative",
      "|",
      "imageStyle:inline",
      "imageStyle:wrapText",
      "imageStyle:breakText",
      "|",
      "resizeImage",
    ],
  },
  initialData: "",
  licenseKey: LICENSE_KEY,
  list: {
    properties: {
      styles: true,
      startIndex: true,
      reversed: true,
    },
  },
  mention: {
    feeds: [
      {
        marker: "@",
        feed: [],
      },
    ],
  },
  placeholder: "Nhập hoặc dán nội dung của bạn tại đây!",
  table: {
    contentToolbar: [
      "tableColumn",
      "tableRow",
      "mergeTableCells",
      "tableProperties",
      "tableCellProperties",
    ],
  },
  ui: {
    viewportOffset: {
      top: 0,
    },
  },
};

class CustomUploadAdapter {
  constructor(loader) {
    this.loader = loader;
  }

  upload() {
    return this.loader.file.then(
      (file) =>
        new Promise((resolve, reject) => {
          const data = new FormData();
          data.append("file", file);

          fetch(`/api/files`, {
            method: "POST",
            body: data,
            credentials: "include",
          })
            .then((response) => response.json())
            .then((data) => {
              if (data.code == "UPLOAD_SUCCESS") {
                this.loader.fileId = data.id || data.url;
                resolve({
                  default: data.url,
                  attributes: {
                    class: "img-fluid",
                  },
                });
              } else {
                reject(data.message || "Lỗi khi tải lên hình ảnh");
              }
            })
            .catch((error) => reject(error.message));
        })
    );
  }
}

function CustomUploadAdapterPlugin(editor) {
  editor.plugins.get("FileRepository").createUploadAdapter = (loader) => {
    return new CustomUploadAdapter(loader);
  };
}

DecoupledEditor.create(document.querySelector("#editor"), {
  ...editorConfig,
  extraPlugins: [CustomUploadAdapterPlugin],
})
  .then((editor) => {
    window.editor = editor;
    const wordCount = editor.plugins.get("WordCount");
    document
      .querySelector("#editor-word-count")
      .appendChild(wordCount.wordCountContainer);

    document
      .querySelector("#editor-toolbar")
      .appendChild(editor.ui.view.toolbar.element);
  })
  .catch((error) => {
    console.error("Lỗi khi khởi tạo trình chỉnh sửa:", error);
  });

const editorElement = document.querySelector("#editor");
if (editorElement) {
  editorElement.style.padding = "5px 20px";
}
