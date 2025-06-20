const hostingApiUrl = apiUrls.hosting;
var $ = window.$;

function GetData(url) {
  let result;
  $.ajax({
	url: url,
	type: "GET",
	async: false,
	statusCode: {
	  200: function (response) {
		result = response;
	  },
      404: function () {
        result = null;
      }
	}
  });
  return result;
}

function RenderCertificateInfo()
{
    var resource = GetData(`${hostingApiUrl}certificateInfo`);
    if (resource === null)
        $("#hostingInfo").append(
            `<div class="col-md-6 pb-3" style="border: 1px solid black; border-radius: 10px; background-color: var(--bs-teal);">
                <p>Certificate info not available now.</p>
		    </div>`
        );
    else
        $("#hostingInfo").append(
            `<div class="col-md-5 pb-3 mb-3" style="border: 1px solid black; border-radius: 10px; background-color: var(--bs-teal);">
                <p>Certificate info</p>
                <div class="pb-2">Issue date: ${FormatDateTime(GetDateTimeObjectAsClientZone(resource.issueTime, false), "dd-MM-yyyy")}</div>
                <div>Expiration date: ${FormatDateTime(GetDateTimeObjectAsClientZone(resource.expirationTime, false), "dd-MM-yyyy")}</div>
            </div>`
        );
}

function RenderStorageInfo()
{
    var resource = GetData(`${hostingApiUrl}storageInfo`);
    $("#hostingInfo").append(
        `<div class="col-md-5 offset-md-2 pb-3" style="border: 1px solid black; border-radius: 10px; background-color: var(--bs-cyan);">
            <p>Storage info</p>
            <div class="pb-2">All: ${resource.all.toFixed(2)} MB</div>
            <div class="pb-2">Used: ${resource.used.toFixed(2)} MB</div>
            <div>Free: ${(resource.all - resource.used).toFixed(2)} MB</div>
        </div>`
    );
}

$(window).on("load", function () {
    RenderCertificateInfo();
    RenderStorageInfo();
});