const apiUrl = apiUrls.products;
const itemForm = $("#manageItems")[0];
let dataTable = null;
let ignoreTimeZone = false;
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

	const productResource = getFormData($(this));
	const submitType = $('#manageModal button[type="submit"]').text();
	SaveItem(productResource, submitType, table);
  });
}

function ConfigureEdit(id, table) {
  $(table).on("click", "a.edit-btn", function (e) {
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

function DeleteItem(id, table) {
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
			LoadData(query, table);
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
	DeleteItem(id, table);
  });
}

function UpdateRow(id, product) {
	const ignoreTimeZone = true;
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
  row.find("td").eq(7).text(FormatDateTime(GetDateTimeObjectAsClientZone(product.lastUpdate, ignoreTimeZone), "dd-MM-yyyy hh:mm a"));
  row.find("td:eq(8)").find("img:eq(0)").attr("src", product.imagePath);
  row.find("td:eq(9)").text(product.description);
  row.find("td:eq(10) > a > i.fa-eye").parent().attr("href", `/products/${product.categoryName}/${product.subCategoryName}/${product.name}`);
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
					ignoreTimeZone = true;
					dataTable.AddRow(product);
					ignoreTimeZone = false
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
dataTable = new DataTable({ columns: [
		{
			selector: "name",
			label: "Name"
		},
		{
			selector: "categoryName",
			label: "Category"
		},
		{
			selector: "subCategoryName",
			label: "SubCategory",
			getCustomAttributes: (product) => `data-id="${product.subCategoryId}"`
		},
		{
			selector: "brandName",
			label: "Brand",
			getCustomAttributes: (product) => `data-id="${product.brandId}"`
		},
		{
			selector: "price",
			label: "Price"
		},
		{
			selector: "discount",
			label: "Discount"
		},
		{
			selector: "quantity",
			label: "Quantity"
		},
		{
			selector: "lastUpdate",
			label: "Last update",
			isSearchable: false,
			getContent: (product) => FormatDateTime(GetDateTimeObjectAsClientZone(product.lastUpdate, (() => ignoreTimeZone)()), "dd-MM-yyyy hh:mm a")
		},
		{
			selector: "imagePath",
			label: "Image",
			isSortable: false,
			classes: ["imageColumn"],
			getContent: (product) => `<img src="${baseAppUrl}${product.imagePath}" style="width:100%;"/>`
		},
		{
			selector: "description",
			label: "Description",
			isSortable: false
		}
	], getAdditionalActions: (product) => [
		`<a href="${baseAppUrl}products/${product.categoryName}/${product.subCategoryName}/${product.name}" target="_blank" data-id="1"><i class="fas fa-eye me-3 mb-3"></i></a>`
	] });
	dataTable.initialize(query);
	
	const itemsTable = $("#items");

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

  ConfigureEdit(itemId, itemsTable);
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
			url: apiUrls.subCategories + "false?pageNumber=1&pageSize=20&sortBy=name&includeCategory=false&searchByCategory=false&searchQuery=%searchQuery",
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
			url: apiUrls.brands + "false?pageNumber=1&pageSize=20&sortBy=name&searchQuery=%searchQuery",
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