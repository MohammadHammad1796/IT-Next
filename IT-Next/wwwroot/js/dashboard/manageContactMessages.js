const apiUrl = apiUrls.contactMessages;
const itemForm = $("#manageItems")[0];
let dataTable = null;
var selectedCategory = null;
var $ = window.$;

var query = {
  pageNumber: 1,
  pageSize: 10,
  isSortAscending: true,
  sortBy: "customerName",
  searchQuery: ""
};
var entries = {
  from: 0,
  to: 0,
  all: 0
};
var itemId = null;

function ClearInputs() {
    itemId = null;
}

function DeleteItem(id, table) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		dataTable.closeDeleteModal();
		window.DisplayToastNotification("Message not found.");
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
		window.DisplayToastNotification("Message have been deleted successfully.");
	  }
	}
  });
}

function handleOpenDeleteModal(message) {
	dataTable.showDeleteModal(`Are you sure you want to delete this message?
<div>Customer name: ${message.customerName}</div><div>Email: ${message.email}</div>
<div>Mobile number: ${message.mobileNumber}</div><div>Time: ${formatMessageTime(message)}</div>
<div>Status: ${formatMessageStatus(message)}</div><div>Message: ${message.message}</div>`);
}

function ConfigureToggleStatus(table, id) {
    $(table).on("click", "a.toggleStatus-btn", function (e) {
        e.preventDefault();
        id = $(this).attr("data-id");
        ToggleStatus(id);
    });
}

function ToggleStatusInRow(id) {
  const row = $(`${id}`);
  const previousStatus = row.find("td").eq(4).text();
  const isNew = previousStatus.toLowerCase() !== "new";
  const newStatus = isNew ? "New" : "Read";
  row.find("td").eq(4).text(newStatus);
  row.find("td").eq(6).find("a.toggleStatus-btn > i:eq(0)").toggleClass("fa-eye").toggleClass("fa-eye-slash");
}

function ToggleStatus(id) {
  $.ajax({
	async: false,
	url: `${apiUrl}toggleStatus/${id}`,
	method: "PUT",
    statusCode: {
	  204: function () {
          ToggleStatusInRow(`#item_${id}`);
          window.DisplayToastNotification("Status have been changed.");
	  },
      404: function () {
          window.DisplayToastNotification("Message not found.");
	  }
	}
  });
}

const ignoreTimeZone = false;
const formatMessageTime = (message) => `${FormatDateTime(GetDateTimeObjectAsClientZone(message.time, ignoreTimeZone), "dd-MM-yyyy hh:mm a")}`;

const formatMessageStatus = (message) => `${message.isRead ? "read" : "New"}`;

$(window).on("load", function () {
dataTable = new DataTable({ columns: [
		{
			selector: "customerName",
			label: "Customer name"
		},
		{
			selector: "email",
			label: "Email",
			getContent: (message) => `<a style="text-decoration: auto;" href="mailto:${message.email}">${message.email}</a>`
		},
		{
			selector: "mobileNumber",
			label: "Mobile number",
			getContent: (message) => `<a style="text-decoration: auto;" href="tel:${message.mobileNumber}">${message.mobileNumber}</a>`
		},
		{
			selector: "time",
			label: "Time",
			getContent: formatMessageTime
		},
		{
			selector: "isRead",
			label: "Status",
			getContent: formatMessageStatus
		},
		{
			selector: "message",
			label: "Message"
		}
	], allowEdit: false, allowAdd: false, getAdditionalActions: (message) => [
		`<a href="#" class="toggleStatus-btn" data-id="${message.id}"><i class="fas ${message.isRead ? "fa-eye-slash" : "fa-eye"} me-3"></i></a>`
	], pageName: "Message",
	onDelete: DeleteItem,
	onOpenDeleteModal: handleOpenDeleteModal,
	onCloseDeleteModal: () => {
		ClearInputs();
	} });
	dataTable.initialize(query);

	const itemsTable = $("#items");

  ConfigureToggleStatus(itemsTable, itemId);
});