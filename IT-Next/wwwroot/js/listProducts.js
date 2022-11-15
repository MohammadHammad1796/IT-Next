const apiUrl = "/api/subCategories/";
let categoryName = $(".breadcrumb li:eq(1)").text().trim(),
  subCategoryName = $(".breadcrumb li:eq(2)").text().trim();
var query = {
  pageNumber: 1,
  pageSize: 12,
  sortBy: "id",
  isSortAscending: false,
};

var entries = {
  from: 0,
  to: 0,
  all: 0,
};

let lastQuery = null;
let searchStarted = false;

function ResetQuery(queryObject) {
  queryObject.pageNumber = 1;
  queryObject.pageSize = 12;
  queryObject.sortBy = "id";
  queryObject.isSortAscending = false;
}

function UpdateQueryFromQueryStringValues(queryObject) {
  const params = new URL(document.location).searchParams;
  let queryChanged = false;
  for (let parameter of params.entries()) {
    const key = parameter[0];
    let value = parameter[1];
    switch (key.toLowerCase()) {
      case "pageNumber".toLowerCase():
        value = parseInt(value);
        if (!isNaN(value))
          if (value < 1) queryObject.pageNumber = 1, queryChanged = true;
          else queryObject.pageNumber = value;
        break;
      case "pageSize".toLowerCase():
        value = parseInt(value);
        if (!isNaN(value))
          if (value < 10) queryObject.pageSize = 12, queryChanged = true;
          else if (value > 100) queryObject.pageSize = 96, queryChanged = true;
          else queryObject.pageSize = value;
        break;
      case "sortBy".toLowerCase():
        switch (value.trim()) {
          case "id":
          case "name":
          case "brand":
          case "price":
            queryObject.sortBy = value;
            break;
          default:
            queryObject.sortBy = "id", queryChanged = true;
        }
        break;
      case "isSortAscending".toLowerCase():
        if (value.trim() == "true") queryObject.isSortAscending = true;
        else queryObject.isSortAscending = false;
        break;
      case "minimumPrice".toLowerCase():
        value = parseFloat(value);
        if (!isNaN(value)) queryObject.minimumPrice = value;
        break;
      case "maximumPrice".toLowerCase():
        value = parseFloat(value);
        if (!isNaN(value)) queryObject.maximumPrice = value;
        break;
      case "searchQuery".toLowerCase():
        value = value.trim();
        const regex = new RegExp("[ ]{2,}", "g");
        value = value.replace(regex, " ");
        if (value.length > 50) queryObject.searchQuery = value.substr(0, 50), queryChanged = true;
        else if (value.length >= 2) queryObject.searchQuery = value;
        break;
      case "brandsIds".toLowerCase():
        value = parseInt(value);
        if (!isNaN(value)) {
          if ("brandsIds" in queryObject) {
            if (!queryObject.brandsIds.includes(value)) {
              queryObject.brandsIds.push(value);
              queryObject.brandsIds.sort();
            }
          } else queryObject.brandsIds = [value];
        }
        break;
    }
  }
  return queryChanged;
}

function GetData(url, queryObject) {
  let result;
  $.ajax({
    url: url,
    type: "GET",
    traditional : true,
    data: queryObject,
    async: false,
    statusCode: {
      200: function (response) {
        result = response;
      },
    },
  });
  return result;
}

