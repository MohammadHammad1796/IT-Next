// used in LoadData function only
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

//private method
function matchCase(searchText, matchedPart) {
  var result = "";

  for (let i = 0; i < searchText.length; i++) {
    const searchCharacter = searchText.charAt(i);
    const matchedCharacter = matchedPart.charCodeAt(i);

    if (matchedCharacter >= 65 && matchedCharacter < 65 + 26) {
      result += searchCharacter.toUpperCase();
    } else if (matchedCharacter >= 97 && matchedCharacter < 97 + 26) {
      result += searchCharacter.toLowerCase();
    } else {
      result += searchCharacter;
    }
  }

  return result;
}

// used in LoadData function only
function HighlightSearchResult(table, queryObject) {
  if (queryObject.searchQuery.length < 1 || queryObject.searchQuery.length > 50)
    return;

  const columns = $(table)
    .find("td:not(.not-searchable)")
    .filter(function () {
      return (
        $(this)
          .text()
          .toLowerCase()
          .indexOf(queryObject.searchQuery.toLocaleLowerCase()) > -1
      );
    });

  for (let column of columns) {
    let regex = new RegExp(`(${queryObject.searchQuery})`, "gi");

    const columnChildren = $(column).find("*");
    if (columnChildren.length) {
      for (let child of columnChildren) {
        const elementText = $(child).text();
        if (regex.test(elementText)) {
          const elementNewHtml = elementText.replace(
            regex,
            function (matchedPart) {
              return `<span class="highlighted">${matchCase(
                queryObject.searchQuery,
                matchedPart
              )}</span>`;
            }
          );

          $(child).html(elementNewHtml);
        }
      }
    } else {
      const columnText = $(column).text();
      if (regex.test(columnText)) {
        const columnNewHtml = columnText.replace(regex, function (matchedPart) {
          return `<span class="highlighted">${matchCase(
            queryObject.searchQuery,
            matchedPart
          )}</span>`;
        });

        $(column).html(columnNewHtml);
      }
    }
  }
}

// used in LoadData function only
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
      },
    },
  });
  return result;
}

function CalculateEntries(
  count,
  queryObject,
  currentItemsCount,
  entriesObject
) {
  entriesObject.from = queryObject.pageSize * (queryObject.pageNumber - 1);
  if (currentItemsCount) entriesObject.from++;
  entriesObject.to =
    currentItemsCount < queryObject.pageSize
      ? queryObject.pageSize * (queryObject.pageNumber - 1) + currentItemsCount
      : queryObject.pageSize * queryObject.pageNumber;
  entriesObject.all = count;
}

// used in LoadData, SaveItem, DeleteItem functions only
function RenderEntriesInfo(entriesObject) {
  $("#countInfo").text(
    `Showing ${entriesObject.from} to ${entriesObject.to} of ${entriesObject.all} entries`
  );
}

