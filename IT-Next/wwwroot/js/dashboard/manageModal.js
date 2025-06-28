function ManageModal({
  containerSelector,
  fields = [],
  isEdit = false,
  pageName = "",
  onSave,
  onCloseManageModal,
} = props) {
  let originalForm = null;
  const modalTemplate = `
    <div class="modal fade" id="manageModal" tabindex="-1" aria-labelledby="manageModalLabel" aria-hidden="true">
	<div class="modal-dialog">
		<div class="modal-content">
			<div class="modal-header">
				<h5 class="modal-title" id="manageModalLabel">${
          isEdit ? "Edit" : "New"
        } ${pageName.toLowerCase()}</h5>
				<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
			</div>
			<form id="manageItems">
				<div class="modal-body">
                    ${fields
                      .map((field) => {
                        const value = escapeDangerousHtml(
                          field.value ?? field.defaultValue ?? ""
                        );
                        const valueAttribute =
                          (![undefined, null, ""].includes(value) &&
                            `value="${value}"`) ||
                          null;
                        const idAttribute = `id="${field.selector}"`;
                        const nameAttribute = `name="${field.selector}"`;

                        const attributes = [
                          idAttribute,
                          nameAttribute,
                          valueAttribute,
                        ]
                          .filter((attribute) => !!attribute)
                          .join(" ");

                        switch (field.type) {
                          case "text":
                            return `
                                <div class="mb-3">
                                <label class="col-form-label" for="${field.selector}">${field.label}</label>
						<input autocomplete="off" class="form-control" type="text" ${attributes}>
                        </div>
                                `;
                            break;
                          case "hidden":
                            return `
                                <input type="hidden" ${attributes}>
                                `;
                            break;
                        }
                      })
                      .join("")}					
				</div>
				<div class="modal-footer">
					<button type="submit" class="btn btn-primary" id="addBtn" disabled="disabled">${
            isEdit ? "Save" : "Add"
          }</button>
				</div>
			</form>
		</div>
	</div>
</div>
    `;

  this.open = function () {
    $(containerSelector).append(modalTemplate);
    $("#manageModal").modal("show");

    $("#manageItems").find("input:first").focus();
    $("#addBtn").attr("disabled", "disabled");
    originalForm = $("#manageItems").serialize();

    $("#manageModal").on("hidden.bs.modal", function () {
      if (onCloseManageModal) onCloseManageModal();

      $("#manageModal").remove();
    });

    const validationRules = fields
      .filter(
        ({
          isRequired = false,
          minLength = undefined,
          maxLength = undefined,
        }) => isRequired || minLength || maxLength
      )
      .reduce(
        (
          rules,
          {
            selector,
            isRequired = false,
            minLength = undefined,
            maxLength = undefined,
          }
        ) => {
          rules[selector] = {
            required: isRequired,
            minlength: minLength,
            maxlength: maxLength,
          };
          return rules;
        },
        {}
      );

    $.validator.messages.required = function (_param, input) {
      const label = $(`label.col-form-label[for="${input.id}"]`).text();
      return `The field '${label}' is required.`;
    };

    $.validator.messages.minlength = function (param, input) {
      const label = $(`label.col-form-label[for="${input.id}"]`).text();
      return `The '${label}' field minimum length is ${param}.`;
    };

    $.validator.messages.maxlength = function (param, input) {
      const label = $(`label.col-form-label[for="${input.id}"]`).text();
      return `The '${label}' field maximum length is ${param}.`;
    };

    $("#manageItems").validate({
      rules: validationRules,
    });

    $("#manageItems").data("validator").settings.ignore = null;

    $("#manageItems").on("change input blur keyup paste", "input", function () {
      if (
        $("#manageItems").serialize() !== originalForm &&
        $("#manageItems").valid()
      )
        $("#addBtn").removeAttr("disabled");
      else $("#addBtn").attr("disabled", "disabled");
    });

    $("#manageItems").submit(function (e) {
      e.preventDefault();

      const brandResource = getFormDataAsJson($(this));
      const submitType = $('#manageModal button[type="submit"]').text();

      onSave(brandResource, submitType)
        .then((message) => window.DisplayToastNotification(message))
        .catch((error) => {
          if (typeof error === "string")
            window.DisplayToastNotification(message);
          else {
            $("#manageItems").data("validator").showErrors(error);
          }
        })
        .finally(() => $("#manageModal").modal("hide"));
    });
  };
}
