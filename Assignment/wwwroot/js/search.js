function search() {
    const searchValue = document.getElementById("search").value.trim();
    const minValue = document.getElementById("min").value;
    const maxValue = document.getElementById("max").value;

    const customEncodeUriComponent = (value) => {
        return encodeURIComponent(value).replace(/%20/g, "+");
    };

    const params = [];
    if (searchValue) params.push(`text=${customEncodeUriComponent(searchValue)}`);
    if (minValue && !isNaN(minValue))
        params.push(`min=${minValue}`);
    if (maxValue && !isNaN(maxValue))
        params.push(`max=${maxValue}`);

    const queryString = params.length > 0 ? params.join("&") : "";
    if (queryString) {
        location.href = `/products/search?${queryString}`;
    } else {
        location.href = "/";
    }
}

function resetSearch() {
    document.getElementById("min").value = "";
    document.getElementById("max").value = "";
    search();
}