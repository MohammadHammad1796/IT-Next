const apiUrl = "/api/contact/";
const itemsTable = $("#items");
const itemForm = $("#manageItems")[0];
var counter = 1;
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
  for (let message of items) {
	if (!isOdd) isOdd = true;
	else isOdd = false;
	AddRow(table, message, isOdd);
  }
}

function AddRow(table, message, isOdd = null) {
    const ignoreTimeZone = false;
  if (isOdd === null)
	isOdd = $(table).find("tr:last").hasClass("odd") ? false : true;
  let row = `<tr id='item_${message.id}'`;
  if (!isOdd) row += 'class="odd"';
  row += `><td head='Customer name'>${message.customerName}</td>
						<td head='Email'><a style="text-decoration: auto;" href="mailto:${message.email}">${message.email}</a></td>
						<td head='Mobile number' ><a style="text-decoration: auto;" href="tel:${message.mobileNumber}">${message.mobileNumber}</a></td>
						<td head='Time' >${FormatDateTime(GetDateTimeObjectAsClientZone(message.time, ignoreTimeZone), "dd-MM-yyyy hh:mm a")}</td>
						<td head='Status' >${message.isRead ? "Readed" : "New"}</td>
						<td head='Message' >${message.message}</td>
						<td head='Actions'>
						<a href="#" class="toggleStatus-btn" data-id="${message.id}"><i class="fas ${message.isRead ? "fa-eye-slash" : "fa-eye"} me-3"></i></a>
						<a href="#" class="remove-btn" data-id="${message.id}"><i class="fas fa-trash"></i></a>
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
    itemId = null;
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

function DeleteItem(id) {
  $.ajax({
	async: false,
	url: `${apiUrl}${id}`,
	method: "DELETE",
	statusCode: {
	  404: function () {
		$("#deleteModal").modal("toggle");
		window.DisplayToastNotification("Message not found.");
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
		window.DisplayToastNotification("Message have been deleted successfully.");
	  }
	}
  });
}

function ConfigureDelete(table, id) {
  $(table).on("click", "a.remove-btn", function (e) {
	e.preventDefault();
	id = $(this).attr("data-id");const tableRow = $(`#item_${id}`);
	const customerName = tableRow.children(":eq(0)").text();
	const email = tableRow.children(":eq(1)").find("a:eq(0)").text();
	const mobileNumber = tableRow.children(":eq(2)").find("a:eq(0)").text();
    const time = tableRow.children(":eq(3)").text();
    const status = tableRow.children(":eq(4)").text();
    const message = tableRow.children(":eq(5)").text();
    $("#deleteParagraph").html(`Are you sure you want to delete this message?
<div>Customer name: ${customerName}</div><div>Email: ${email}</div>
<div>Mobile number: ${mobileNumber}</div><div>Time: ${time}</div>
<div>Status: ${status}</div><div>Message: ${message}</div>`);
	$("#deleteModal").modal("toggle");
  });

  $("#deleteItem").submit(function (e) {
	e.preventDefault();
	DeleteItem(id);
  });
}

function ConfigureToggleStatus(table, id) {
    $(table).on("click", "a.toggleStatus-btn", function (e) {
        e.preventDefault();
        id = $(this).attr("data-id");
        ToggleStatus(id);
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

function ToggleStatusInRow(id) {
  const row = $(`${id}`);
  const previousStatus = row.find("td").eq(4).text();
  const isNew = previousStatus.toLowerCase() !== "new";
  const newStatus = isNew ? "New" : "Readed";
  row.find("td").eq(4).text(newStatus);
  row.find("td").eq(6).find("i:eq(0)").toggleClass("fa-eye").toggleClass("fa-eye-slash");
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

$(window).on("load", function () {
  LoadData(query, itemsTable);

  EnableSearch(query, itemsTable);
  EnableSorting(query, itemsTable);
  EnableMobileSorting(query, itemsTable);
  EnablePagination(query, itemsTable);

  $("#deleteModal").on("hidden.bs.modal", function () {
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

  ConfigureToggleStatus(itemsTable, itemId);
  ConfigureDelete(itemsTable, itemId);
});