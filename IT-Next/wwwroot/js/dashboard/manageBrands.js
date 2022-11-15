const apiUrl = "/api/brands/";
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
var counter = 1;
var $ = window.$;

var query = {
  pageNumber: 1,
  pageSize: 10,
  isSortAscending: true,
  searchQuery: "",
  sortBy: "name"
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
  for (let brand of items) {
	if (!isOdd) isOdd = true;
	else isOdd = false;
	AddRow(table, brand, isOdd);
  }
}

function AddRow(table, brand, isOdd = null) {
  if (isOdd === null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? false : true;
  let row = `<tr id='item_${brand.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Name'>${brand.name}</td>
						<td head='Actions'>
						<a href="#" class="edit-btn" data-id="${brand.id}"><i class="fas fa-edit me-3"></i></a>
						<a href="#" class="remove-btn" data-id="${brand.id}"><i class="fas fa-trash"></i></a>
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

	const brandResource = getFormDataAsJson($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(brandResource, submitType, table);
  });
}

function ConfigureEdit(id) {
  $("#items").on("click", "a.edit-btn", function (e) {
	e.preventDefault();
	ClearInputs();
	id = $(this).attr("data-id");
	const tableRow = $(`#item_${id}`);
	$("#Id").val(id);
	const brandName = tableRow.children(":eq(0)").text();
	$("#Name").val(brandName);
	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit brand: ${brandName}`);
	$("#manageModal").modal("toggle");
  });
}

function getFormDataAsJson(form) {
  const unIndexedArray = form.serializeArray();
  const indexedArray = {};

  $.map(unIndexedArray, function (n) {
	if (typeof n["value"] === 'string')
	  n["value"] = n["value"].trim();
	indexedArray[n["name"]] = n["value"];
  });

  const result = JSON.stringify(indexedArray);
  return result;
}

function DeleteItem(id) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Brand not found.");
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
		window.DisplayToastNotification("Brand have been deleted successfully.");
	  }
	}
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");
	const tableRow = $(`#item_${id}`);
	const brandName = tableRow.children(":eq(0)").text();
	$("#deleteParagraph").html(`Are you sure you want to delete this brand?<div>Name: ${brandName}</div>`);
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
	if (searchText.length < 2 || searchText.length > 50)
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
	if (queryObject.searchQuery.length < 2 || queryObject.searchQuery.length > 50)
		return;
	const columns = $(table).find("td").filter(function() {
		return $(this).text().toLowerCase().indexOf(queryObject.searchQuery.toLocaleLowerCase()) > -1;
	});
	for (let column of columns)
	{
		const columnText = $(column).text();
		const regex = new RegExp(`(${queryObject.searchQuery})`, "gi");
		const columnNewHtml = columnText.replace(regex, function(match) {
			return `<span>${matchCase(queryObject.searchQuery, match)}</span>`;
		});
		$(column).html(columnNewHtml);
	}
}

function EnableSorting(queryObject, table) {
  $(".sort").on("click", function () {
	$(this).siblings().find('i').removeClass('fa-sort-up fa-sort-down');
	const child = $(this).children("i:eq(0)");
	if (!child.hasClass("fa-sort-up") && !child.hasClass("fa-sort-down")) {
	  child.addClass("fas");
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

function UpdateRow(id, brand) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(brand.name);
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
}

function SaveItem(resource, saveType, table) {
  $.ajax({
	async: false,
	url: apiUrl,
	method: "POST",
	contentType: "application/json",
	dataType: "json",
	data: resource,
	statusCode: {
	  200: function (brand) {
		let message = "";
		switch (saveType) {
		  case "Save":
			UpdateRow(`#item_${brand.id}`, brand);
			message = "Changes have been saved successfully.";
			break;
		  case "Add":
			AddRow(table, brand);
			entries.from = entries.from === 0 ? ++entries.from : entries.from;
			entries.to++;
			entries.all++;
			RenderEntriesInfo(entries);
			message = "Brand have been added successfully.";
			break;
		}

		$("#manageModal").modal("toggle");
		window.DisplayToastNotification(message);
	  },
	  400: function (response) {
		var data = JSON.parse(response.responseText);
		$("#manageItems").data("validator").showErrors(data);
	  },
	  404: function () {
		$("#manageModal").modal("toggle");
		window.DisplayToastNotification("Brand not found.");
	  }
	}
  });
}

$(window).on("load", function () {
  LoadData(query, itemsTable);

  EnableSearch(query, itemsTable);
  EnableSorting(query, itemsTable);
  EnableMobileSorting(query, itemsTable);
  EnablePagination(query, itemsTable);

  let origForm = $(itemForm).serialize();

  $("#new").on("click", function () {
	$('#manageModal button[type="submit"]').text("Add");
	$("#manageModalLabel").text("New brand");
	ClearInputs();
  });

  $("#manageModal").on("shown.bs.modal", function () {
	$(this).find("input:first").focus();
	$("#addBtn").attr("disabled", "disabled");
	origForm = $(itemForm).serialize();
  });

  $("#manageModal, #deleteModal").on("hidden.bs.modal", function () {
	ClearInputs();
  });

  $(itemsTable).on("click", "tr td:first-child", function () {
	if ($(this).css("display") !== "block") return;

	if ($(this).siblings().eq(0).css("display") !== "block") {
	  $(this).siblings().show(200, function() {
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

  $(itemForm).data('validator').settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input", function () {
	if ($(itemForm).serialize() !== origForm && $(itemForm).valid())
	  $("#addBtn").removeAttr("disabled");
	else $("#addBtn").attr("disabled", "disabled");
  });
});
