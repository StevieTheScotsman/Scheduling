


$(function () {

    $('.special-issue-container').find('.three');

    $('img.special-issue-delete').on("click", function () {
        $(this).parents('div.special-issue-delete').find('form').submit();

    });

    $('img.add-special-issue').on("click", function () {

        $(this).parent().find('.three').toggle();


    });

});



    