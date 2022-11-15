const apiUrl = "/api/products/";
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
var counter = 1;
var selectedSubCategory = null;
var selectedBrand = null;
var $ = window.$;

var query = {
  pageNumber: 1,
  pageSize: 10,
  isSortAscending: true,
  sortBy: "name",
  searchQuery: ""
};
var entries = {
  from: 0,
  to: 0,
  all: 0
};
var itemId = null;
var resource = null;

function matchCase(text, pattern) {
	var result = "";

	for(let i = 0; i < text.length; i++) {
		const c = text.charAt(i);
		const p = pattern.charCodeAt(i);

		if(p >= 65 && p < 65 + 26) {
			result += c.toUpperCase();
		} else {
			result += c.toLowerCase();
		}
	}

	return result;
}

function GetData(queryObject) {
  let result;
  $.ajax({
	url: apiUrl,
	type: "GET",
	data: queryObject,
	async: false,
	statusCode: {
	  200: function (response) {
		result = response;
	  }
	}
  });
  return result;
}

function RenderItemsTable(items, table) {
  const tbody = $(table).find("tbody:eq(0)");
  tbody.empty();
  let isOdd = false;
  for (let products of items) {
	if (!isOdd) isOdd = true;
	else isOdd = false;
	AddRow(table, products, false, isOdd);
  }
}

function AddRow(table, product, ignoreTimeZone, isOdd) {
  if (isOdd == null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? true : false; 
  let row = `<tr id='item_${product.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Name'>${product.name}</td>
						<td head='Category'>${product.categoryName}</td>
						<td head='SubCategory' data-id="${product.subCategoryId}">${product.subCategoryName}</td>
						<td head='Brand' data-id="${product.brandId}">${product.brandName}</td>
						<td head='Price'>${product.price}</td>
						<td head='Discount'>${product.discount}</td>
						<td head='Quantity'>${product.quantity}</td>
						<td head='Last Update' class="notsearchable">${FormatDateTime(GetDateTimeObjectAsClientZone(product.lastUpdate, ignoreTimeZone), "dd-MM-yyyy hh:mm a")}</td>
						<td head='Image' class="imageColumn"><img src="${product.imagePath}" style="width:100%;"/></td>
						<td head='Description'>${product.description}</td>
						<td head='Actions'>
						<a href="/products/${product.categoryName}/${product.subCategoryName}/${product.name}" target="_blank" data-id="1"><i class="fas fa-eye me-3 mb-3"></i></a>
						<a href="#" class="edit-btn" data-id="${product.id}"><i class="fas fa-edit me-3 mb-3"></i></a>
						<a href="#" class="remove-btn" data-id="${product.id}"><i class="fas fa-trash"></i></a>
						</td>
						</tr>`;
  $(table).find("tbody:eq(0)").append(row);
}

function CalculateEntries(
  count,
  queryObject,
  currentItemsCount,
  entriesObject
) {
  entriesObject.from = queryObject.pageSize * (queryObject.pageNumber - 1);
  if (currentItemsCount)
	entriesObject.from++;
  entriesObject.to =
	currentItemsCount < queryObject.pageSize
	  ? queryObject.pageSize * (queryObject.pageNumber - 1) + currentItemsCount
	  : queryObject.pageSize * queryObject.pageNumber;
  entriesObject.all = count;
}

function RenderEntriesInfo(entriesObject) {
  $("#countInfo").text(
	`Showing ${entriesObject.from} to ${entriesObject.to} of ${entriesObject.all} entries`
  );
}

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#Id").val(0);
  $("#SubCategoryId").val("");
  $("#SubCategoryName").typeahead("val", "");
  $("#BrandId").val("");
  $("#BrandName").typeahead("val", "");
  selectedSubCategory = null;
  selectedBrand = null;
  itemId = null;
  resource = null;

  const validator = $(itemForm).validate();

  const errors = $(itemForm).find(".field-validation-error span");
  errors.each(function() {
	   validator.settings.success($(this));
	   $(this).remove();
  });

  $(".field-validation-valid span").remove();
  validator.resetForm();
}