// used there and outside when DeleteItem
function LoadData(queryObject, table) {
  let resource = GetData(queryObject);

  if (!resource.items.length && parseInt(queryObject.pageNumber) !== 1) {
    queryObject.pageNumber = 1;
    resource = GetData(queryObject);
  }
  // ready
  dataTable.RenderItemsTable(resource.items, table);
  // ready
  HighlightSearchResult(table, queryObject);
  // ready
  RenderPaginationButtons(
    queryObject.pageNumber,
    queryObject.pageSize,
    resource.total
  );
  // ready
  CalculateEntries(resource.total, query, resource.items.length, entries);
  RenderEntriesInfo(entries);
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

function EnableSorting(queryObject, table) {
  $(".sort").on("click", function () {
    $(this).siblings().find("i").removeClass("fa-sort-up fa-sort-down");
    const child = $(this).children("i:eq(0)");
    queryObject.sortBy = $(this).attr("sortby");
    $(`#sortBy option[value='${$(this).attr("sortby")}']`).prop(
      "selected",
      true
    );
    if (!child.hasClass("fa-sort-up") && !child.hasClass("fa-sort-down")) {
      child.addClass("fa-sort-down");
      $("[name='sort'][value='desc']").prop("checked", true);
      queryObject.isSortAscending = false;
      LoadData(queryObject, table);
      return;
    }

    if (child.hasClass("fa-sort-up")) {
      queryObject.isSortAscending = false;
      $("[name='sort'][value='desc']").prop("checked", true);
    } else {
      queryObject.isSortAscending = true;
      $("[name='sort'][value='asc']").prop("checked", true);
    }

    child.toggleClass("fa-sort-up fa-sort-down");
    LoadData(queryObject, table);
  });
}

function EnableMobileSorting(queryObject, table) {
  $("[name='sort']").on("click", function () {
    const value = $(this).val();
    if (
      (value === "asc" && queryObject.isSortAscending === true) ||
      (value === "desc" && queryObject.isSortAscending === false)
    )
      return;

    if (value === "asc") {
      $(".sort")
        .find("i.fa-sort-down")
        .addClass("fa-sort-up")
        .removeClass("fa-sort-down");
      queryObject.isSortAscending = true;
      LoadData(queryObject, table);
      return;
    }
    $(".sort")
      .find("i.fa-sort-up")
      .addClass("fa-sort-down")
      .removeClass("fa-sort-up");
    queryObject.isSortAscending = false;
    LoadData(queryObject, table);
  });

  $("#sortBy").on("click", function () {
    const value = $(this).val();
    if (value === queryObject.sortBy) return;

    $(".sort i").removeClass("fa-sort-up fa-sort-down");
    if (queryObject.isSortAscending)
      $(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-up");
    else $(`.sort[sortby='${value}'] i`).eq(0).addClass("fa-sort-down");
    queryObject.sortBy = value;
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

function EnableBlockView(table) {
  $(table).on("click", "tr td:first-child", function () {
    if ($(this).css("display") !== "block") return;

    if ($(this).siblings().eq(0).css("display") !== "block") {
      $(this)
        .siblings()
        .show(200, function () {
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
}

function DataTable({
  columns = [],
  itemKey = "id",
  allowEdit = true,
  allowDelete = true,
  getAdditionalActions = undefined,
} = props) {
  const availablePagesSizes = [10, 25, 50, 100];

  const processedColumns = columns.map((column) => ({
    ...column,
    isSortable: "isSortable" in column ? column.isSortable : true,
    isSearchable: "isSearchable" in column ? column.isSearchable : true,
  }));

  this.AddRow = function (item) {
    let row = `<tr id='item_${item[itemKey]}'>
            ${columns
              .map(
                (column) =>
                  `<td head='${column.label}' class="${
                    column.classes ? column.classes.join(" ") : ""
                  } ${column.isSearchable ? "" : "not-searchable"}" ${
                    column.getCustomAttributes
                      ? column.getCustomAttributes(item)
                      : ""
                  }>${
                    column.getContent
                      ? column.getContent(item)
                      : item[column.selector]
                  }</td>`
              )
              .join("")}
            ${
              allowEdit || allowDelete || getAdditionalActions
                ? `
            <td head='Actions'>
						${
              allowEdit
                ? `<a href="#" class="edit-btn" data-id="${item[itemKey]}"><i class="fas fa-edit me-3"></i></a>`
                : ""
            }
						${
              allowDelete
                ? `<a href="#" class="remove-btn" data-id="${item[itemKey]}"><i class="fas fa-trash"></i></a>`
                : ""
            }
            ${getAdditionalActions ? getAdditionalActions(item).join("") : ""}
						</td>
            `
                : ""
            }
						</tr>`;
    $("#items").find("tbody:eq(0)").append(row);
  };

  // used in LoadData function only
  // require AddRow function only
  this.RenderItemsTable = function (items, table) {
    const tbody = $(table).find("tbody:eq(0)");
    tbody.empty();

    for (let item of items) {
      this.AddRow(item);
    }
  };

  this.initialize = function (queryObject) {
    const html = `
    <div class="row">
      <div class="col-md-12">
        <div class="row">
          <div class="col-md-5 col-sm-12 center mb-3">
            <label for="pageSize">Show</label>
            <select id="pageSize" class="form-select">
              ${availablePagesSizes
                .map(
                  (pageSize, index) => `
                <option value="${pageSize}" ${
                    (index === 0 && `selected="selected"`) || ""
                  }>${pageSize}</option>
              `
                )
                .join("")}
            </select>
            <label for="pageSize">entries</label>
          </div>
          <div class="col-md-7 col-sm-12 center mb-3" style="text-align: right;">
            <label for="searchText">Search</label>
            <input type="text" id="searchText" autocomplete="off" class="form-control" placeholder="Type 2 ~ 50 character."/>
          </div>
        </div>

        <div class="row sortSection">
          <div class="col-sm-6 col-12 mb-3">
            <label for="sortBy">Sort By</label>
            <select id="sortBy" class="form-select" style="width: auto; display: inline-block;">
              ${processedColumns
                .filter((column) => column.isSortable)
                .map(
                  (column, index) => `
                <option value="${column.selector}" ${
                    (index === 0 && `selected="selected"`) || ""
                  }>${column.label}</option>
              `
                )
                .join("")}
            </select>
          </div>
          <div class="col-sm-6 col-12 mb-3">
            <div class="form-check">
              <input class="form-check-input" type="radio" id="sortAsc" name="sort" value="asc" checked="checked">
              <label class="form-check-label" for="sortAsc">
                Ascending
              </label>
            </div>
            <div class="form-check">
              <input class="form-check-input" type="radio" id="sortDesc" name="sort" value="desc">
              <label class="form-check-label" for="sortDesc">
                Descending
              </label>
            </div>
          </div>
        </div>
      </div>

      <table id="items" class="table border-bottom border-top">
        <thead>
          <tr>
            ${processedColumns
              .map(
                (column, index) => `
              <th ${
                column.isSortable
                  ? `class="sort" sortby="${column.selector}"`
                  : ""
              }>
                ${column.label}
                ${
                  column.isSortable
                    ? `<i class="fas ${
                        (index === 0 && "fa-sort-up") || ""
                      } float-end"></i>`
                    : ""
                }
              </th>
            `
              )
              .join("")}
            <th>Actions</th>
          </tr>
        </thead>
        <tbody></tbody>
      </table>

      <div class="row">
        <div class="col-md-6 col-sm-12 center mb-3" id="countInfo">Showing 1 to 10 of 10 entries</div>
        <div id="paginationSection" class="col-md-6 col-sm-12 center mb-3" style="text-align: right;">
          <a value="0">Previous</a>
          <a value="0">Next</a>
        </div>
      </div>
    </div>`;

    $("section.dataTableSection").html(html);

    const itemsTable = $("#items");
    LoadData(queryObject, itemsTable);

    EnableSearch(queryObject, itemsTable);
    EnableSorting(queryObject, itemsTable);
    EnableMobileSorting(queryObject, itemsTable);
    EnablePagination(queryObject, itemsTable);

    EnableBlockView(itemsTable);
  };
}
