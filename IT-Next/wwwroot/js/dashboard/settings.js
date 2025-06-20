const apiUrl = apiUrls.settings;
const itemForm = $("#settings")[0];
var $ = window.$;

var resource = null;

function GetData() {
	let result;
	$.ajax({
		url: apiUrl,
		type: "GET",
		async: false,
		statusCode: {
			200: function (response) {
				result = response;
			}
		}
	});
	return result;
}

function LoadData() {
	const { connectionString, httpsPort, hostingSpaceInMb } = GetData();
	$("#ConnectionString").val(connectionString);
	$("#HttpsPort").val(httpsPort);
	$("#HostingSpaceInMb").val(hostingSpaceInMb);
}

function ConfigureSubmit(id) {
	$(`#${id}`).submit(function (e) {
		e.preventDefault();

		const settingsResource = getFormDataAsJson($(this));
		SaveItem(settingsResource);
	});
}

function SaveItem(resource) {
	$.ajax({
		async: false,
		url: apiUrl,
		method: "POST",
		contentType: "application/json",
		dataType: "json",
		data: resource,
		statusCode: {
			200: function () {
				const message = "Settings have been saved successfully. Please restart the application to reflect new settings";
				window.DisplayToastNotification(message);
				origForm = $(itemForm).serialize();
				$("#saveBtn").attr("disabled", "disabled");
			},
			400: function (response) {
				var data = JSON.parse(response.responseText);
				$(`#${itemForm.id}`).data("validator").showErrors(data);
				origForm = $(itemForm).serialize();
				$("#saveBtn").attr("disabled", "disabled");
			},
		}
	});
}

$(window).on("load", function () {
	LoadData();

	$(this).find("input:first").focus();
	$("#saveBtn").attr("disabled", "disabled");
	var origForm = $(itemForm).serialize();

	ConfigureSubmit(itemForm.id);

	$(itemForm).data('validator').settings.ignore = null;

	$(itemForm).on("change input blur keyup paste", "input", function () {
		if ($(itemForm).serialize() !== origForm && $(itemForm).valid())
			$("#saveBtn").removeAttr("disabled");
		else $("#saveBtn").attr("disabled", "disabled");
	});
});