function LoadData(queryObject, table) {
  resource = GetData(queryObject);
  if (!resource.items.length && parseInt(queryObject.pageNumber) !== 1) {
	queryObject.pageNumber = 1;
	resource = GetData(queryObject);
  }
  RenderItemsTable(resource.items, table);
  HighlightSearchResult(table, queryObject);
  RenderPaginationButtons(
	queryObject.pageNumber,
	queryObject.pageSize,
	resource.total
  );
  CalculateEntries(resource.total, query, resource.items.length, entries);
  RenderEntriesInfo(entries);
  resource = null;
}

function ConfigureSubmit(id, table) {
  $(`#${id}`).submit(function (e) {
	e.preventDefault();

	const productResource = getFormData($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(productResource, submitType, table);
  });
}

function ConfigureEdit(id) {
  $("#items").on("click", "a.edit-btn", function (e) {
	e.preventDefault();
	ClearInputs();
	id = $(this).attr("data-id");
	const tableRow = $(`#item_${id}`);
	$("#Id").val(id);
	const  productName = tableRow.children(":eq(0)").text();
	$("#Name").val(productName);
	const subCategoryId = tableRow.children(":eq(2)").attr("data-id");
	selectedSubCategory = {
		id: subCategoryId,
		name: tableRow.children(":eq(2)").text()
	};
	const brandId = tableRow.children(":eq(3)").attr("data-id");
	selectedBrand = {
		id: brandId,
		name: tableRow.children(":eq(3)").text()
	};

	$("#SubCategoryId").val(selectedSubCategory.id);
	$("#SubCategoryName").val(selectedSubCategory.name);
	$("#BrandId").val(selectedBrand.id);
	$("#BrandName").val(selectedBrand.name);
	$("#Quantity").val(tableRow.children(":eq(6)").text());
	$("#Price").val(tableRow.children(":eq(4)").text());
	$("#Discount").val(tableRow.children(":eq(5)").text());
	$("#Description").val(tableRow.children(":eq(9)").text());
	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit product: ${productName}`);
	$("#manageModal").modal("toggle");
  });
}

function getFormData(form) {
  const unIndexedArray = form.serializeArray();
  var data = new FormData();
  $.map(unIndexedArray, function (n) {
	if (typeof n["value"] === "string")
	  n["value"] = n["value"].trim();
	data.append(n["name"], n["value"]);
  });

  if ($("#Image").val() !== "")
	  data.append("image", $("#Image").get(0).files[0]);

	return data;
}

function DeleteItem(id) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Product not found.");
	  },
	  204: function () {
		$(`#item_${id}`).remove();
		entries.from = entries.to === 1 ? --entries.from : entries.from;
		entries.to--;
		entries.all--;
		if (entries.from > entries.to) {
			query.pageNumber--;
			LoadData(query, itemsTable);
		}
		else
			RenderEntriesInfo(entries);
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Product have been deleted successfully.");
	  }
	}
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");const tableRow = $(`#item_${id}`);
	const productName = tableRow.children(":eq(0)").text();
	const category = tableRow.children(":eq(1)").text();
	const subCategory = tableRow.children(":eq(2)").text();
	const brand = tableRow.children(":eq(3)").text();
	const price = tableRow.children(":eq(4)").text();
	const discount = tableRow.children(":eq(5)").text();
	const quantity = tableRow.children(":eq(6)").text();
	const lastUpdate = tableRow.children(":eq(7)").text();
	const image = tableRow.children(":eq(8)").find("img:eq(0)").attr("src");
	const description = tableRow.children(":eq(9)").text();

	$("#deleteParagraph").html(`<p>Are you sure you want to delete this product?</p>
		<div class="col-md-6 col-8">
		<div>Name: ${productName}</div><div>Category: ${category}</div>
		<div>Subcategory: ${subCategory}</div><div>Brand: ${brand}</div>
		<div>Price: ${price}</div><div>Discount: ${discount}</div>
		<div>Quantity: ${quantity}</div><div>Last Update: ${lastUpdate}</div>
		<div>Description: ${description}</div></div><div class="col-md-6 col-4"><img src="${image}" style="width:95%;"/></div>`);
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id);
  });
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

  $("#paginationSection input").remove();
  for (let current = maxPage; current >= minPage; current--) {
	let button = `<input type='button' value='${current}' class='btn me-1 ms-1 `;
	if (current === parseInt(pageNumber)) button += "active";
	button += "' ";
	if (current === parseInt(pageNumber)) button += "disabled='disabled'";
	button += " />";
	$("#paginationSection a:eq(0)").after(button);
  }
  if (pageNumber - 1 >= minPage) {
	$("#paginationSection a:eq(0)").val(pageNumber - 1);
	$("#paginationSection a:eq(0)").attr("href", "#");
  } else {
	$("#paginationSection a:eq(0)").val(0);
	$("#paginationSection a:eq(0)").removeAttr("href");
  }
  if (parseInt(pageNumber) + 1 <= maxPage) {
	$("#paginationSection a:eq(1)").val(parseInt(pageNumber) + 1);
	$("#paginationSection a:eq(1)").attr("href", "#");
  } else {
	$("#paginationSection a:eq(1)").val(0);
	$("#paginationSection a:eq(1)").removeAttr("href");
  }
}

