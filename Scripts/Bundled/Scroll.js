
$(function () {

    $('.information-container').show();

    $(window).scroll(function () {
      $('.information-container').hide();
    })


});


$(function () {

    $('.backtotop').hide();

    var r = parseInt($('#main').length);
    var u = parseInt($('ul.responsive-tabs__list:visible').length);

    if (r > 0 && u == 0) {

        $('html, body').animate({ scrollTop: $('#main').offset().top - 50 }, 'slow');
        $('.backtotop').show();
    }

    $('.backtotop').on("click", function () {

        $('html, body').animate({ scrollTop: $('body').offset().top - 50 }, 'slow');

    });

});

//In Administration tab nav to element with this class 
$(function () {

    if ($('.scroll-to-on-load').length > 0) {
        $('html, body').animate({ scrollTop: $('.scroll-to-on-load').offset().top }, 'slow');
    }

});

