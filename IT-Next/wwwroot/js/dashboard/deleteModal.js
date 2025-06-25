function DeleteModal({
  containerSelector,
  itemId,
  pageName = "",
  onCloseDeleteModal,
  onDelete,
  deleteParagraph,
} = props) {
  const modalTemplate = `
    <div class="modal fade" id="deleteModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="deleteModalLabel">Delete ${pageName.toLowerCase()}</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <form id="deleteItem">
            <div class="modal-body">
              <div class="mb-3">
                <p id="deleteParagraph">${deleteParagraph}</p>
              </div>
            </div>
            <div class="modal-footer">
              <button type="submit" class="btn btn-primary">Yes</button>
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">No</button>
            </div>
          </form>
        </div>
      </div>
    </div>
    `;

  this.open = function () {
    $(containerSelector).append(modalTemplate);
    $("#deleteModal").modal("show");

    $("#deleteModal").on("hidden.bs.modal", function () {
      if (onCloseDeleteModal) onCloseDeleteModal();

      $("#deleteModal").remove();
    });

    $("#deleteItem").submit(function (e) {
      e.preventDefault();
      onDelete(itemId, $("#items"))
        .then((message) => window.DisplayToastNotification(message))
        .catch((message) => window.DisplayToastNotification(message))
        .finally(() => $("#deleteModal").modal("hide"));
    });
  };
}
