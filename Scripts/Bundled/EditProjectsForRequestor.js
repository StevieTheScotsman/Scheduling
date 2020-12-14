

$(function () {



    //toggle change request
    $('img.proj-request-change').on("click", function () {

        $(this).parent().find('.textarea-proj-request-change').toggle();
        $(this).parent().find('.submit-textarea-proj-request-change').toggle();
    });


    //active filtering
    $('.req-project-select-filter').on("change", function () {
        $('#EditProjectForRequestorFormFilter').submit();

    });


});