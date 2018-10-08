function alertSuccess(message) {
    $.notify({
        title: '<strong>Success</strong>',
        message: message
    }, {
            type: 'success'
        });
}

function alertWarning(message) {
    $.notify({
        title: '<strong>Success</strong>',
        message: message
    }, {
            type: 'warning'
        });
}

function alertError(message) {
    $.notify({
        title: '<strong>Error</strong>',
        message: message
    }, {
            type: 'danger'
        });

}

function processLocationLimits() {
    if ($("#gridLocations > tbody > tr").length >= 3)
        $("#btnLocation").hide();
    else
        $("#btnLocation").show();
}

function updateLastNote(eventId, successMessage) {
    $.ajax({
        type: "PUT",
        url: '/Home/EditLastNote',
        data: {EventId: eventId, LastNote: $("#LastNote").val()},
        success: function (data) {
            alertSuccess(successMessage);
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alertError(thrownError);
        }
    });
}

function addPage(element) {
    $.ajax({
        type: "GET",
        url: '/Home/AddViewPage',
        success: function (data) {
            var idview = $(element).parent().parent().find('input').val();
            data = data.replace(new RegExp("name=\"Views", 'g'), "name=\"EventViews[" + idview + "].Views");
            $(element).parent().find("table > tbody").append(data);
        }
    });
}

function addElement(gridId, addAction) {
    let form = $("#formEvent");
    $.ajax({
        type: "GET",
        url: '/Home/' + addAction,
        success: function (data) {
            $(gridId + " > tbody").append(data);
        }
    });
}

function saveFormUpdate(successMessage) {
    let form = $("#formEvent");
    if (form.valid()) {
        $.ajax({
            type: "PUT",
            url: form.attr("action"),
            data: form.serialize(),
            dataType: "html",
            success: function (data) {
                alertSuccess(successMessage);
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alertError(thrownError);
            }
        });
    }
}

function addDelivery() {
    let form = $("#formEvent");
    if (form.valid()) {
        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            dataType: "html",
            success: function (data) {
                $("#modalAddDelivery").modal('hide');
                $("#DriverId").val("");
                $("#LocationId").val("");
                $("#Destination").val("");
                $("#LunchCount").val("1");
                $("#gridDeliveries > tbody").append(data);
                alertSuccess("A new delivery was added successfully");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alertError(thrownError);
            }
        });
    }
}

function deleteDelivery(element, eventId, id) {
    $.ajax({
        type: "DELETE",
        url: "/Home/DeleteDelivery?eventId=" + eventId + "&deliveryId=" + id,
        success: function (data) {
            alertSuccess("A delivery was removed successfully");
            $(element).parent().parent().remove();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alertError(thrownError);
        }
    });
}

function wipeAllNotes(eventId) {
    alertWarning("Deleting all notes can take a while...");
    $.ajax({
        type: "DELETE",
        url: "/Home/DeleteAllNotes?eventId=" + eventId,
        success: function (data) {
            alertSuccess("All notes were successfully delete.");
            $(element).parent().parent().remove();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alertError(thrownError);
        }
    });

}

function addNoteTemplate() {
    let form = $("#formEvent");
    if (form.valid()) {
        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            dataType: "html",
            success: function (data) {
                $("#modalAddNoteTemplate").modal('hide');
                $("#Note").val("");
                $("#Sentiment").val("");
                $("#gridNoteTemplates > tbody").append(data);
                alertSuccess("A new template note was added successfully");
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alertError(thrownError);
            }
        });
    }
}

function deleteNoteTemplate(element, id) {
    $.ajax({
        type: "DELETE",
        url: "/Home/DeleteNoteTemplate?noteTemplateId=" + id,
        success: function (data) {
            alertSuccess("A note template was removed successfully");
            $(element).parent().parent().remove();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alertError(thrownError);
        }
    });
}

function addEvent() {
    let form = $("#formEvent");
    if (form.valid()) {
        $.ajax({
            type: "POST",
            url: form.attr("action"),
            data: form.serialize(),
            dataType: "html",
            success: function (data) {
                alertSuccess("A new event was added successfully");
                window.location.reload();
            },
            error: function (xhr, ajaxOptions, thrownError) {
                alertError(thrownError);
            }
        });
    }
}

function deleteEvent(id) {
    $.ajax({
        type: "DELETE",
        url: "/Home/DeleteEvent?eventId=" + id,
        success: function (data) {
            alertSuccess("An event was successfully deleted");
            window.location.reload();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            alertError(thrownError);
        }
    });
}