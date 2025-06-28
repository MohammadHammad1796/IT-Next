const apiUrl = apiUrls.brands;
let dataTable = null;
var $ = window.$;

var query = {
  pageNumber: 1,
  pageSize: 10,
  isSortAscending: true,
  searchQuery: "",
  sortBy: "name",
};
var entries = {
  from: 0,
  to: 0,
  all: 0,
};

function DeleteItem(id, table) {
  return new Promise((resolve, reject) => {
    $.ajax({
      async: false,
      url: `${apiUrl}${id}`,
      method: "DELETE",
      statusCode: {
        404: function () {
          reject("Brand not found.");
        },
        204: function () {
          $(`#item_${id}`).remove();
          entries.from = entries.to === 1 ? --entries.from : entries.from;
          entries.to--;
          entries.all--;
          if (entries.from > entries.to) {
            query.pageNumber--;
            LoadData(query, table);
          } else RenderEntriesInfo(entries);
          resolve("Brand have been deleted successfully.");
        },
      },
    });
  });
}

function generateDeleteParagraph(brand) {
  return `Are you sure you want to delete this brand?<div>Name: ${brand.name}</div>`;
}

function SaveItem(resource, saveType) {
  return new Promise((resolve, reject) => {
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
              dataTable.updateRow(brand);
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

          resolve(message);
        },
        400: function (response) {
          const data = JSON.parse(response.responseText);
          reject(data);
        },
        404: function () {
          reject("Brand not found.");
        },
      },
    });
  });
}

$(window).on("load", function () {
  dataTable = new DataTable({
    columns: [{ selector: "name", label: "Name" }],
    pageName: "Brand",
    onDelete: DeleteItem,
    getDeleteParagraph: generateDeleteParagraph,
    onCloseDeleteModal: () => {},
    fields: [
      {
        type: "text",
        selector: "name",
        label: "Name",
        maxLength: 50,
        minLength: 2,
        isRequired: true,
      },
      {
        type: "hidden",
        selector: "id",
        isRequired: true,
        defaultValue: 0,
      },
    ],
    onSave: SaveItem,
  });
  dataTable.initialize(query);
});