function EnableSearch(queryObject, table) {
  $("#searchText").on("change paste keyup", function () {
	let searchText = $(this).val().trim();
	searchText = searchText.replace("[ ]{2,}", " ");
	if (searchText.length < 1 || searchText.length > 50)
	  if (queryObject.searchQuery === "") return;
	  else {
		queryObject.searchQuery = "";
		LoadData(queryObject, table);
		return;
	  }

	query.searchQuery = searchText;
	LoadData(queryObject, table);
  });
}

function HighlightSearchResult(table, queryObject) {
	if (queryObject.searchQuery.length < 1 || queryObject.searchQuery.length > 50)
		return;

	let numberQuery;
	if (!isNaN(queryObject.searchQuery)) {
		numberQuery = Number(queryObject.searchQuery) % 1 === 0
			? parseInt(queryObject.searchQuery).toString()
			: parseFloat(queryObject.searchQuery).toString();
	}

	const columns = $(table).find("td:not(.notsearchable)").filter(function() {
		if (numberQuery === undefined)
			return $(this).text().toLowerCase().indexOf(queryObject.searchQuery.toLocaleLowerCase()) > -1;

		return $(this).text().toLowerCase().indexOf(queryObject.searchQuery.toLocaleLowerCase()) > -1 ||
			$(this).text() === numberQuery;
	});
	for (let column of columns)
	{
		const columnText = $(column).text();
		let regex = new RegExp(`(${queryObject.searchQuery})`, "gi");
		let columnNewHtml = columnText.replace(regex, function(match) {
			return `<span>${matchCase(queryObject.searchQuery, match)}</span>`;
		});
		$(column).html(columnNewHtml);

		if (numberQuery !== undefined) {
			regex = new RegExp(`(${numberQuery})`);
			columnNewHtml = columnText.replace(regex, function() {
				return `<span>${numberQuery}</span>`;
			});
			$(column).html(columnNewHtml);
		}
	}
}

function EnableSorting(queryObject, table) {
  $(".sort").on("click", function () {
	$(this).siblings().find('i').removeClass('fa-sort-up fa-sort-down');
	const child = $(this).children("i:eq(0)");
	queryObject.sortBy = $(this).attr('sortby');
	$(`#sortBy option[value='${$(this).attr('sortby')}']`).prop("selected", true);
	if (!child.hasClass("fa-sort-up") && !child.hasClass("fa-sort-down")) {
		child.addClass("fa-sort-down");
	  $("[name='sort'][value='desc']").prop("checked", true);
	  queryObject.isSortAscending = false;
	  LoadData(queryObject, table);
	  return;
	}

	if (child.hasClass("fa-sort-up"))  {
	  queryObject.isSortAscending = false;
	  $("[name='sort'][value='desc']").prop("checked", true);
	}
	else {
	  queryObject.isSortAscending = true;
	  $("[name='sort'][value='asc']").prop("checked", true);
	}

	child.toggleClass("fa-sort-up fa-sort-down");
	LoadData(queryObject, table);
  });
}

function EnablePagination(queryObject, table) {
  $("#pageSize").on("change", function () {
	queryObject.pageSize = $(this).val();
	LoadData(queryObject, table);
  });

  $("#paginationSection").on("click", "input[type=button]", function (e) {
	e.preventDefault();
	if ($(this).hasClass("active")) return;

	queryObject.pageNumber = $(this).val();
	LoadData(queryObject, table);
  });

  $("#paginationSection").on("click", "a", function (e) {
	e.preventDefault();
	if (!$(this).attr("href")) return;

	queryObject.pageNumber = $(this).val();
	LoadData(queryObject, table);
  });
}

