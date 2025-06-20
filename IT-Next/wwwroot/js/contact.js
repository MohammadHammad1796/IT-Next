const apiUrl = apiUrls.contactMessages;

function getFormDataAsJson(form) {
    const unIndexedArray = form.serializeArray();
    const indexedArray = {};

    $.map(unIndexedArray, function (n) {
        if (typeof n["value"] === "string")
            n["value"] = n["value"].trim();
        indexedArray[n["name"]] = n["value"];
    });

    const result = JSON.stringify(indexedArray);
    return result;
}

$("#contactForm").on("submit", function(e) {
    e.preventDefault();
    if (!$(this).valid())
        return;

    $("#contactModal div.modal-body.wait").show();
    $("#contactModal div.modal-body.result").hide();
    $("#contactModal").modal("toggle");

    const resource = getFormDataAsJson($(this));

    $.ajax({
        async: false,
        url: apiUrl,
        method: "POST",
        contentType: "application/json",
        dataType: "json",
        data: resource,
        statusCode: {
            200: function () {
                $("#contactForm")[0].reset();
                $("#contactForm").validate();
                $("#contactModal div.modal-body.result").html("<p>Your message were sent successfully.</p>");
                $("#contactModal div.modal-body.wait").hide();
                $("#contactModal div.modal-body.result").show();
            },
            500: function () {
                $("#contactModal div.modal-body.result").html("<p>Your message were not sent successfully, " +
                    "due server error. Please try again.</p>");
                $("#contactModal div.modal-body.wait").hide();
                $("#contactModal div.modal-body.result").show();
            }
        }
    });
});