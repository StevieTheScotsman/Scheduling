$(document).ready(function () {

    //methods

    function BuildEyeContainerContents() {

        $('#eye-container').empty();

        var baseHtml = "<div class='eye-container-inner rounded-corners' style='float:right;width:200px;margin-bottom:10px;margin-left:10px;border:3px solid #0769ad;padding:5px'><h4 class='disOrder-#DIS#' style='padding-left:5px'>#ORDER#</h4><h4 class='rounded-corners' style='padding-left:5px;background-color:#ccc'>#NAME#</h4><p>#DEP#</p><p>#CALC#</p><p>#FIRORDER#</p></div>";

        var n = parseInt($('#hf-num-records').val())


        for (var i = 1; i <= n; i++) {


            if ($('#MilestoneField-' + i).val() != '') {

                //get dependant field if present
                var depVal = $('#DepField-' + i).val();
                var depInfo = '';
                var calcInfo = '';
                var firInfo = '';

                if (depVal != '') {

                    var calcVal = $('#CalcField-' + i).val();


                    if (calcVal != '') {
                        calcInfo = 'Calc is ' + $('#CalcField-' + i + ' option:selected').text();


                    }

                    var firVal = $('#FirOrderField-' + i).val();
                    if (firVal != '') {

                        firInfo = 'Fir Order is <strong>' + $('#FirOrderField-' + i + ' option:selected').text() + '</strong>';
                    }


                    depInfo = $('#DepField-' + i + ' option:selected').text();

                }

                var curOrder = $('#DisplayField-' + i).val();

                var curName = $('#MilestoneField-' + i + ' option:selected').text();
                var curHtml = baseHtml.replace("#ORDER#", curOrder).replace("#NAME#", curName).replace("#DEP#", depInfo).replace("#DIS#", curOrder);

                //replace calc 
                curHtml = curHtml.replace("#CALC#", calcInfo);
                //replace firorder
                curHtml = curHtml.replace("#FIRORDER#", firInfo);


                $('#eye-container').append(curHtml);

            }

            var setHeight = 0;
            //set all to max height
            $('.eye-container-inner').each(function () {
                var curHeight = $(this).height();
                if (curHeight > setHeight) {
                    setHeight = curHeight;
                }
            });

            var hs = setHeight + 'px';

            $('.eye-container-inner').each(function () {

                $(this).css('height', hs);

            });


        }


        //<!--start -->
        var nm = parseInt($('#hf-NumDefaultMilestones').val());
        //sort function..get from config file
        $('.eye-sort-container').empty();

        for (var i = 1; i <= nm; i++) {

            var e = $('.disOrder-' + i);

            var es = $('.disOrder-' + i).length;
            if (es > 0) {

                var ve = e.text();
                var ett = $('.disOrder-' + ve).parent();
                //appending auto removes source.              
                $('.eye-sort-container').append(ett);
            }


        };



    }

    function ShowBottomRow() {

        $('div.bottom-row').show();
        HideSubmit();
    }

    function HideSubmit() {

        $('div.sub-click').hide();

    }

    function GenerateFirstField(data) {
        //id in data contains proj type.
        $('#DivField-1').show();
        ShowOnlyMilestoneFieldForTheRow(1);
        PreselectFirstSelectDropdown(data);
        ShowBottomRow();
        RecordNumOfRecords('1');
        $('.create-milestone-eye').show();

    }

    function RecordNumOfRecords(i) {

        $('#hf-num-records').val(i);

    }

    function IncrementNumOfRecords() {

        var c = parseInt($('#hf-num-records').val());
        c += 1;
        $('#hf-num-records').val(c);
    }

    function ResetRecordCount() {

        $('#hf-num-records').val('0');

    }

    function GetCurrentRecordCount() {

        return parseInt($('#hf-num-records').val());
    }


    function OnlyShowProfileSelect() {
        $('.bottom-row').hide();
        $('.item-record').hide();
    }

    function ShowOnlyMilestoneFieldForTheRow(input) {
        $('#DepField-' + input).parent().hide();
        $('#ParentField-' + input).parent().hide();
        $('#CalcField-' + input).parent().hide();
        $('#FirOrderField-' + input).parent().hide();
        $('#RangeField-' + input).parent().hide();
        $('#RemoveField-' + input).parent().hide();
        $('#CheckField-' + input).parent().hide();
    }



    function PreselectFirstSelectDropdown(data) {

        var magtypeID = $('#hf-MagProjTypeValue').val();
        var profVal = data.id;


        if (magtypeID == profVal) {

            var nsID = $('#hf-NewstandValue').val();
            $('#MilestoneField-1').val(nsID);

            //need to put in hidden fields otherwise they wont be picked up on the server since they are disabled.
            $('#hf-milestone-profile').val($('#MilestoneProfile').val());
            $('#hf-first-field-value').val(nsID);

            $('#MilestoneField-1').prop('disabled', true);
            $('#MilestoneProfile').prop('disabled', true);
            $('.select-range-create-milestone-setting').parent().hide();
            $('.select-parent-create-milestone-setting').parent().hide();
        }

    }

    function GetLastSelectedValidSelectedValue() {

        var retStr = '';

        $('.select-create-milestone-setting').each(function () {

            var cv = $(this).val();
            if (cv !== '') {
                retStr = cv;
            }
        });

        return retStr;
    }

    function ClearCheckIcons() {

        $('.img-check').parent().hide();

    }

    function ValidateUniqueMilestoneField(mf) {

        var counter = 0;

        $('div.item-record:visible').find('.select-create-milestone-setting').each(

     function () {

         var e = $(this).val();

         if (e != '') {

             if (e == mf) {

                 counter++;
             }

         }

     });


        return counter == 1;


    }

    function ValidateUniqueFiringOrder(fv) {

        var counter = 0;

        $('div.item-record:visible').find('.select-order-create-milestone-setting').each(

     function () {

         var e = $(this).val();

         if (e != '') {

             if (e == fv) {

                 counter++;
             }

         }

     });


        return counter == 1;

    }


    function UniqueAndValidFiringOrder(input) {

        if (isNaN(input)) {
            return false;
        }



        var counter = 0;

        $('div.item-record:visible').find('.textbox-display').each(

             function () {

                 var e = $(this).val();

                 if (e != '') {

                     if (e == input) {

                         counter++;
                     }

                 }

             });


        return counter == 1;

    }

    //events click refresh
    $('.create-milestone-reset').on("click", function () {

        window.location.href = window.location.href;

    });

    //events click eye

    $('.create-milestone-eye').on("click", function () {

        BuildEyeContainerContents();

    });



    //events add milestone
    $('.img-add-click').on("click", function () {
        $('.sub-click').hide();
        ClearCheckIcons();
        IncrementNumOfRecords();
        var cc = GetCurrentRecordCount();
        $('#DivField-' + cc).show();
        ShowOnlyMilestoneFieldForTheRow(cc);
        $('#RemoveField-' + cc).parent().show();

    });

    //events add milestone dependancy populate dep field with last known value.
    $('.img-add-dep-click').on("click", function () {

        ClearCheckIcons();
        $('.sub-click').hide();
        IncrementNumOfRecords();
        var cc = GetCurrentRecordCount();
        $('#DivField-' + cc).show();
        var cv = GetLastSelectedValidSelectedValue();
        $('#DepField-' + cc).val(cv);
        $('#CheckField-' + cc).parent().hide();

    });

    //events remove button clicked set selection to empty char and hide the div
    $('.remove-click').on("click", function () {

        var cid = $(this).attr('id');
        var ar = cid.split('-');
        var s = ar[1];
        $('#MilestoneField-' + s).val('');
        $('#DivField-' + s).hide();

    });

    //event help
    $('.create-milestone-help').on("click", function () {

        $('p.p-help').toggle();

    });

    //events validate 
    $('.img-val-click').on("click", function () {

        var canSubmit = true;
        $('.sub-click').hide();

        var message = '';

        $('div.item-record:visible').each(

        function () {

            var i = $(this).attr('id');
            var ar = i.split('-');
            var s = ar[1];

            $('#CheckField-' + s).parent().show();
            $('#CheckField-' + s).attr('src', '/images/check.png');

            //ensure milestonefield is selected
            var mv = $('#MilestoneField-' + s).val();

            if (mv == '') {
                $('#CheckField-' + s).attr('src', '/images/warning.png');
                canSubmit = false;
            }


            else {

                var bUniqueMilestoneField = ValidateUniqueMilestoneField(mv);
                if (!bUniqueMilestoneField) {

                    $('#CheckField-' + s).attr('src', '/images/warning.png');
                    canSubmit = false;
                }

            }


            //if there is a dependancy ensure it has a calc and a valid firing order.

            var dv = $('#DepField-' + s).val();

            if (dv !== '') {

                var calcVal = $('#CalcField-' + s).val();
                var firVal = $('#FirOrderField-' + s).val();


                if (calcVal == '' || firVal == '') {

                    $('#CheckField-' + s).attr('src', '/images/warning.png');
                    canSubmit = false;
                }

                //ensure dep and milestone are unique

                if (dv == mv) {

                    $('#CheckField-' + s).attr('src', '/images/warning.png');
                    canSubmit = false;
                }

            }


            //if we have a calculation ensure it has a dep and fir order
            var cv = $('#CalcField-' + s).val();

            if (cv !== '') {

                var depVal = $('#DepField-' + s).val();
                var firVal = $('#FirOrderField-' + s).val();


                if (depVal == '' || firVal == '') {

                    $('#CheckField-' + s).attr('src', '/images/warning.png');
                    canSubmit = false;

                }

            }
            //ensure display order is a number and is unique
            var disVal = $('#DisplayField-' + s).val();
            var bUniqueAndValidFiringOrder = UniqueAndValidFiringOrder(disVal)

            if (!bUniqueAndValidFiringOrder) {

                $('#CheckField-' + s).attr('src', '/images/warning.png');
                canSubmit = false;

            }


            //if we have a firing order ensure it has a dep and a calc order

            var fv = $('#FirOrderField-' + s).val();

            if (fv !== '') {

                var depVal = $('#DepField-' + s).val();
                var CalcVal = $('#CalcField-' + s).val();


                if (depVal == '' || CalcVal == '') {

                    $('#CheckField-' + s).attr('src', '/images/warning.png');
                    canSubmit = false;

                }

                //ok check it is unique
                else {

                    var bValidateUniqueFiringOrder = ValidateUniqueFiringOrder(fv);
                    if (!bValidateUniqueFiringOrder) {

                        $('#CheckField-' + s).attr('src', '/images/warning.png');
                        canSubmit = false;
                    }

                }

            }

        });

        //Ensure DepAndMilestone are Different


        if (canSubmit) {

            $('#hf-can-submit').val('y');
            $('.sub-click').show();

        }

    });


    //Profile Change..lock after first Milestone..
    $('#MilestoneProfile').change(function () {


        $.ajaxSetup({ cache: false });

        var curVal = $(this).val();
        var url = baseUrl + '/Ajax/GetProjectType/'

        $.ajax({
            url:url,
            type: 'POST',
            data: { input: curVal },
            success: function (result) {
                GenerateFirstField(result);

            },
            error: function () {
                alert("error");
            }
        });
        return false;
    });




    //page load
    $('p.p-help').hide();
    ResetRecordCount();
    OnlyShowProfileSelect();



});