function UpdateRow(id, product) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(product.name);
  row.find("td").eq(1).text(product.categoryName);
  row.find("td").eq(2).text(product.subCategoryName);
  row.find("td").eq(2).attr("data-id", product.subCategoryId);
  row.find("td").eq(3).text(product.brandName);
  row.find("td").eq(3).attr("data-id", product.brandId);
  row.find("td").eq(4).text(product.price);
  row.find("td").eq(5).text(product.discount);
  row.find("td").eq(6).text(product.quantity);
  row.find("td").eq(7).text(FormatDateTime(GetDateTimeObjectAsClientZone(product.lastUpdate, true), "dd-MM-yyyy hh:mm a"));
  row.find("td:eq(8)").find("img:eq(0)").attr("src", product.imagePath);
  row.find("td:eq(9)").text(product.description);
  row.find("td:eq(10) > a > i.fa-eye").parent().attr("href", `/products/${product.categoryName}/${product.subCategoryName}/${product.name}`);
}

function EnableMobileSorting(queryObject, table) {
  $("[name='sort']").on("click", function () {

	const value = $(this).val();
	if (value === "asc" && queryObject.isSortAscending === true || 
		value === "desc" && queryObject.isSortAscending === false)
	  return;

	if (value === "asc") {
	  $('.sort').find('i.fa-sort-down').addClass('fa-sort-up').removeClass('fa-sort-down');
	  queryObject.isSortAscending = true;
	  LoadData(queryObject, table);
	  return;
	}
	$('.sort').find('i.fa-sort-up').addClass('fa-sort-down').removeClass('fa-sort-up');
	queryObject.isSortAscending = false;
	LoadData(queryObject, table);
  });

  $("#sortBy").on("click", function() {
	  const value = $(this).val();
	  if (value === queryObject.sortBy)
		  return;

	  $(".sort i").removeClass("fa-sort-up fa-sort-down");
	  if (queryObject.isSortAscending)
		$(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-up");
	  else
		  $(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-down");
	  queryObject.sortBy = value;
	  LoadData(queryObject, table);
  });
}

function SaveItem(resource, saveType, table) {
	switch (saveType) {
	case "Save":
		$.ajax({
			async: false,
			url: apiUrl + $("#Id").val(),
			method: "PUT",
			contentType: false,
			processData: false,
			dataType: "json",
			data: resource,
			statusCode: {
				200: function(product) {
					UpdateRow(`#item_${product.id}`, product);
					const message = "Changes have been saved successfully.";
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification(message);
				},
				400: function(response) {
					const data = JSON.parse(response.responseText);
					$("#manageItems").data("validator").showErrors(data);
				},
				404: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Product not found.");
				},
				204: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Product does not changed.");
				},
				500: function() {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Internal server error, please try again.");
				}
			}
		});
		break;
	case "Add":
		$.ajax({
			async: false,
			url: apiUrl,
			method: "POST",
			contentType: false,
			processData: false,
			dataType: "json",
			data: resource,
			statusCode: {
				200: function (product) {
					AddRow(table, product, true, null);
					entries.from = entries.from === 0 ? ++entries.from : entries.from;
					entries.to++;
					entries.all++;
					RenderEntriesInfo(entries);
					const message = "Product have been added successfully.";
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification(message);
				},
				400: function (response) {
					const data = JSON.parse(response.responseText);
					$("#manageItems").data("validator").showErrors(data);
				},
				500: function () {
					$("#manageModal").modal("toggle");
					window.DisplayToastNotification("Internal server error, please try again.");
				}
			}
		});
		break;
	}
}

