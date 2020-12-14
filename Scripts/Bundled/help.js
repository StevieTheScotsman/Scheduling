
//baseline help logic

$(function () {

    //init
    $('.help-project-reporting-information').hide();
    $('.help-baseline-container-toggle').hide();
    $('.help-project-container-toggle').hide();
    //reporting
  

    $('.help-baseline-reporting-toggle-click').on("click", function () {

        $('.help-project-reporting-information').toggle();

        $('html, body').animate({
            scrollTop: $(".help-baseline-reporting-toggle-click").offset().top
        }, 2000);

    });

    //


  

    $('.help-baseline-container-toggle-click').on("click", function () {

       
        $('.help-baseline-container-toggle').toggle();


        $('html, body').animate({
            scrollTop: $("img.help-baseline-container-toggle-click").offset().top
        }, 2000);

    }


     );

    //ensure baseline doc section open when we link to it
    $('.open-hidden-baseline').on("click", function () {

        $('.help-baseline-container-toggle').show();
    });



    //project help logic start 

   // $('.help-project-container-toggle').hide();

    $('.help-project-container-toggle-click').on("click", function () {

        $('.help-project-container-toggle').toggle();


        $('html, body').animate({
            scrollTop: $("img.help-project-container-toggle-click").offset().top
        }, 2000);

    }


     );

    //stop

});



    