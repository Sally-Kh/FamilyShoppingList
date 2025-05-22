$(document).ready(function () {
  // Initialisiert die Anwendung beim Laden der Seite
  loadFamilyMembers();
  loadShoppingLists();

  // Event-Handler für das Hinzufügen von Familienmitgliedern
  $("#add-member-form").submit(function (e) {
    e.preventDefault();
    const btn = $(this).find("button[type='submit']");
    btn.prop("disabled", true);

    const memberData = {
      name: $("#member-name").val().trim(),
    };

    $.ajax({
      url: "/api/users",
      method: "POST",
      contentType: "application/json",
      data: JSON.stringify(memberData),
      success: function () {
        $("#member-name").val("");
        loadFamilyMembers();
      },
      error: function (xhr) {
        showError(
          "Fehler beim Hinzufügen: " +
            (xhr.responseJSON?.message || "Serverfehler")
        );
      },
      complete: function () {
        btn.prop("disabled", false);
      },
    });
  });

  // FIX: Korrigierter Event-Handler für das Löschen von Familienmitgliedern
  $(document).on("click", ".delete-member", function (e) {
    e.stopPropagation(); // Verhindert unerwünschtes Event-Bubbling
    const memberId = $(this).data("id");
    const $memberElement = $(this).closest(".member-badge");

    if (confirm("Dieses Familienmitglied wirklich löschen?")) {
      $.ajax({
        url: `/api/users/${memberId}`,
        method: "DELETE",
        success: function () {
          // Entfernt das Element sofort aus der Anzeige
          $memberElement.remove();

          // Aktualisiert die Dropdown-Liste
          $(`#assign-to option[value="${memberId}"]`).remove();

          // Lädt die Einkaufslisten neu (falls Artikel zugewiesen waren)
          loadShoppingLists();
        },
        error: function (xhr) {
          showError(
            "Fehler beim Löschen: " +
              (xhr.responseJSON?.message || "Serverfehler")
          );
        },
      });
    }
  });

  // Event-Handler für das Hinzufügen von Artikeln
  $("#add-item-form").submit(function (e) {
    e.preventDefault();
    const btn = $(this).find("button[type='submit']");
    btn.prop("disabled", true);

    const itemData = {
      name: $("#item-name").val().trim(),
      quantity: parseInt($("#item-quantity").val()) || 1,
      assignedUserId: $("#assign-to").val() || null,
    };

    $.ajax({
      url: "/api/items",
      method: "POST",
      contentType: "application/json",
      data: JSON.stringify(itemData),
      success: function () {
        $("#item-name").val("");
        $("#item-quantity").val("1");
        loadShoppingLists();
      },
      error: function (xhr) {
        showError(
          "Fehler beim Hinzufügen: " +
            (xhr.responseJSON?.message || "Serverfehler")
        );
      },
      complete: function () {
        btn.prop("disabled", false);
      },
    });
  });

  // Event-Handler für "Als gekauft markieren"
  $(document).on("click", ".mark-bought", function () {
    const itemId = $(this).closest("li").data("id");
    promptForMember("Wer hat diesen Artikel gekauft?").then((memberId) => {
      if (memberId) {
        $.ajax({
          url: `/api/items/${itemId}/mark-bought`,
          method: "PUT",
          contentType: "application/json",
          data: JSON.stringify({ buyerId: memberId }),
          success: loadShoppingLists,
          error: function (xhr) {
            showError(
              "Fehler beim Aktualisieren: " +
                (xhr.responseJSON?.message || "Serverfehler")
            );
          },
        });
      }
    });
  });

  // Event-Handler für das Löschen von Artikeln
  $(document).on("click", ".delete-item", function () {
    if (confirm("Artikel wirklich löschen?")) {
      const itemId = $(this).closest("li").data("id");
      $.ajax({
        url: `/api/items/${itemId}`,
        method: "DELETE",
        success: loadShoppingLists,
        error: function (xhr) {
          showError(
            "Fehler beim Löschen: " +
              (xhr.responseJSON?.message || "Serverfehler")
          );
        },
      });
    }
  });

  // Lädt alle Familienmitglieder und aktualisiert die Anzeige
  function loadFamilyMembers() {
    $.get("/api/users")
      .done(function (users) {
        $("#family-members-display").empty();
        $("#assign-to")
          .empty()
          .append('<option value="">Familienmitglied wählen</option>');

        users.forEach((user) => {
          $("#family-members-display").append(`
            <div class="member-badge rounded-pill bg-primary text-white mb-2 me-2">
              ${user.name}
              <button class="btn-close btn-close-white ms-2 delete-member" data-id="${user.id}"></button>
            </div>
          `);
          $("#assign-to").append(
            `<option value="${user.id}">${user.name}</option>`
          );
        });
      })
      .fail(() => showError("Fehler beim Laden der Familienmitglieder"));
  }

  // Lädt beide Einkaufslisten (To Buy und Bought)
  function loadShoppingLists() {
    $.get("/api/items")
      .done(function (items) {
        renderList(
          "#to-buy-list",
          items.filter((i) => !i.boughtDate)
        );
        renderList(
          "#bought-list",
          items.filter((i) => i.boughtDate)
        );
      })
      .fail(() => showError("Fehler beim Laden der Einkaufsliste"));
  }

  // Rendert eine einzelne Liste (To Buy oder Bought)
  function renderList(selector, items) {
    const $list = $(selector).empty();
    items.forEach((item) => {
      $list.append(`
        <li class="list-group-item ${
          item.boughtDate ? "bought-item" : ""
        }" data-id="${item.id}">
          <div class="d-flex justify-content-between align-items-center">
            <div>
              <div class="${
                item.boughtDate ? "text-decoration-line-through" : ""
              }">
                ${item.name} <span class="text-muted">× ${item.quantity}</span>
              </div>
              <div class="mt-1">
                ${
                  item.assignedUserId
                    ? `<span class="badge bg-primary">${item.assignedUserName}</span>`
                    : '<span class="badge bg-secondary">Nicht zugewiesen</span>'
                }
              </div>
              ${
                item.boughtDate
                  ? `<div class="mt-1 small text-muted">
                      Gekauft von ${item.buyerName} am ${formatDate(
                      item.boughtDate
                    )}
                     </div>`
                  : ""
              }
            </div>
            <div class="btn-group">
              ${
                !item.boughtDate
                  ? '<button class="btn btn-sm btn-outline-success mark-bought">✔️</button>'
                  : ""
              }
              <button class="btn btn-sm btn-outline-danger delete-item">🗑️</button>
            </div>
          </div>
        </li>
      `);
    });
  }

  // Zeigt einen Dialog zur Auswahl des Käufers an
  function promptForMember(message) {
    return new Promise((resolve) => {
      $.get("/api/users")
        .done(function (users) {
          if (users.length === 0) {
            alert("Keine Familienmitglieder vorhanden!");
            resolve(null);
            return;
          }

          const dialog = `
            <div class="modal-overlay" style="
              position: fixed;
              top: 0;
              left: 0;
              right: 0;
              bottom: 0;
              background: rgba(0,0,0,0.5);
              display: flex;
              justify-content: center;
              align-items: center;
              z-index: 1000;
            ">
              <div class="modal-content" style="
                background: white;
                padding: 20px;
                border-radius: 5px;
                width: 80%;
                max-width: 400px;
              ">
                <h5>${message}</h5>
                <select class="form-select mb-3" id="buyer-select">
                  ${users
                    .map((u) => `<option value="${u.id}">${u.name}</option>`)
                    .join("")}
                </select>
                <div class="text-end">
                  <button class="btn btn-secondary" id="dialog-cancel">Abbrechen</button>
                  <button class="btn btn-primary" id="dialog-confirm">OK</button>
                </div>
              </div>
            </div>`;

          const $dialog = $(dialog).appendTo("body");

          $("#dialog-confirm").click(() => {
            resolve($("#buyer-select").val());
            $dialog.remove();
          });

          $("#dialog-cancel").click(() => {
            resolve(null);
            $dialog.remove();
          });
        })
        .fail(() => {
          showError("Fehler beim Laden der Mitglieder");
          resolve(null);
        });
    });
  }

  // Formatiert ein Datum in deutschem Format
  function formatDate(dateString) {
    if (!dateString) return "";
    const date = new Date(dateString);
    return (
      date.toLocaleDateString("de-DE") +
      " " +
      date.toLocaleTimeString("de-DE", { hour: "2-digit", minute: "2-digit" })
    );
  }

  // Zeigt eine Fehlermeldung an
  function showError(message) {
    const $alert = $(`
      <div class="alert alert-danger alert-dismissible fade show fixed-top m-3" role="alert">
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
      </div>
    `).appendTo("body");

    setTimeout(() => $alert.alert("close"), 5000);
  }
});
