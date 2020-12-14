


$(function () {



    function BuildJsonStringForUpdate() {

        var itemObjectArray = [];

        var count = $('#RegularDisplay').find('.edit-profile-item-record:visible').length;

        if (count > 0) {


            $('#RegularDisplay').find('.edit-profile-item-record:visible').each(


            function () {

                var itemArray = {};

                //add mf
                var mileField = $(this).find('.edit-profile-select-milestone-field').val();



                //add dep field 

                var depCount = $(this).find('.edit-profile-select-dep-field:visible').length;

                var depVal = 'null';
                if (depCount > 0) {

                    depVal = $(this).find('.edit-profile-select-dep-field').val();

                }

                //add calc field
                var calcCount = $(this).find('.edit-profile-select-calc-field:visible').length;

                var calcVal = 'null';

                if (calcCount > 0) {

                    calcVal = $(this).find('.edit-profile-select-calc-field').val();

                }

                //add fir order field 

                var firOrderVal = 'null';
                var firOrderCount = $(this).find('.edit-profile-fire-order:visible').length;

                if (firOrderCount > 0) {

                    firOrderVal = $(this).find('.edit-profile-fire-order').val();

                }


                var displayOrderVal = 'null';

                var displayOrderCount = $(this).find('.edit-profile-display-order:visible').length;

                if (displayOrderCount > 0) {

                    displayOrderVal = $(this).find('.edit-profile-display-order').val();
                }




                //add to main array
                itemArray.mileFieldVal = mileField;
                itemArray.depFieldVal = depVal;
                itemArray.calcFieldVal = calcVal;
                itemArray.firOrderFieldVal = firOrderVal;
                itemArray.displayOrderFieldVal = displayOrderVal;

                itemArray.profileID = $('#hf-profile-id').val();

                itemObjectArray.push(itemArray);


            }

            );



        } //end if count

        return itemObjectArray;


    }

    function LoadUpdateFromEditProfile() {

        var json = BuildJsonStringForUpdate();
        var str = JSON.stringify(json);
        var url = baseUrl + '/Ajax/UpdateEditProfileJson'
        $.ajax({
            url: url,
            data: { values: str },
            type: 'POST',
            success: function (data) {
                ShowSuccessFlag();
            }
        });


    }

    function ShowErrorFlag(elem) {

        var flagPath = baseUrl + '/images/warning.png';
        $(elem).parents('.edit-profile-item-record').find('.edit-profile-img-check').attr('src', flagPath).parent().show();

    }

    function ShowCheckFlag(elem) {

        var flagPath = baseUrl + '/images/check.png';
        $(elem).parents('.edit-profile-item-record').find('.edit-profile-img-check').attr('src', flagPath).parent().show();

    }





    function ValidateUniqueDisplayOrder() {

        var retStr = true;

        $('#RegularDisplay').find('.edit-profile-display-order').each(function () {

            var curOrder = $(this).val();

            if (curOrder == '' || isNaN(Number(curOrder)) || Number(curOrder) <= 0) {
                ShowErrorMessage("Display Order Required");
                ShowErrorFlag($(this));
                retStr = false;
            }


            var displayCount = 0;

            $('#RegularDisplay').find('.edit-profile-display-order').each(function () {


                if ($(this).val() == curOrder) {

                    displayCount++;
                }



            });

            if (displayCount > 1) {
                ShowErrorMessage("Display Order Needs To Be Unique");
                ShowErrorFlag($(this));
                retStr = false;

            }


        });

        return retStr;
    }



    function ValidateUniqueCalcOrder() {

        var retStr = true;

        $('#RegularDisplay').find('.edit-profile-select-dep-field').each(function () {


            var curDepVal = $(this).val()

            if (curDepVal != '') {

                var calcOrder = $(this).parents('.edit-profile-item-record').find('.edit-profile-fire-order').val();

                var orderCount = 0;
                $('#RegularDisplay').find('.edit-profile-fire-order').each(function () {

                    if ($(this).val() == calcOrder) {

                        orderCount++;

                    }

                });

                if (orderCount > 1) {

                    ShowErrorMessage("Calculation Firing Order Needs To Be Unique");
                    ShowErrorFlag($(this));
                    retStr = false;

                }

            }


        });

        return retStr;

    }


    //Validate Required Milestone Fields
    function ValidateDependancyFields() {

        var retStr = true;

        $('#RegularDisplay').find('.edit-profile-select-dep-field').each(function () {

            var depVal = $(this).val();
            ShowCheckFlag($(this));

            if (depVal != '') {




                //verify that mf is not the same
                var mv = $(this).parents('.edit-profile-item-record').find('.edit-profile-select-milestone-field').val();
                var cv = $(this).parents('.edit-profile-item-record').find('.edit-profile-select-calc-field').val();
                var co = $(this).parents('.edit-profile-item-record').find('.edit-profile-fire-order').val();
                if (mv == depVal) {
                    ShowErrorMessage("Milestone Fields And Dependant Fields Need To Be Different");
                    ShowErrorFlag($(this));
                    retStr = false;

                }

                if (cv == '') {
                    ShowErrorMessage("Calculation Field Required");
                    ShowErrorFlag($(this));
                    retStr = false;

                }

                if (co == '' || isNaN(Number(co))) {
                    ShowErrorMessage("Calc Order Field Required");
                    ShowErrorFlag($(this));
                    retStr = false;

                }

                var hasFieldVal = false;
                //ensure there is a corresponding select milestone field .We can use this one if it is the same if would barf on previous check
                $('.edit-profile-select-milestone-field').each(function () {

                    var fieldVal = $(this).val();
                    if (fieldVal == depVal) {
                        hasFieldVal = true;

                    }
                });

                if (!hasFieldVal) {

                    ShowErrorMessage("The Dependancy Does Not Have a Corresponding Milestone Entry");
                    ShowErrorFlag($(this));
                    retStr = false;

                }


            }

        }


    );


        return retStr;

    }


    //Validate Required Milestone Fields
    function ValidateRequiredMilestoneFields() {

        var retStr = true;

        $('#RegularDisplay').find('.edit-profile-select-milestone-field').each(function () {

            ShowCheckFlag($(this));

            var currentMileVal = $(this).val();


            if ($(this).val() == '') {
                ShowErrorFlag($(this));
                ShowErrorMessage('Milestones  Are Required');
                retStr = false;

            }

            else {

                var counter = 0;

                $('#RegularDisplay').find('.edit-profile-select-milestone-field').each(function () {

                    if ($(this).val() == currentMileVal) {

                        counter++;
                    }
                });

                if (counter > 1) {

                    ShowErrorMessage('Milestones Need to be Unique');
                    ShowErrorFlag($(this));
                    retStr = false;


                }

            }


        }

    );


        return retStr;

    }

    function ValidateCalcFiringOrder() {

        var retStr = true;
        var messageStr = '';

        $('#RegularDisplay').find('.edit-profile-select-milestone-field').each(function () {

            var pardepVal = $(this).parents('.edit-profile-item-record').find('.edit-profile-select-dep-field').val();
            var parMileVal = $(this).val();
            var parMileText = $(this).text();

            ShowCheckFlag($(this));

            if (pardepVal != '') {

                var curCalcOrder = $(this).parents('.edit-profile-item-record').find('.edit-profile-fire-order').val();
                var curDisplayOrder = $(this).parents('.edit-profile-item-record').find('.edit-profile-display-order').val();

                $('#RegularDisplay').find('.edit-profile-select-dep-field').each(function () {

                    if ($(this).val() != '') {

                        if ($(this).val() == parMileVal) {

                            var refDisplayOrder = $(this).parents('.edit-profile-item-record').find('.edit-profile-display-order').val();
                            var refCalcOrder = $(this).parents('.edit-profile-item-record').find('.edit-profile-fire-order').val();

                            if (parseInt(refCalcOrder) <= parseInt(curCalcOrder)) {

                                messageStr = 'The parent calculation on display order row ' + curDisplayOrder + ' is greater than the child calculation on display row ' + refDisplayOrder;
                                ShowErrorMessage(messageStr);

                            }
                        }
                    }

                });

            }

        });


        return retStr;

    }

    function RemoveErrorMessage() {

        $('.edit-profile-error p').empty();

    }



    function ValidateEntries() {

        RemoveErrorMessage();

        var boolRet = false;
        var bValidateRequiredMilestoneFields = ValidateRequiredMilestoneFields();

        if (bValidateRequiredMilestoneFields) {

            boolRet = ValidateDependancyFields()
        }

        if (boolRet) {

            boolRet = ValidateUniqueCalcOrder();

        }

        if (boolRet) {

            boolRet = ValidateUniqueDisplayOrder();
        }

        if (boolRet) {
            ValidateCalcFiringOrder();

        }

        return boolRet;

    }




    //When cloning you need to pass true otherwise the event handlers for the cloned intance will not fire..

    //----------------------------------------single methods

    function CloneDummyAddDependancy() {

        var elem = $('#RegularDisplay');
        $('#DummyMilestoneDisplay div.orig-dummy').clone(true).appendTo(elem).removeClass('orig-dummy');

    }

    function CloneDummyAddNoDependancy() {

        var elem = $('#RegularDisplay');
        $('#DummyMilestoneDisplay div.orig-dummy').clone(true).appendTo(elem).removeClass('orig-dummy');

        var elemToRemove = $('.edit-profile-item-record').last();
        $(elemToRemove).find('.edit-profile-select-dep-field').parent().hide();
        $(elemToRemove).find('.edit-profile-select-calc-field').parent().hide();
        $(elemToRemove).find('.edit-profile-fire-order').parent().hide();

    }

    function HideDummyMilestoneDisplay() {

        $('#DummyMilestoneDisplay').hide();

    }

    function HideSubmitButton() {

        $('.edit-profile-sub-click').hide();
    }

    function HideCheckFlag() {

        $('.edit-profile-img-check').parent().hide();

    }



    function HideUnselectedDropdowns() {

        $('#RegularDisplay').find('select').each(function () {

            var t = $(this).val();
            if (t == '') {

                $(this).parent().hide();
            }



        });

    }

    function HideEmptyFirOrderTextbox() {


        $('#RegularDisplay').find('.edit-profile-fire-order').each(function () {

            var t = $(this).val();
            if (t == '') {

                $(this).parent().hide();
            }



        });

    }

    function HideSuccessFlag() {


        $('.edit-profile-success').hide();


    }

    function DontShowHideFlag() {

        $('.hide-flag').hide();

    }

    function ShowHideFlag() {

        $('.hide-flag').show();

    }

    function ShowSuccessFlag() {
        $('.edit-profile-success').show().fadeOut(2000);
    }




    function ShowErrorMessage(input) {

        $('.edit-profile-error p').text(input);
        var errorDiv = $('.edit-profile-error');
        var scrollPos = errorDiv.offset().top;
        $(window).scrollTop(scrollPos);

    }

    function setEyeContainerHeights() {

        var setHeight = 0;
        //set all to max height
        $('.edit-profile-eye-container-inner').each(function () {
            var curHeight = $(this).height();
            if (curHeight > setHeight) {
                setHeight = curHeight;
            }
        });

        var hs = setHeight + 'px';

        $('.edit-profile-eye-container-inner').each(function () {

            $(this).css('height', hs);

        });



    }

    function showNodeDisplay() {


        $('#edit-profile-eye-container').show();


        var con = $('#edit-profile-eye-container');
        $(con).empty();


        var maxVal = getMaximumDisplayOrder();
        for (var i = 1; i <= maxVal; i++) {

            var baseHtml = "<div class='edit-profile-eye-container-inner rounded-corners' style='float:right;width:200px;margin-bottom:10px;margin-left:10px;border:3px solid #0769ad;padding:5px'><h4  style='padding-left:5px'>#ORDER#</h4><h4 class='rounded-corners' style='padding-left:5px;background-color:#ccc'>#NAME#</h4><p>#DEP#</p><p style='width:180px'>#CALC#</p><p>#FIRORDER#</p></div>";

            $('#RegularDisplay .edit-profile-display-order:visible').each(function () {

                var curVal = $(this).val();
                if (curVal == i) {

                    //get row reference
                    var record = $(this).parents('.edit-profile-item-record');

                    //get milestone ref
                    var mileDesc = $(record).find('.edit-profile-select-milestone-field option:selected').text();
                    baseHtml = baseHtml.replace("#NAME#", mileDesc);

                    //get display reference
                    baseHtml = baseHtml.replace("#ORDER#", curVal);
                    var disOrder = curVal;

                    //get dep reference
                    var depDesc = '';
                    var depVal = $(record).find('.edit-profile-select-dep-field option:selected').val();
                    if (depVal != '') {
                        depDesc = "<span style='font-style:italic;color:#ddd;font-weight:bold'>Depends On</span><br/>" + $(record).find('.edit-profile-select-dep-field option:selected').text();
                    }

                    baseHtml = baseHtml.replace("#DEP#", depDesc);

                    //get calc reference
                    var calcDesc = '';
                    var calcVal = $(record).find('.edit-profile-select-calc-field option:selected').val();
                    if (calcVal != '') {
                        calcDesc = "<span style='font-style:italic;color:#ddd;font-weight:bold'>Calculation</span><br/>" + $(record).find('.edit-profile-select-calc-field option:selected').text();
                    }

                    baseHtml = baseHtml.replace("#CALC#", calcDesc);

                    //get fir order

                    var firDesc = '';
                    var firVal = $(record).find('.edit-profile-fire-order').val();
                    if (firVal != '') {

                        firDesc = "<span style='font-style:italic;color:#ddd;font-weight:bold'>Calc Firing Order</span><br/>" + $(record).find('.edit-profile-fire-order').val();
                    }

                    baseHtml = baseHtml.replace("#FIRORDER#", firDesc);

                    $(con).append(baseHtml);
                }
            });

        }


    }

    function getMaximumDisplayOrder() {

        var disMax = 1;

        $('#RegularDisplay').find('.edit-profile-display-order:visible').each(function () {

            //need parseint for comparison to work
            var curVal = parseInt($(this).val());
            if (curVal > disMax) {
                disMax = curVal;

            }
        });

        return disMax;
    }

    //-------------------------------------------------------------call aggregate methods
    function initPage() {

        HideDummyMilestoneDisplay();
        HideUnselectedDropdowns();
        HideEmptyFirOrderTextbox();
        HideCheckFlag();
        HideSuccessFlag();
        DontShowHideFlag();
    }


    //--------------------------------------------------------events


    //clone with dependancy
    $('.edit-profile-img-add-dep-click').on("click", function () {

        CloneDummyAddDependancy();

    });

    //clone no dependancy

    $('.edit-profile-img-add-click').on("click", function () {

        CloneDummyAddNoDependancy();

    });


    //remove record
    $('.edit-profile-remove-click').on("click", function () {

        $(this).parents('.edit-profile-item-record').remove();

    });


    //validation

    $('.edit-profile-img-val-click').on("click", function () {

        $('.edit-profile-error p').empty();
        ValidateEntries();

    });

    //update using ajax..
    $('.edit-profile-save-click').on("click", function () {

        var retStr = ValidateEntries();

        if (retStr) {
            LoadUpdateFromEditProfile();
        }

    });

    //    $('.img-hide-flag').on("click", function () {

    //        $('#edit-profile-eye-container').toggle();

    //    });


    //eye click
    $('.edit-profile-eye-click').on("click", function () {


        $('.edit-profile-error p').empty();

        var b = $('#edit-profile-eye-container').is(":hidden");
        var imgElem = $('img.edit-profile-eye-click');

        if (b) {

            var valEntries = ValidateEntries();
            if (valEntries) {
                showNodeDisplay();
                setEyeContainerHeights();
                imgElem.attr('title', 'Hide Milestone View');
            }

        }

        else {

            $('#edit-profile-eye-container').hide();
            imgElem.attr('title', 'Show Milestone View');
        }

    });

    //page load
    initPage();

});