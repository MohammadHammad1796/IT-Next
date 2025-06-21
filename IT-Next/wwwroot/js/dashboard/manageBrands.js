const apiUrl = apiUrls.brands;
const itemForm = $("#manageItems")[0];
let dataTable = null;
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

function ClearInputs() {
  $(itemForm)[0].reset();
  $("#Id").val(0);
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

function ConfigureSubmit(id) {
  $(`#${id}`).submit(function (e) {
	e.preventDefault();

	const brandResource = getFormDataAsJson($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(brandResource, submitType);
  });
}

function ConfigureEdit(id, table) {
  $(table).on("click", "a.edit-btn", function (e) {
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

function DeleteItem(id, table) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		dataTable.closeDeleteModal();
		window.DisplayToastNotification("Brand not found.");
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
		dataTable.closeDeleteModal();
		window.DisplayToastNotification("Brand have been deleted successfully.");
	  }
	}
  });
}

function handleOpenDeleteModal(brand) {
	dataTable.showDeleteModal(`Are you sure you want to delete this brand?<div>Name: ${brand.name}</div>`);
}

function UpdateRow(id, brand) {
  const row = $(`${id}`);
  row.find("td").eq(0).text(brand.name);
}

function SaveItem(resource, saveType) {
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
			dataTable.AddRow(brand);
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
	dataTable = new DataTable({ columns: [
		{selector: "name",
		label: "Name"}
	], pageName: "Brand",
	onDelete: DeleteItem,
	onOpenDeleteModal: handleOpenDeleteModal,
	onCloseDeleteModal: () => {
		ClearInputs();
	}});
	dataTable.initialize(query);

	const itemsTable = $("#items");

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

  $("#manageModal").on("hidden.bs.modal", function () {
	ClearInputs();
  });

  ConfigureEdit(itemId, itemsTable);
  ConfigureSubmit(itemForm.id);

  $(itemForm).data('validator').settings.ignore = null;

  $(itemForm).on("change input blur keyup paste", "input", function () {
	if ($(itemForm).serialize() !== origForm && $(itemForm).valid())
	  $("#addBtn").removeAttr("disabled");
	else $("#addBtn").attr("disabled", "disabled");
  });
});
