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

$(function () {
	jQuery.validator.addMethod("requiredifintegeridzero", function (value) {
		var isValid = true;
		if (parseInt($("#Id").val()) === 0 && (value === null || value === undefined || value === ""))
			isValid = false;
		return isValid;
	});

	jQuery.validator.unobtrusive.adapters.add("requiredifintegeridzero", [], function (options) {
		options.rules["requiredifintegeridzero"] = {};
		options.messages["requiredifintegeridzero"] = options.message;
	});
}(jQuery));