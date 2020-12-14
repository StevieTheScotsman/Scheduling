
$(document).ready(function () {

    //link logic
    $('.manage-single-project-link-inner').hide();
    $('.manage-single-project-link img.image-link').click(function () { $('.manage-single-project-link-inner').toggle(); });
    $('.delete-project-link').click(function () { $(this).parent().submit(); });

    //fade out add remove success
    $('.add-remove-milestone-success').fadeOut(5000);

    //This markup is used on create change request .Disable scrollTo on create change request
    var s = $('.edit-projects-inner-container').length;

    if (s > 0) {
        var curLoc = window.location.href;
        if (curLoc.indexOf('CreateChangeRequest') == -1) {
            if (curLoc.indexOf('ManageSingleProject?')) {
                $('html, body').animate({ scrollTop: $('.manage-project-navigation').offset().top - 100 }, 'fast');
            }

            else {
                $('html, body').animate({ scrollTop: $('.edit-projects-inner-container').offset().top - 50 }, 'slow');
            }
        }

    }


    var my_options = $("#remove-project-milestone-select option.sortme");
    var first_entry = "<option value=''>Choose A Milestone For Deletion</option>";

    my_options.sort(function (a, b) {
        if (a.text > b.text) return 1;
        else if (a.text < b.text) return -1;
        else return 0
    })
    //sort removal dropdown
    $("#remove-project-milestone-select").empty().append(first_entry).append(my_options);
    $("#remove-project-milestone-select option:eq(0)").prop('selected', true);

    //sort add dependancy dropdown
    var add_options = $("#edit-project-add-milestone-dependancy option.sortme");
    var add_first_entry = "<option value=''>Choose An Existing Project Milestone</option>";

    add_options.sort(function (a, b) {
        if (a.text > b.text) return 1;
        else if (a.text < b.text) return -1;
        else return 0
    })

    $("#edit-project-add-milestone-dependancy").empty().append(add_first_entry).append(add_options);
    $("#edit-project-add-milestone-dependancy option:eq(0)").prop('selected', true);




    /*CRUD SECTION START */
    $('.edit-project-inner-container-remove').hide();
    $('.edit-project-inner-container-add').hide();

    $('#remove-project-milestone-select').on("change", function () {

        var sv = $(this).val();
        var curChoice = $(this).find('option:selected').text();
        if (sv != '') {
            var CurrentProjectID = $('#ProjectID').val();
            var CurrentMilestoneFieldVal = sv;
            var url = baseUrl + '/Ajax/HasDependantFields/'

            $.ajax({
                type: 'POST',
                url: url,
                cache: false,
                dataType: 'json',
                data: {
                    ProjectID: CurrentProjectID,
                    MilestoneFieldID: CurrentMilestoneFieldVal


                },
                success: function (data) {

                    if (data == '1') {

                        alert("This milestone has dependencies..Please remove dependencies before deleting the item");

                    }

                    if (data == '0') {


                        //start 
                        $('<div></div>').appendTo('body')
                      .html('<div><h6>Are you sure you want to remove ' + curChoice + '?</h6></div>')
                      .dialog({
                          modal: true, title: 'Confirm Milestone Removal', zIndex: 10000, autoOpen: true,
                          width: 'auto', resizable: true,
                          buttons: {
                              Yes: function () {
                                  AjaxRemoveProjectMilestone(CurrentProjectID, CurrentMilestoneFieldVal)
                                  $(this).dialog("close");
                              },
                              No: function () {
                                  $(this).dialog("close");
                              }


                          },

                          close: function (event, ui) {

                          }
                      });



                        //stop


                    }



                },
                error: function (xhr, ajaxOptions, error) {
                    alert('Error: ' + xhr.responseText);
                }



            });




        }

    });

    //add/removal toggle
    $('img.milestone-plus-minus-toggle').on("click", function () {
        $('.edit-project-inner-container').toggle();

    });

    //add/add with dep/remove radio button
    $("input[name='edit-project-radio-group']").on("change", function () {

        var gv = $(this).val();

        if (gv == '1') {
            $('.edit-project-inner-container-remove').hide();
            $('.edit-project-inner-container-add').show();
        }



        if (gv == '2') {
            $('.edit-project-inner-container-add').hide();
            $('.edit-project-inner-container-remove').show();
        }



    });


    /*STOP*/

    //edit single milestone edit 

    $('img.manage-single-project').on("click", function () {
        $(this).parent().submit();

    });

    //help toggle

    $('img.image-help').on("click", function () {

        $('div.manage-single-project-help-inner').toggle();

    });

    //expand container 
    $('div.milestone-item').show();


    //note 
    $('div.manage-single-project-note').hide();

    $('img.image-note').on("click", function () {

        $('div.manage-single-project-note').toggle();

    });



    function DisplaySingleDependant(input) {


        $('.milestone-field-hidden').each(function () {


            if ($(this).val() == input) {


                $(this).parent().find('p').first().css('color', '#ffa500').css('font-weight', 'bold');
            }
        });

    }

    function DisplayDependants(input) {


        var i = input.indexOf(',');
        if (i > -1) {

            var strArray = input.split(",");
            for (var j = 0; j < strArray.length; j++)
                DisplaySingleDependant(strArray[j]);

        }

        else {

            DisplaySingleDependant(input);

        }

    }

    function AjaxShowSuccess(CurrentMilestoneFieldVal) {

        $('.milestone-field-hidden').each(function () {

            if ($(this).val() == CurrentMilestoneFieldVal) {
                $(this).parents('.milestone-item').find('.img-ajax-success').show().fadeOut(2000);
            }

        });

    }

    function AjaxRemoveProjectMilestone(CurrentProject, CurrentMilestoneField) {

        var url = baseUrl + '/Ajax/AjaxRemoveProjectMilestone/';
        //start 

        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectID: CurrentProject,
                MilestoneFieldID: CurrentMilestoneField


            },
            success: function (data) {

                //start 
                setTimeout(function () {

                    window.location.href = baseUrl + "/home/ManageSingleProjectAfterDependencyAjaxCall/" + CurrentProject;


                }, 3000);
                //stop

            },
            error: function (xhr, ajaxOptions, error) {
                alert('Error: ' + xhr.responseText);
            }



        });



        //stop



    }

    function ResetDueDate(CurrentMilestoneFieldVal) {
        $('.milestone-field-hidden').each(function () {

            if ($(this).val() == CurrentMilestoneFieldVal) {
                var OldDate = $(this).parents('.milestone-item').find('.original-due-date-hidden').val();
                $(this).parents('.milestone-item').find('.datepicker').val(OldDate);
            }

        });

    }
    //tests ok
    function AjaxNoDependencySimpleUpdate(CurrentProject, CurrentMilestoneFieldVal, CurrentDueDate) {


        var url = baseUrl + '/Ajax/AjaxNoDependencySimpleUpdate/';

        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectID: CurrentProject,
                MilestoneFieldFK: CurrentMilestoneFieldVal,
                DueDate: CurrentDueDate

            },
            success: function (data) {
                AjaxShowSuccess(CurrentMilestoneFieldVal);

            },
            error: function (xhr, ajaxOptions, error) {
                alert('Error: ' + xhr.responseText);
            }



        });


    }

    function AjaxShowDependants(CurrentProject, CurrentMilestone) {

        var url = baseUrl + '/Ajax/GetDependantFields/';
        //start return comma separated string.
        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectID: CurrentProject,
                MilestoneFieldID: CurrentMilestone

            },
            success: function (data) {
                DisplayDependants(data);

            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Error: ' + xhr.responseText);
            }



        });


    }


    //update keeping dependancies
    function AjaxUpdateKeepingDependants(CurrentProject, CurrentMilestone, CurDueDate) {

        var url = baseUrl + '/Ajax/AjaxUpdateKeepingDependants/';
        //start return comma separated string.
        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectID: CurrentProject,
                MilestoneFieldFK: CurrentMilestone,
                DueDate: CurDueDate

            },
            success: function (data) {
                AjaxShowSuccess(CurrentMilestone);
                setTimeout(function () {

                    window.location.href = baseUrl + "/home/ManageSingleProjectAfterDependencyAjaxCall/" + CurrentProject;


                }, 3000);

            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Error: ' + xhr.responseText);
            }



        });


    }
    //

    //update keeping dependancies
    function AjaxUpdateBreakingDependants(CurrentProject, CurrentMilestone, CurDueDate) {

        var url = baseUrl + '/Ajax/AjaxUpdateBreakingDependants/';
        //start return comma separated string.
        $.ajax({
            type: 'POST',
            url: url,
            dataType: 'json',
            data: {
                ProjectID: CurrentProject,
                MilestoneFieldFK: CurrentMilestone,
                DueDate: CurDueDate

            },
            success: function (data) {
                AjaxShowSuccess(CurrentMilestone);

            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Error: ' + xhr.responseText);
            }



        });


    }
    //

    //single note click steve 3/20
    $('.save-note').on("click", function () {
        var url = baseUrl + '/Ajax/AjaxUpdateNoteField/';
        var currentElem = $(this);
        var CurrentProjectID = $(this).parents('.note-container').find('.project-identifier').val();
        var CurrentNoteLabelID = $(this).parents('.note-container').find('.note-identifier').val();
        var CurrentNoteValue = $(this).parents('.note-container').find('.note-field-value').val();


        $.ajax({
            type: 'POST',
            url: url,
            cache: false,
            dataType: 'json',
            data: {
                ProjectID: CurrentProjectID,
                NoteLabelID: CurrentNoteLabelID,
                NoteValue: CurrentNoteValue


            },
            success: function (data) {
                $(currentElem).parents('.note-container').find('.img-ajax-success').show().fadeOut(2000);

            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Note Error: ' + xhr.responseText);
            }



        });

        //ajax stop


    });


    //single milestone click
    $('.single-milestone-edit').on("click", function () {

        var CurrentMfVal = $(this).parent().find('.milestone-field-hidden').val();
        var CurrentProjectID = $("#ProjectID").val();
        var CurElem = $(this);
        var CurMileName = $(this).parent().find('p').first().text();
        var CurDueDate = $(this).parent().find('.datepicker').val();
        var OldDueDate = $(this).parent().find('.original-due-date-hidden').val();

        //reset 
        $('.milestone-item').find('p').css('color', 'black').css('font-weight', 'normal').css('text-decoration', 'none');

        var url = baseUrl + '/Ajax/HasDependantFields/';

        $.ajax({
            type: 'POST',
            url: url,
            dataType: 'json',
            data: {
                ProjectID: CurrentProjectID,
                MilestoneFieldID: CurrentMfVal

            },
            success: function (data) {
                var r = data;
                var executed = false;
                if (parseInt(data) == '1') {

                    //if no action taken or cancel checked reset the due date.

                    $('<div></div>').appendTo('body')
                      .html('<div><h6>Are you sure you want to change the due date for ' + CurMileName + ' to ' + CurDueDate + ' ?</h6></div>')
                      .dialog({
                          modal: true, title: 'Confirm Milestone Due Date Change', zIndex: 10000, autoOpen: true,
                          width: 'auto', resizable: true,
                          buttons: {
                              UpdateKeepingDependencies: function () {
                                  AjaxUpdateKeepingDependants(CurrentProjectID, CurrentMfVal, CurDueDate);
                                  executed = true;
                                  $(this).dialog("close");
                              },
                              UpdateBreakingDependencies: function () {
                                  AjaxUpdateBreakingDependants(CurrentProjectID, CurrentMfVal, CurDueDate);
                                  executed = true;
                                  $(this).dialog("close");
                              },

                              Cancel: function () {

                                  ResetDueDate(CurrentMfVal);
                                  $(this).dialog("close");
                              }


                          },

                          close: function (event, ui) {
                              if (!executed) {



                              }
                              $(this).remove();
                          }
                      });

                    $('.milestone-field-hidden').each(function () {

                        if ($(this).val() == CurrentMfVal) {
                            $(this).parents('.milestone-item').find('p').first().css('font-weight', 'bold').css('text-decoration', 'underline');
                        }

                    });

                    //show dependants
                    AjaxShowDependants(CurrentProjectID, CurrentMfVal);



                }

                //No Dependancies ...tests ok
                else {

                    if (CurDueDate != '') {


                        //start
                        $('<div></div>').appendTo('body')
                      .html('<div><h6>Are you sure you want to change the due date for ' + CurMileName + ' to ' + CurDueDate + ' ?</h6></div>')
                      .dialog({
                          modal: true, title: 'Confirm Milestone Due Date Change', zIndex: 10000, autoOpen: true,
                          width: 'auto', resizable: false,
                          buttons: {
                              Yes: function () {
                                  AjaxNoDependencySimpleUpdate(CurrentProjectID, CurrentMfVal, CurDueDate);
                                  $(this).dialog("close");
                              },
                              No: function () {
                                  ResetDueDate(CurrentMfVal);
                                  $(this).dialog("close");
                              }
                          },
                          close: function (event, ui) {
                              $(this).remove();
                          }
                      });

                        //stop

                    }

                }
            },
            error: function (xhr, ajaxOptions, error) {
                alert(xhr.status);
                alert('Error: ' + xhr.responseText);
            }

        });

    });

});