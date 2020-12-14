

$(function () {

    function ExecuteAjaxCall(idList) {

        $.ajaxSetup({ cache: false });
        var url = baseUrl + '/Ajax/UpdateReviewedProjects/'
        //start
        $.ajax({
            url: url,
            type: 'POST',
            data: { input: idList },
            success: function (result) {
                alert('baseurl is' + baseUrl);
                window.location.href = baseUrl + '/home/ManageReviewableProjects';

            },
            error: function () {
                alert("error updating reviewable projects");
            }
        });



        //stop

    }

    $('#ReviewAcceptAllAndSubmit').click(function () {
        var $this = $(this);

        if ($this.is(':checked')) {
            var idList = '';
            var allProjects = $('.hf-project-id');
            $(allProjects).each(function () {

                idList = idList + $(this).val() + ',';


            });

            idList = idList.substring(0, idList.length - 1);

            //start

            //dialog start

            $('<div></div>').appendTo('body')
         .html('<div><h6>This will update the Project Status To On Sale Date Approved For All Entries.Are you sure you wish to continue ?</h6></div>')
        .dialog({
            modal: true, title: 'Confirm Mass Review Select', zIndex: 10000, autoOpen: true,
            width: 'auto', resizable: false,
            buttons: {
                Yes: function () {
                    ExecuteAjaxCall(idList);
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


        }
    });

});