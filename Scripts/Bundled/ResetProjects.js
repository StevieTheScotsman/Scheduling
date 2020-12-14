

$(function () {

    //use ui dialog 
    $('img.reset-project-delete').on("click", function () {

        var curElem = $(this).parent().find('form').first();
        var nameElem = $(this).parents('tr').find('td.project-name-ref').text();

        $('<div></div>').appendTo('body')
  .html('<div><h6>Are you sure you want to delete  project ' + nameElem + '?</h6></div>')
  .dialog({
      modal: true, title: 'Confirm Project Deletion', zIndex: 10000, autoOpen: true,
      width: 'auto', resizable: false,
      buttons: {
          Yes: function () {
              $(curElem).submit();
              $(this).dialog("close");
          },
          No: function () {
              $(this).dialog("close");
          }
      },
      close: function (event, ui) {
          $(this).remove();
      }
  });

    });



    //use ui dialog to reset the history

    $('img.reset-project-history').on("click", function () {

        var curElem = $(this).parent().find('form').first();
        var nameElem = $(this).parents('tr').find('td.project-name-ref').text();

        $('<div></div>').appendTo('body')
  .html('<div><h6>Are you sure you want to remove all history entries for the project ' +  nameElem + ' where the project status was not created ' +  ' ?</h6></div>')
  .dialog({
      modal: true, title: 'Confirm Project History Reset', zIndex: 10000, autoOpen: true,
      width: 'auto', resizable: false,
      buttons: {
          Yes: function () {
              $(curElem).submit();
              $(this).dialog("close");
          },
          No: function () {
              $(this).dialog("close");
          }
      },
      close: function (event, ui) {
          $(this).remove();
      }
  });

    });


});