

$(function () {

    //datepicker intanstiation
    $('input.datepicker').datepicker();

    //toggle milestones  hide initially in css
    $('img.milestone-toggle').on("click", function () {
        $(this).parent().parent().find('div').not('.edit-projects-expand-logo-container').toggle();
    });

    //submit calculation and ensure we have the newsstand date
    $('img.milestone-calc').on("click", function () {

        var currentDueDate = $(this).parent().parent().parent().find('.is-newsstand-item').val();

        var hfElem = $(this).parent().find('.hfDueDate');
        $(hfElem).val(currentDueDate);

      
        var formcalcElem = $(this).parent();

        //dialog start

        $('<div></div>').appendTo('body')
         .html('<div><h6>This will recalculate the values after winding back to the profile settings.Are you sure you want to continue ?</h6></div>')
        .dialog({
            modal: true, title: 'Confirm Milestone Calculation', zIndex: 10000, autoOpen: true,
            width: 'auto', resizable: false,
            buttons: {
                Yes: function () {
                    $(formcalcElem).submit();
                    $(this).dialog("close");
                },
                No: function () {
                    $(this).dialog("close");
                }
            },
            close: function (event, ui) {
                $(this).remove();
            }
        });

        //dialog stop


    });

    //active filtering
    $('.edit-project-select-filter').on("change", function () {

        $('#EditProjectFormFilter').submit();

    });

    //history toggle
    $('.edit-projects-show-history-container img').on("click", function () {
        $(this).parent().find('.edit-project-show-history').toggle();

    });

    //submit update 
    $('img.edit-project-update').on("click", function () {

        var formElem = $(this).parents('.edit-projects-container').find('form').first();

        $('<div></div>').appendTo('body')
         .html('<div><h6>Are you sure you want to update the Project Name/Project Status/Project Lock Status</h6></div>')
        .dialog({
            modal: true, title: 'Confirm Project Deletion', zIndex: 10000, autoOpen: true,
            width: 'auto', resizable: false,
            buttons: {
                Yes: function () {
                    $(formElem).submit();
                    $(this).dialog("close");
                },
                No: function () {
                    $(this).dialog("close");
                }
            },
            close: function (event, ui) {
                $(this).remove();
            }
        });



    });

    

});