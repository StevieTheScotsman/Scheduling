

$(function () {
   
    //active filtering
    $('.list-project-select-filter').on("change", function () {

        $('#ListProjectFormFilter').submit();

    });

    $('div.sub-toggle img').on("click", function () {

        $('tr.list-project-sub-items-row').toggle();

    });


    $('div.parent-toggle img').on("click", function () {

        $('tr.list-project-parent-item-row').toggle();

    });
 

});