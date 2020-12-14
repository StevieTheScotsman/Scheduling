

$(function () {

    $('.request-container').hide();

    $('.img-edit-projects-generate-request-click').on("click", function () {

        $(this).parent().find('.request-container').toggle();

    });

    $('.button-request-submit').on("click", function () {

        $(this).parent().find('.request-textarea').css('border','none');

        var v = $(this).parent().find('.request-textarea').val().length;
        if (parseInt(v) >= 2) {

            $(this).parent().submit();

        }

        else {
            
            $(this).parent().find('.request-textarea').css('border', '1px solid red');
            
        }
        
                
    });

});