function RenderProducts(products) {
  $("#products").empty();
  for (let product of products) {
    const productLink = `/products/${categoryName}/${subCategoryName}/${product.name}`;
    let item = `<div class="col-md-4 col-sm-6 col-xs-12 margin_bottom_30_all">
                        <div class="product_list" onclick="location.href='${productLink}';" style="cursor:pointer;">
                            <div class="product_img"> <img class="img-responsive" src="${product.imagePath}"
                                alt="${product.brand} ${product.name}">
                            </div>
                            <div class="product_detail_btm">
                                <div class="center">
                                    <h4>
                                        <a href="${productLink}">${product.brand} ${product.name}</a>
                                    </h4>
                                </div>
                                <div class="product_price">
                                    <p>
            `;
    if (product.newPrice === product.oldPrice)
      item += `
                                        <span class="new_price">$${product.oldPrice.toFixed(
                                          2
                                        )}</span>
                    `;
    else
      item += `
                                        <span class="old_price">$${product.oldPrice.toFixed(
                                          2
                                        )}</span>
                                         - 
                                        <span class="new_price">${product.newPrice.toFixed(
                                          2
                                        )}</span>
                    `;
    item += `   
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
    $("#products").append(item);
  }
}

function RenderPaginationButtons(pageNumber, pageSize, count) {
  const availablePages =
    count % pageSize === 0
      ? Math.floor(count / pageSize)
      : Math.floor(count / pageSize) + 1;
  let minPage, maxPage;
  if (pageNumber <= 3) minPage = 1;
  else minPage = pageNumber - 2;
  maxPage = minPage + 4;
  if (maxPage > availablePages) maxPage = availablePages;
  if (maxPage - minPage < 4) {
    if (maxPage - minPage < availablePages) {
      minPage = 1;
    }
  }

  $("#pagination input").remove();
  for (let current = maxPage; current >= minPage; current--) {
    let button = `<input type='button' value='${current}' class='btn me-1 ms-1 `;
    if (current === parseInt(pageNumber)) button += "active";
    button += "' ";
    if (current === parseInt(pageNumber)) button += "disabled='disabled'";
    button += " />";
    $("#pagination a:eq(0)").after(button);
  }
  if (pageNumber - 1 >= minPage) {
    $("#pagination a:eq(0)").val(pageNumber - 1);
    $("#pagination a:eq(0)").prop("href", "#");
  } else {
    $("#pagination a:eq(0)").val(0);
    $("#pagination a:eq(0)").removeAttr("href");
  }
  if (parseInt(pageNumber) + 1 <= maxPage) {
    $("#pagination a:eq(1)").val(parseInt(pageNumber) + 1);
    $("#pagination a:eq(1)").prop("href", "#");
  } else {
    $("#pagination a:eq(1)").val(0);
    $("#pagination a:eq(1)").removeAttr("href");
  }
}

function CalculateEntries(
  count,
  queryObject,
  currentItemsCount,
  entriesObject
) {
  entriesObject.from = queryObject.pageSize * (queryObject.pageNumber - 1);
  if (currentItemsCount) entriesObject.from++;
  entriesObject.to =
    currentItemsCount < queryObject.pageSize
      ? queryObject.pageSize * (queryObject.pageNumber - 1) + currentItemsCount
      : queryObject.pageSize * queryObject.pageNumber;
  entriesObject.all = count;
}

function RenderEntriesInfo(entriesObject) {
  $("#countInfo").text(
    `Showing ${entriesObject.from} to ${entriesObject.to} of ${
      entriesObject.all
    } ${entriesObject.all > 1 ? "products" : "product"}`
  );
}

function LoadProducts(queryObject) {
  let resource = GetData(`${apiUrl}${subCategoryName}/products`, queryObject);
  if (!resource.items.length) {
    if (parseInt(queryObject.pageNumber) !== 1 && resource.total) {
      queryObject.pageNumber = 1;
      LoadProducts(queryObject);
      UpdateUrlFromQuery(queryObject);
      return;
    } else if (Object.keys(queryObject).length > 4) {
      $("#products").html("<p>There is no search result</p>");
      $("#products").siblings().hide();
      searchStarted = true;
    }
    else {
        $("#products").html("<p>There is no available products</p>");
        $("#products").siblings().hide();
        $("#filters").hide();
    }
  }
  else {
    searchStarted = true;
    RenderProducts(resource.items);
    RenderPaginationButtons(
        queryObject.pageNumber,
        queryObject.pageSize,
        resource.total
    );
    CalculateEntries(resource.total, query, resource.items.length, entries);
    RenderEntriesInfo(entries);

    $("#products").show();
    $("#products").siblings().show();
    $("#filters").show();
  }

  resource = null;
}

function RenderRelatedSubCategories() {
    let relatedSubCategories = GetData(`${apiUrl}${subCategoryName}/related`);
  if (relatedSubCategories.length) {
    let newHtml = `
            <div class="side_bar_blog">
                <h4>Related Categories</h4>
                <div class="tags">
                    <ul>
        `;
    for (let subCategory of relatedSubCategories) {
      newHtml += `
                    <li>
                        <a href="/products/${categoryName}/${subCategory}">${subCategory}</a>
                    </li>
            `;
    }
    newHtml += `
                    </ul>
                </div>
            </div>
        `;
    $("#relatedSubCategories").html(newHtml).show();
  }
  else
    $("#relatedSubCategories").html("").hide();
}

