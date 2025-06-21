const apiUrl = apiUrls.subCategories;
const itemForm = $("#manageItems")[0];
let dataTable = null;
var selectedCategory = null;
var $ = window.$;

var query = {
  pageNumber: 1,
  pageSize: 10,
  isSortAscending: true,
  sortBy: "name",
  searchQuery: "",
  includeCategory: true,
  searchByCategory: true
};
var entries = {
  from: 0,
  to: 0,
  all: 0
};
var itemId = null;

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#Id").val(0);
  $("#CategoryId").val("");
  $("#CategoryName").typeahead("val", "");
  selectedCategory = null;
  itemId = null;

  const validator = $(itemForm).validate();

  const errors = $(itemForm).find(".field-validation-error span");
  errors.each(function() {
	   validator.settings.success($(this));
	   $(this).remove();
  });

  $(".field-validation-valid span").remove();
  validator.resetForm();
}

function ConfigureSubmit(id, table) {
  $(`#${id}`).submit(function (e) {
	e.preventDefault();

	const subCategoryResource = getFormDataAsJson($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(subCategoryResource, submitType, table);
  });
}

function ConfigureEdit(id, table) {
  $(table).on("click", "a.edit-btn", function (e) {
	e.preventDefault();
	ClearInputs();
	id = $(this).attr("data-id");
	const tableRow = $(`#item_${id}`);
	$("#Id").val(id);
	const  subCategoryName = tableRow.children(":eq(0)").text();
	$("#Name").val(subCategoryName);
	const categoryId = tableRow.children(":eq(1)").attr("data-id");
	selectedCategory = {
		id: categoryId,
		name: tableRow.children(":eq(1)").text()
	};
	/*selectedCategory.id = categoryId;
	selectedCategory.name = tableRow.children(":eq(1)").text();*/
	/*$.ajax({
		url: `/api/categories/${categoryId}`,
		type: "GET",
		async: false,
		statusCode: {
			200: function (response) {
				selectedCategory = response;
			}
		}
	});*/

	$("#CategoryId").val(selectedCategory.id);
	$("#CategoryName").val(selectedCategory.name);
	$('#manageModal button[type="submit"]').text("Save");
	$("#manageModalLabel").text(`Edit sub category: ${subCategoryName}`);
	$("#manageModal").modal("toggle");
  });
}

function DeleteItem(id, table) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Sub category not found.");
	  },
	  204: function () {
		$(`#item_${id}`).remove();
		entries.from = entries.to === 1 ? --entries.from : entries.from;
		entries.to--;
		entries.all--;
		if (entries.from > entries.to) {
			query.pageNumber--;
			LoadData(query, table);
		}
		else
			RenderEntriesInfo(entries);
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Sub category have been deleted successfully.");
	  }
	}
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");const tableRow = $(`#item_${id}`);
	const subCategoryName = tableRow.children(":eq(0)").text();
	const categoryName = tableRow.children(":eq(1)").text();
	$("#deleteParagraph").html(`Are you sure you want to delete this sub category?
<div>Name: ${subCategoryName}</div><div>Category: ${categoryName}</div>`);
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id, table);
  });
}

function UpdateRow(id, subCategory) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(subCategory.name);
  row.find("td").eq(1).text(subCategory.categoryName);
  row.find("td").eq(1).attr("data-id", subCategory.categoryId);
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
	  200: function (subCategory) {
		let message = "";
		switch (saveType) {
		  case "Save":
			UpdateRow(`#item_${subCategory.id}`, subCategory);
			message = "Changes have been saved successfully.";
			break;
		  case "Add":
			dataTable.AddRow(subCategory);
			entries.from = entries.from === 0 ? ++entries.from : entries.from;
			entries.to++;
			entries.all++;
			RenderEntriesInfo(entries);
			message = "Sub category have been added successfully.";
			break;
		}

		$("#manageModal").modal("toggle");
		window.DisplayToastNotification(message);
	  },
	  400: function (response) {
		  const data = JSON.parse(response.responseText);
		  $("#manageItems").data("validator").showErrors(data);
	  },
	  404: function () {
		$("#manageModal").modal("toggle");
		window.DisplayToastNotification("Sub category not found.");
	  }
	}
  });
}

$(window).on("load", function () {
dataTable = new DataTable({ columns: [
		{selector: "name",
		label: "Name"},
		{selector: "categoryName",
		label: "Category",
		getCustomAttributes: (subCategory) => `data-id="${subCategory.categoryId}"`}
	]});
	dataTable.initialize(query);

	const itemsTable = $("#items");

  let origForm = $(itemForm).serialize();

  $("#new").on("click", function () {
	$('#manageModal button[type="submit"]').text("Add");
	$("#manageModalLabel").text("New sub category");
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

  ConfigureEdit(itemId, itemsTable);
  ConfigureDelete(itemsTable, itemId);
  ConfigureSubmit(itemForm.id, itemsTable);

  $(itemForm).data('validator').settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input", function () {
	if ($(itemForm).serialize() !== origForm && $(itemForm).valid())
	  $("#addBtn").removeAttr("disabled");
	else $("#addBtn").attr("disabled", "disabled");
  });

  var typeaheadResources = new window.Bloodhound({
		datumTokenizer: window.Bloodhound.tokenizers.obj.whitespace("searchQuery"),
		queryTokenizer: window.Bloodhound.tokenizers.whitespace,
		remote: {
			url: apiUrls.categories + "false?pageNumber=1&pageSize=20&sortBy=name&searchQuery=%searchQuery",
			wildcard: "%searchQuery"
		}
	});

  $("#CategoryName").typeahead({
		minLength: 2,
		highlight: true
	}, {
		name: "category",
		display: "name",
		limit: 20,
		source: typeaheadResources
	}).on("typeahead:select", function (e, category) {
		selectedCategory = category;
		$("#CategoryId").val(category.id);
	});

	$("#CategoryName").on("blur keyup paste keydown", function() {
		if (selectedCategory !== null && $(this).val().trim() !== selectedCategory.name)
			$("#CategoryId").val("");
	});

	$("#CategoryName").on("keypress", function(e) {
		if ($(this).val().length < 2 && e.keyCode === 32)
			e.preventDefault();
	});
});