
//filter on projects
$(function () {

    $('.select-change-request-project').on("change", function () {

        $('#FilterByPendingChangeRequests').prop('checked', false);
        $('#ProcessManageChangeRequests').submit();
        
    });

});


//filter projects with pending changes.
$(function () {

    $('#FilterByPendingChangeRequests').on("click", function () {

        var $this = $(this);


        if ($this.is(':checked')) {

            $('.select-change-request-project').val('');
            $('#ProcessManageChangeRequests').submit();

        };

    });

});


$(function () {

    $('img.manage-change-request-save').on("click", function (e) {

        //start

        e.preventDefault();
        var curElem = $(this);

        $('<div></div>').appendTo('body')
            .html('<div><h6>Are you sure you want to make this change ?</h6></div>')
            .dialog({
                modal: true, title: 'Confirm Project Link Deletion', zIndex: 10000, autoOpen: true,
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