function RenderFilters() {
    if ("searchQuery" in query)
        $("#searchQuery").val(query.searchQuery);

    let brands = GetData(`${apiUrl}${subCategoryName}/products/brands`);
    if (brands.length > 1) {
      $("#brands").empty();
      $("#brands").append("<h4>Brands</h4>");
      for (let brand of brands) {
        let item = `
                    <div class="form-check mb-2">
                        <input class="form-check-input" type="checkbox" value="${brand.id}" name="BrandsIds">
                        <label class="form-check-label" for="BrandsIds">${brand.name}</label>
                    </div>
                `;
        $("#brands").append(item);
      }
      if ("brandsIds" in query) {
        for (let brand of brands){
            if (query.brandsIds.includes(brand.id)){
                $(`input[name='BrandsIds'][value='${brand.id}']`).prop("checked", "checked");}}

        for (let brandId of query.brandsIds)
              if (!brands.map(b => b.id).includes(brandId))
                query.brandsIds.splice(query.brandsIds.indexOf(brandId), 1);

        if ($("input[name='BrandsIds']:checked").length === $("input[name='BrandsIds']").length){
          delete query.brandsIds;
            $("input[name='BrandsIds']:checked").each(function () {
                $(this).removeAttr("checked");
            });}
      }

      $("#brands").show();
    }
    else
        $("#brands").empty().hide();

    let priceRange = GetData(
      `${apiUrl}${subCategoryName}/products/minimumAndMaximumPrice`
    );
    if (priceRange.minimum !== priceRange.maximum) {
      $("#priceRange").html(`
                <h4>Price</h4>
                <div class="range_container">
                    <div class="row mb-2">
                        <p id="fromPrice" class="col-6" data-min="${priceRange.minimum.toFixed(
                          2
                        )}" data-current="${priceRange.minimum.toFixed(
        2
      )}">$${priceRange.minimum.toFixed(2)}</p>
                        <p id="toPrice" class="col-6 text-right" data-max="${priceRange.maximum.toFixed(
                          2
                        )}" data-current="${priceRange.maximum.toFixed(
        2
      )}">$${priceRange.maximum.toFixed(2)}</p>
                    </div>
                    <div class="sliders_control">
                        <input id="fromSlider" type="range" value="0" min="0" max="100" />
                        <input id="toSlider" type="range" value="100" min="0" max="100" />
                    </div>
                </div>
            `);
      ConfigurePriceRangeStyle(priceRange.minimum, priceRange.maximum);

      if ("minimumPrice" in query) {
        if (Number(query.minimumPrice) === Number(priceRange.minimum))
            delete query.minimumPrice;
        else {
            $("#fromSlider").val(parseInt((query.minimumPrice - priceRange.minimum) / ((priceRange.maximum - priceRange.minimum) / 100)));
            $("#fromPrice").attr("data-current", query.minimumPrice.toFixed(2));
            $("#fromPrice").text(`$${query.minimumPrice.toFixed(2)}`);
            fillSlider($("#fromSlider")[0], $("#toSlider")[0], "#C6C6C6", "#007bff", $("#toSlider")[0]);
        }
      }
      if ("maximumPrice" in query) {
        if (Number(query.maximumPrice) === Number(priceRange.maximum))
            delete query.maximumPrice;
        else {
            $("#toSlider").val(parseInt((query.maximumPrice - priceRange.minimum) / ((priceRange.maximum - priceRange.minimum) / 100)));
            $("#toPrice").attr("data-current", query.maximumPrice.toFixed(2));
            $("#toPrice").text(`$${query.maximumPrice.toFixed(2)}`);
            fillSlider($("#fromSlider")[0], $("#toSlider")[0], "#C6C6C6", "#007bff", $("#toSlider")[0]);
        }
      }

      $("#priceRange").show();
    }
    else
        $("#priceRange").empty().hide();
}

function controlFromSlider(fromSlider, toSlider, minimum, maximum) {
  let [from, to] = getParsed(fromSlider, toSlider);
  fillSlider(fromSlider, toSlider, "#C6C6C6", "#007bff", toSlider);

  if (from > to) (fromSlider.value = to), (from = to);
  const current = (minimum + ((maximum - minimum) * from) / 100).toFixed(2);
  $("#fromPrice").attr("data-current", current);
  $("#fromPrice").text(`$${current}`);
}

function controlToSlider(fromSlider, toSlider, minimum, maximum) {
  let [from, to] = getParsed(fromSlider, toSlider);
  fillSlider(fromSlider, toSlider, "#C6C6C6", "#007bff", toSlider);
  setToggleAccessible(toSlider);

  if (to < from) (toSlider.value = from), (to = from);
  const current = (minimum + ((maximum - minimum) * to) / 100).toFixed(2);
  $("#toPrice").attr("data-current", current);
  $("#toPrice").text(`$${current}`);
}

