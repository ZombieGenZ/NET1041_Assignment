let selectedProduct = 0;

function startCreate() {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Tạo sản đánh giá";
  document.getElementById("createAndUpdateButton").textContent = "Tạo đánh giá";
  document.getElementById("createAndUpdateButton").classList.add("btn-success");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", create);
}

function create() {
  const star = document.getElementById("star").value;
  const content = window.editor.getData();

  if (!star) {
    showWarningToast(`Vui lòng nhập số sao đánh giá`, 4000);
    return;
  }

  fetch("/api/evaluate", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Star: star,
      Content: content,
      ProductId: selectedProduct,
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi tạo đánh giá. Vui lòng thử lại sau.",
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

      if (data.code == "CREATE_SUCCESS") {
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
        setTimeout(() => location.reload(), 1000);
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

let selectedIds = 0;

function startUpdate(id, star, content) {
  clearEvent();
  clearClass();
  clearInput();
  document.getElementById("createAndUpdateModalLabel").textContent =
    "Cập nhật đánh giá";
  document.getElementById("createAndUpdateButton").textContent =
    "Cập nhật đánh giá";
  document.getElementById("createAndUpdateButton").classList.add("btn-warning");
  document
    .getElementById("createAndUpdateButton")
    .addEventListener("click", update);

  selectedIds = id;
  document.getElementById("star").value = star;
  window.editor.setData(content);
}

function update() {
  const star = document.getElementById("star").value;
  const content = window.editor.getData();

  if (!star) {
    showWarningToast(`Vui lòng nhập số sao đánh giá`, 4000);
    return;
  }

  fetch(`/api/evaluate/${selectedIds}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      Star: star,
      Content: content,
    }),
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi sửa đánh giá. Vui lòng thử lại sau.",
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

      if (data.code == "UPDATE_SUCCESS") {
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
        setTimeout(() => location.reload(), 1000);
        showSuccessToast(data.message, 4000);
        return;
      } else {
        showErrorToast(data.message, 4000);
      }
    });
}

function startDelete(id) {
  selectedIds = id;
}

function del() {
  fetch(`/api/evaluate/${selectedIds}`, {
    method: "DELETE",
  })
    .then((response) => {
      if (!response.ok) {
        if (response.status === 422 || response.status === 401) {
          return response.json();
        }

        if (!response.ok) {
          return showErrorToast(
            "Lỗi khi xóa đánh giá. Vui lòng thử lại sau.",
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

      if (data.code == "DELETE_SUCCESS") {
        clearInput();
        const createAndUpdateModal = document.getElementById("deleteModal");
        const modal = bootstrap.Modal.getInstance(createAndUpdateModal);
        if (modal) {
          modal.hide();
        } else {
          new bootstrap.Modal(createModal).hide();
        }
        setTimeout(() => location.reload(), 1000);
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
  document.getElementById("star").value = "5";
  window.editor.setData("");
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
