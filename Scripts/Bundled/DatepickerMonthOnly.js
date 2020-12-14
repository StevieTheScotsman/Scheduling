//https://gist.github.com/thiamteck/877276

$(document).ready(function () {
    $(".monthPicker").datepicker({
        dateFormat: 'mm-yy',
        changeMonth: true,
        changeYear: true,
        showButtonPanel: true,

        onClose: function (dateText, inst) {
            var month = $("#ui-datepicker-div .ui-datepicker-month :selected").val();
            var year = $("#ui-datepicker-div .ui-datepicker-year :selected").val();
            $(this).val($.datepicker.formatDate('yy-mm', new Date(year, month, 1)));
        }
    });

    $(".monthPicker").focus(function () {
        $(".ui-datepicker-calendar").hide();
        //added by steve
        $(".ui-datepicker-current").hide();
        $("#ui-datepicker-div").position({
            my: "center top",
            at: "center bottom",
            of: $(this)
        });
    });

    //prepopulate DatePicker if it is empty.
    var currentStartDate = $('.timelineRangeStart').val();
    var currentEndDate = $('.timelineRangeEnd').val();

    if (currentStartDate.length == 0 && currentEndDate.length == 0 && prepopulateReportTimeline)
    {
        var d = new Date();
        var startYear = parseInt(d.getFullYear()) + 1;
        var endYear = startYear + 1;
        var startVal = startYear + '-01';
        var endVal = endYear + '-02';

        $('.timelineRangeStart').val(startVal);
        $('.timelineRangeEnd').val(endVal);
    }

    $('#ExcelStyleReportsFormFilterByTimelineRange').submit(function () {

        $('.error-message').empty();

        var start = $.trim($('.timelineRangeStart').val());
        var stop = $.trim($('.timelineRangeEnd').val());

        if ($('.list-project-select-filter-timeline-range option:selected').length == 0) {
            writeErrorMessage("You must choose at least one option"); return false; 
        }

        return verifyTimelineRangeParams(start, stop);


    });

    function writeErrorMessage(input) {

        $('.error-message').html("<p class='error'>" + input + "</p>");

    }

    function verifyTimelineRangeParams(start, stop) {
        var validformat = /^\d{4}-\d{2}$/
        if (!validformat.test(start)) {
            writeErrorMessage("Invalid Start Date Format. Please correct and submit again."); return false;

        }

        if (!validformat.test(stop)) {
            writeErrorMessage("Invalid End Date Format. Please correct and submit again."); return false;
        }


        //ensure end is greater than start
        var startArray = start.split("-");
        var startDateStr = startArray[0] + "/" + startArray[1] + "/1";
        var startDate = new Date(startDateStr);

        var startMsec = startDate.getTime();


        var stopArray = stop.split("-");
        var stopDateStr = stopArray[0] + "/" + stopArray[1] + "/1";
        var stopDate = new Date(stopDateStr);

        var stopMsec = stopDate.getTime();

        var diff = stopMsec - startMsec;

        if (diff < 0) {
            writeErrorMessage('Stop Date needs  to be greater than Start Date'); return false;
        }


        return true;

    }
});