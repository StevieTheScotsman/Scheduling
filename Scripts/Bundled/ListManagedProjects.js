

$(function () {

    
    //milestone summary
    $('.manage-project-partial').find('tbody').hide();

    $('.img-manage-project-partial').on("click", function () {

        $(this).parents('.manage-project-partial').find('tbody').toggle();

    });

    //dependancy summary
    $('.manage-project-dependancy-partial').find('tbody').hide();

    $('.img-manage-project-dependancy-partial').on("click", function () {

        $(this).parents('.manage-project-dependancy-partial').find('tbody').toggle();
    });

    //slider summary
    $('.img-manage-project-partial-slider').on("click", function () {

        $(this).parents('.slider-container').find('.slider-content-container').toggle();
    });

    //change request invoke

    $('.image-change-request-partial').on("click", function () {
        $(this).parent().submit();
    });

    //help text version 3.1
    $('.image-help').on("click", function () {
        $(this).parents('.icon-help-container').find('.image-help-content').toggle();
    });


});