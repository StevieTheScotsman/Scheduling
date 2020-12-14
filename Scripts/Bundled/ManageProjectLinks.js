$(function () {


    //PopulateProjectInfo();

    $('.link-setting-select').change(function () {
        $('.link-setting-project-submission').hide();
        PopulateProjectInfo();
    });

    //methods
    function PopulateProjectInfo() {
        var curSetVal = $('.link-setting-select').val();
        var con = $('.link-setting-select-project');
        con.empty();
        if (curSetVal != '') {
            AjaxGeneratePrimaryProjects(curSetVal, con);

        }



    }

    function AjaxGeneratePrimaryProjects(curSetVal, con) {

        var url = baseUrl + '/Ajax/GetAvailableLinkedProjectsBySecondaryProjectID/';
        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectLinkSettingID: curSetVal

            },
            success: function (result) {
                var headerStr = '<h4>Step 2 Choose Available Linking Project(s) for ' + $('.current-project-name').val() + '</h4>';
                var resultStr = result.toString();
                var currentSize = resultStr.length
                if (currentSize == 0) {
                    con.html("<p style='font-style:italic'>No projects for this selection please choose another</p>");

                }

                else {
                    var selectMarkup = "<select class='linked-project-secondary-select'><option value=''>--Choose--</option>";
                    var outerPipeArray = resultStr.split("||");
                    for (var i = 0; i <= outerPipeArray.length - 1; i++) {
                        var innerPipeArray = outerPipeArray[i].split("|");
                        var paVal = innerPipeArray[0];
                        var paText = innerPipeArray[1];
                        var paYear = innerPipeArray[2]
                        selectMarkup += "<option value=" + paVal + ">" + paText + "(" + paYear + ")</option>";
                    }

                    selectMarkup += "</select>";
                    con.html(headerStr + selectMarkup);
                    $('.linked-project-secondary-select').change(function () {
                        var cv = $(this).val();
                        $('.link-setting-project-submission').hide();
                        if (cv != '') {
                            $('.link-setting-project-submission').show();
                            $('.chosen-linkable-project').val(cv);
                            var linkSetting = $('.link-setting-select').val();
                            $('.chosen-setting-link').val(linkSetting);
                        }




                    });

                }


            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Linking Project Error: ' + xhr.responseText);
            }

        });

    }



});



//removing setting will set all affected projects due dates to null.
$(function () {

    $('img.remove-link-setting').on('click', function (e) {

        //start

        e.preventDefault();
        var curElem = $(this);

        $('<div></div>').appendTo('body')
            .html('<div><h6> Doing this will remove all duedates for affected projects.. Are you sure you want to remove this setting link ?</h6></div>')
            .dialog({
                modal: true, title: 'Confirm Project Link Deletion', zIndex: 10000, autoOpen: true,
                width: 'auto', resizable: false,
                buttons: {
                    Yes: function () {
                          $(curElem).parents('form').submit();

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

$(function () {

    $('img.remove-project-link').on("click", function (e) {

        //start
        var dialogSuffix=''
        if ($(this).hasClass('remove-project-link-reset-values')) { dialogSuffix+=' AND RESET ALL VALUES'; }
        e.preventDefault();
        var curElem = $(this);

        $('<div></div>').appendTo('body')
            .html('<div><h6>Are you sure you want to remove this link' + dialogSuffix + ' ?</h6></div>')
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