function getParsed(currentFrom, currentTo) {
  const from = parseInt(currentFrom.value, 10);
  const to = parseInt(currentTo.value, 10);
  return [from, to];
}

function fillSlider(from, to, sliderColor, rangeColor, controlSlider) {
  const rangeDistance = to.max - to.min;
  const fromPosition = from.value - to.min;
  const toPosition = to.value - to.min;
  controlSlider.style.background = `linear-gradient(
              to right,
              ${sliderColor} 0%,
              ${sliderColor} ${(fromPosition / rangeDistance) * 100}%,
              ${rangeColor} ${(fromPosition / rangeDistance) * 100}%,
              ${rangeColor} ${(toPosition / rangeDistance) * 100}%, 
              ${sliderColor} ${(toPosition / rangeDistance) * 100}%, 
              ${sliderColor} 100%)`;
}

function setToggleAccessible(currentTarget) {
  const toSlider = document.querySelector("#toSlider");
  if (Number(currentTarget.value) <= 0) {
    toSlider.style.zIndex = 2;
  } else {
    toSlider.style.zIndex = 0;
  }
}

function ConfigurePriceRangeStyle(minimum, maximum) {
  const fromSlider = document.querySelector("#fromSlider");
  const toSlider = document.querySelector("#toSlider");
  fillSlider(fromSlider, toSlider, "#C6C6C6", "#007bff", toSlider);
  setToggleAccessible(toSlider);

  fromSlider.oninput = () =>
    controlFromSlider(fromSlider, toSlider, minimum, maximum);
  toSlider.oninput = () =>
    controlToSlider(fromSlider, toSlider, minimum, maximum);
}

$(window).load(function () {
  $("#apply").prop("disabled", "disabled");
  const queryChanged = UpdateQueryFromQueryStringValues(query);
  if (queryChanged)
    UpdateUrlFromQuery(query);
  LoadProducts(query);
  if (searchStarted)
    RenderFilters();
  searchStarted = false;
  RenderRelatedSubCategories();
  $("#sortBy").val(query.sortBy);
  if (query.isSortAscending)
    $("#sortAsc").prop('checked', 'checked');
    else
        $("#sortDesc").prop('checked', 'checked');
    if ($(`#pageSize option[value='${query.pageSize}']`).length)
        $("#pageSize").val(query.pageSize);
    else
        $("#pageSize").append(`<option value="${query.pageSize}">${query.pageSize}</option>`);
  lastQuery = Object.assign({}, query);

  $("#pageSize, #sortBy").on("change", function () {
    const id = $(this).prop("id");
    const value = $(this).val();
    switch (id) {
      case "pageSize":
          query.pageSize = parseInt(value);
        break;
      case "sortBy":
        query.sortBy = value;
        break;
    }
    LoadProducts(query);
    UpdateUrlFromQuery(query);
    lastQuery = Object.assign({}, query);
  });
  $("#sortAsc, #sortDesc").on("change", function () {
    query.isSortAscending = !query.isSortAscending;
    UpdateUrlFromQuery(query);
    LoadProducts(query);
    lastQuery = Object.assign({}, query);
  });
  $("#pagination").on("click", "a, input[type='button']", function () {
    const value = $(this).val();
    query.pageNumber = parseInt(value);
    UpdateUrlFromQuery(query);
    LoadProducts(query);
    lastQuery = Object.assign({}, query);
  });

  $("#relatedSubCategories").on("click", "a", function (e) {
    e.preventDefault();
    $(`#navbar_menu a.active:contains(${subCategoryName})`).removeClass("active");
    let subCategoryUrl = $(this).attr("href");
    subCategoryName =
      subCategoryUrl.split("/")[subCategoryUrl.split("/").length - 1];
    $(`#navbar_menu a:contains(${subCategoryName})`).addClass("active");
    $(".breadcrumb li:eq(2) a").text(subCategoryName);
    subCategoryUrl = `${subCategoryUrl.substr(
      0,
      subCategoryUrl.lastIndexOf("/") + 1
    )}${subCategoryName}`;
    $(".breadcrumb li:eq(2) a").prop("href", subCategoryUrl);
    document.title = `IT-Next - Products ${subCategoryName}`;
    history.pushState(null, "", subCategoryUrl);
    ResetQuery(query);
    
    LoadProducts(query);
    if (searchStarted)
      RenderFilters();
    searchStarted = false;
    RenderRelatedSubCategories();
    $("#sortBy").val(query.sortBy);
    if (query.isSortAscending)
      $("#sortAsc").prop('checked', 'checked');
      else
          $("#sortDesc").prop('checked', 'checked');
      if ($(`#pageSize option[value='${query.pageSize}']`).length)
          $("#pageSize").val(query.pageSize);
      else
          $("#pageSize").append(`<option value="${query.pageSize}">${query.pageSize}</option>`);
    lastQuery = Object.assign({}, query);

    $("#apply").prop("disabled", "disabled");

  });

  ConfigureFiltersQuery();

  $("#filters").on("submit", function (e) {
    e.preventDefault();
    if (
        JSON.stringify(query, Object.keys(query).sort()) ===
        JSON.stringify(lastQuery, Object.keys(lastQuery).sort())
      )
      return;
    
    lastQuery = Object.assign({}, query);
    LoadProducts(query);
    UpdateUrlFromQuery(query);
    $("#apply").prop("disabled", "disabled");
  });
});

