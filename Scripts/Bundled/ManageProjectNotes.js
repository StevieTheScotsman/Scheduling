
$(function () {
//prev project
    $('img.edit-prev-project').on("click", function (e) {
        $(this).parents('.prev-project-nav').find('form').submit();
    });

    //next project

    $('img.edit-next-project').on("click", function (e) {
        $(this).parents('.next-project-nav').find('form').submit();
    });

});

//LEGACY

$(function () {

    $('img.remove-project-note').on("click", function (e) {

        //start

        e.preventDefault();
        var curElem = $(this);

        $('<div></div>').appendTo('body')
            .html('<div><h6>Are you sure you want to remove this note?</h6></div>')
            .dialog({
                modal: true, title: 'Confirm Note Deletion', zIndex: 10000, autoOpen: true,
                width: 'auto', resizable: false,
                buttons: {
                    Yes: function () {
                        $(curElem).parent().submit();

                    },
                    No: function () {
                        $(this).dialog("close");
                    }
                },
                close: function (event, ui) {
                    $(this).remove();
                }
            });

        //stop

    });

});