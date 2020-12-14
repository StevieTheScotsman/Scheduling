
//filter on projects
$(function () {

    $('.list-select-change-request-project').on("change", function () {

        $('#FilterByPendingChangeRequests').prop('checked', false);
        $('#ProcessListChangeRequests').submit();

    });

});


//filter projects with pending changes.
$(function () {

    $('#FilterByPendingChangeRequests').on("click", function () {


        var $this = $(this);


        if ($this.is(':checked')) {

            $('.list-select-change-request-project').val('');
            $('#ProcessListChangeRequests').submit();

        };

    });

});


