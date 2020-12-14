

$(function () {

    $('li.delete-activity a').on("click", function (e) {

        //prevent default behaviour
        e.preventDefault();
        var curElem = $(this);
        
        $('<div></div>').appendTo('body')
            .html('<div><h6>Are you sure you want to delete all activity records?</h6></div>')
            .dialog({
                modal: true, title: 'Confirm Activity Deletion', zIndex: 10000, autoOpen: true,
                width: 'auto', resizable: false,
                buttons: {
                    Yes: function () {
                     $(this).dialog("close");
                        window.location.href =baseUrl +  '/home/DeleteAllActivity';

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