

$(function () {

    $('.change-request-information').hide();

    $('img.list-project-change-requests').on("click", function () {
        $(this).parents('.change-request-container-partial').find('.change-request-information').toggle();
    })

});