$(window).on("load", function () {
  LoadData(query, itemsTable);

  EnableSearch(query, itemsTable);
  EnableSorting(query, itemsTable);
  EnableMobileSorting(query, itemsTable);
  EnablePagination(query, itemsTable);

  let origForm = getFormData($(itemForm));

  $("#new").on("click", function () {
	$('#manageModal button[type="submit"]').text("Add");
	$("#manageModalLabel").text("New Product");
	ClearInputs();
  });

  $("#manageModal").on("shown.bs.modal", function () {
	$(this).find("input:first").focus();
	$("#addBtn").attr("disabled", "disabled");
	origForm = getFormData($(itemForm));
  });

  $("#manageModal, #deleteModal").on("hidden.bs.modal", function () {
	ClearInputs();
  });

  $(itemsTable).on("click", "tr td:first-child", function () {
	if ($(this).css("display") !== "block") return;

	if ($(this).siblings().eq(0).css("display") !== "block") {
		$(this).siblings().show(200,
			function() {
				$(this).css("display", "block");
			});
	  $(this)
		.parent()
		.siblings()
		.find("td:not(:first-child)")
		.css("display", "none");
	  return;
	}
	$(this).siblings().hide(200);
  });

  ConfigureEdit(itemId);
  ConfigureDelete(itemsTable, itemId);
  ConfigureSubmit(itemForm.id, itemsTable);

  $(itemForm).data("validator").settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input, textarea", function () {
	  const newFormData = getFormData($(itemForm));
	
	  let formDataEquals = true;
	  for (let [key, value] of newFormData.entries())
		  if (origForm.get(key) !== value) {
			  formDataEquals = false;
			  break;
		  }

	  if (!formDataEquals && $(itemForm).valid()) $("#addBtn").removeAttr("disabled");
	  else $("#addBtn").attr("disabled", "disabled");
  });

  var subCategorytypeaheadResources = new window.Bloodhound({
		datumTokenizer: window.Bloodhound.tokenizers.obj.whitespace("searchQuery"),
		queryTokenizer: window.Bloodhound.tokenizers.whitespace,
		remote: {
			url: "/api/subCategories/false?pageNumber=1&pageSize=20&sortBy=name&includeCategory=false&searchByCategory=false&searchQuery=%searchQuery",
			wildcard: "%searchQuery"
		}
	});

  $("#SubCategoryName").typeahead({
		minLength: 2,
		highlight: true
	}, {
		name: "subCategory",
		display: "name",
		limit: 20,
		source: subCategorytypeaheadResources
	}).on("typeahead:select", function (e, subCategory) {
		selectedSubCategory = subCategory;
		$("#SubCategoryId").val(subCategory.id);
	});

	$("#SubCategoryName").on("blur keyup paste keydown", function() {
		if (selectedSubCategory !== null && $(this).val().trim() !== selectedSubCategory.name)
			$("#SubCategoryId").val("");
	});

	$("#SubCategoryName").on("keypress", function(e) {
		if ($(this).val().length < 2 && e.keyCode === 32)
			e.preventDefault();
	});

	var brandtypeaheadResources = new window.Bloodhound({
		datumTokenizer: window.Bloodhound.tokenizers.obj.whitespace("searchQuery"),
		queryTokenizer: window.Bloodhound.tokenizers.whitespace,
		remote: {
			url: "/api/brands/false?pageNumber=1&pageSize=20&sortBy=name&searchQuery=%searchQuery",
			wildcard: "%searchQuery"
		}
	});

	$("#BrandName").typeahead({
		minLength: 2,
		highlight: true
	}, {
		name: "brand",
		display: "name",
		limit: 20,
		source: brandtypeaheadResources
	}).on("typeahead:select", function (e, brand) {
		selectedBrand = brand;
		$("#BrandId").val(brand.id);
	});

	$("#BrandName").on("blur keyup paste keydown", function() {
		if (selectedBrand !== null && $(this).val().trim() !== selectedBrand.name)
			$("#BrandId").val("");
	});

	$("#BrandName").on("keypress", function(e) {
		if ($(this).val().length < 2 && e.keyCode === 32)
			e.preventDefault();
	});
});

$(function () {
	jQuery.validator.addMethod("requiredif", function (value) {
		var isValid = true;
		if (parseInt($("#Id").val()) === 0 && (value === null || value === undefined || value === ""))
			isValid = false;
		return isValid;
	});

	jQuery.validator.unobtrusive.adapters.add("requiredif", [], function (options) {
		options.rules["requiredif"] = {};
		options.messages["requiredif"] = "The image required.";
	});
}(jQuery));