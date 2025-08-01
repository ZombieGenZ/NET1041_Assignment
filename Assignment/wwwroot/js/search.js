function search() {
    const searchValue = document.getElementById("search").value.trim();
    const minValue = document.getElementById("min")?.value;
    const maxValue = document.getElementById("max")?.value;
    const categoryId = document.getElementById("categoryId")?.value;
    const priceSort = document.getElementById("priceSort")?.value;
    const ratingSort = document.getElementById("ratingSort")?.value;
    const salesSort = document.getElementById("salesSort")?.value;

    const customEncodeUriComponent = (value) => {
        return encodeURIComponent(value).replace(/%20/g, "+");
    };

    const params = [];

    if (searchValue) {
        params.push(`text=${customEncodeUriComponent(searchValue)}`);
    }
    if (minValue && !isNaN(minValue) && minValue !== "") {
        params.push(`min=${minValue}`);
    }
    if (maxValue && !isNaN(maxValue) && maxValue !== "") {
        params.push(`max=${maxValue}`);
    }
    if (categoryId && categoryId !== "") {
        params.push(`categoryId=${categoryId}`);
    }
    if (priceSort && priceSort !== "") {
        params.push(`priceSort=${priceSort}`);
    }
    if (ratingSort && ratingSort !== "") {
        params.push(`ratingSort=${ratingSort}`);
    }
    if (salesSort && salesSort !== "") {
        params.push(`salesSort=${salesSort}`);
    }

    const queryString = params.length > 0 ? params.join("&") : "";
    location.href = `/?${queryString}`;
}

function resetSearch() {
    const minField = document.getElementById("min");
    const maxField = document.getElementById("max");
    const categoryField = document.getElementById("categoryId");
    const priceSortField = document.getElementById("priceSort");
    const ratingSortField = document.getElementById("ratingSort");
    const salesSortField = document.getElementById("salesSort");

    if (minField) minField.value = "";
    if (maxField) maxField.value = "";
    if (categoryField) categoryField.value = "";
    if (priceSortField) priceSortField.value = "";
    if (ratingSortField) ratingSortField.value = "";
    if (salesSortField) salesSortField.value = "";

    search();
}

document.addEventListener('DOMContentLoaded', function () {
    const numberInputs = ['min', 'max'];
    numberInputs.forEach(inputId => {
        const element = document.getElementById(inputId);
        if (element) {
            let timeout;
            element.addEventListener('input', function () {
                clearTimeout(timeout);
                timeout = setTimeout(() => {
                    search();
                }, 500);
            });
        }
    });

    const dropdowns = ['categoryId', 'priceSort', 'ratingSort', 'salesSort'];
    dropdowns.forEach(dropdownId => {
        const element = document.getElementById(dropdownId);
        if (element) {
            element.addEventListener('change', function () {
                search();
            });
        }
    });

    const searchInput = document.getElementById("search");
    if (searchInput) {
        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                search();
            }
        });
    }
});