function ConfigureFiltersQuery() {
  $("#filters").on("input", "input", function () {
    const elementId = $(this).attr("id");
    switch (elementId) {
      case "searchQuery":
        ConfigureSearchQuery($(this).val(), query);
        break;
      case "fromSlider":
      case "toSlider":
        ConfigurePriceQuery(query);
        break;
      default:
        ConfigureBrandsQuery(query);
    }
    if (
      JSON.stringify(query, Object.keys(query).sort()) ===
      JSON.stringify(lastQuery, Object.keys(lastQuery).sort())
    )
      $("#apply").prop("disabled", "disabled");
    else $("#apply").removeAttr("disabled");
  });
}

function ConfigureBrandsQuery(queryObject) {
  let brandsIds = [];
  const availableBrandsCount = $("#brands input[type='checkbox']").length;
  const selectedBrands = $(
    "#brands input[type='checkbox'][name='BrandsIds']:checked"
  );
  selectedBrands.each(function () {
    const brandId = Number($(this).val());
    brandsIds.push(brandId);
  });

  if ("brandsIds" in queryObject) {
    if (!brandsIds.length) delete queryObject.brandsIds;
    else if (
      queryObject.brandsIds.slice().sort().join(",") !==
      brandsIds.slice().sort().join(",")
    )
      if (selectedBrands.length !== availableBrandsCount)
        queryObject.brandsIds = brandsIds.sort();
      else delete queryObject.brandsIds;
  } else if (
    selectedBrands.length &&
    selectedBrands.length !== availableBrandsCount
  )
    queryObject.brandsIds = brandsIds.sort();
}

function ConfigurePriceQuery(queryObject) {
  const minimumPrice = Number($("#fromPrice").attr("data-current"));
  const minimumAllowedPrice = Number($("#fromPrice").attr("data-min"));
  const maximumPrice = Number($("#toPrice").attr("data-current"));
  const maximumAllowedPrice = Number($("#toPrice").attr("data-max"));
  if (
    minimumPrice === minimumAllowedPrice &&
    maximumPrice === maximumAllowedPrice
  ) {
    if ("minimumPrice" in queryObject) delete queryObject.minimumPrice;
    if ("maximumPrice" in queryObject) delete queryObject.maximumPrice;
  } else if (minimumPrice === maximumPrice) {
    queryObject.minimumPrice = minimumPrice;
    queryObject.maximumPrice = maximumPrice;
  } else {
    if (minimumPrice === minimumAllowedPrice) {
      if ("minimumPrice" in queryObject) delete queryObject.minimumPrice;
    } else queryObject.minimumPrice = minimumPrice;
    if (maximumPrice === maximumAllowedPrice) {
      if ("maximumPrice" in queryObject) delete queryObject.maximumPrice;
    } else queryObject.maximumPrice = maximumPrice;
  }
}

function ConfigureSearchQuery(value, queryObject) {
  let searchText = value.trim();
  const regex = new RegExp("[ ]{2,}", "g");
  searchText = searchText.replace(regex, " ");
  if ("searchQuery" in queryObject) {
    if (searchText !== queryObject.searchQuery)
      if (searchText.length < 2) delete queryObject.searchQuery;
      else queryObject.searchQuery = searchText;
  } else if (searchText.length > 1) queryObject.searchQuery = searchText;
}

function UpdateUrlFromQuery(queryObject) {
    let newUrl = new URL(window.location.origin.concat(window.location.pathname));
    let params = new URLSearchParams();
    for (let key in queryObject) {
        if (!Array.isArray(queryObject[key]))
            params.append(key, queryObject[key]);
        else
            for (let element of queryObject[key])
                params.append(key, element);
    }
    newUrl = newUrl.href.concat("?", params.toString());
    history.pushState(null, "", newUrl);
}