
$(document).ready(function () {

    $('ul.responsive-tabs__list li').on("click", function () {

        //user logic
        var id = $(this).attr("id").toLowerCase();
        var u = id.indexOf('tab1');
        if (u > -1) {

            window.location.href = "/Tab/Users";
        }

        //projects logic

        var p = id.indexOf('tab2');
        if (p > -1) {

            window.location.href = "/Tab/Projects";
        }

        //activity logic

        var act = id.indexOf('tab3');
        if (act > -1) {

            window.location.href = "/Tab/Activities";
        }

        //baseline logic

        var prof = id.indexOf('tab4');
        if (prof > -1) {

            window.location.href = "/Tab/Baselines";
        }

        //change requests

        var cr = id.indexOf('tab5');
        if (cr > -1) {

            window.location.href = "/Tab/ChangeRequests";
        }

        //notifications
        var nr = id.indexOf('tab6');
        if (nr > -1) {

            window.location.href = "/Tab/Notifications";
        }

        //reporting
        var ar = id.indexOf('tab7');
        if (ar > -1) {

            window.location.href = "/Tab/Reporting";
        }

        //admin
        var adr = id.indexOf('tab8');
        if (adr > -1) {

            window.location.href = "/Tab/Administration";
        }

        var doc = id.indexOf('tab9');
        if (doc > -1) {

            window.location.href = "/Tab/Documentation";


        }

    });


});


$(document).ready(function () {

    var rootUrl = true;


    var currentUrl = window.location.href;
    currentUrl = currentUrl.toLowerCase();


    /*Users*/

    var bU1 = currentUrl.indexOf("addsingleuser") > -1;
    var bU2 = currentUrl.indexOf("editusers") > -1;
    var bU3 = currentUrl.indexOf("managegroups") > -1;
    var bU4 = currentUrl.indexOf("managegroupassociations") > -1;
    var bU5 = currentUrl.indexOf("listusers") > -1;
    var bU6 = currentUrl.indexOf("listdepartments") > -1;
    var bU7 = currentUrl.indexOf("listgroups") > -1;
    var bU8 = currentUrl.indexOf("listdepartments") > -1;
    var bU9 = currentUrl.indexOf("listroles") > -1;
    var bU10 = currentUrl.indexOf("listgroupassociations") > -1;

    if (bU1 || bU2 || bU3 || bU4 || bU5 || bU6 || bU7 || bU8 || bU9 || bU10) {

        var userElem = $('.res-tabs-user');
        userElem.addClass('responsive-tabs__panel--active');
        rootUrl = false;

    }

    /*Project */

    var bP1 = currentUrl.indexOf("projects") > -1;
    var bP2 = currentUrl.indexOf("managelinkedprojects") > -1;
    var bP3 = currentUrl.indexOf("resetprojects") > -1;
    var bP4 = currentUrl.indexOf("listmanagedprojects") > -1;
    var bP5 = currentUrl.indexOf("createmultiplestepone") > -1;
    var bP6 = currentUrl.indexOf("createmultiplesteptwo") > -1;
    var bP7 = currentUrl.indexOf("managereviewableprojects") > -1;
    var bP8 = currentUrl.indexOf("manageprojectswithonsaledateapprovedstatus") > -1;

    if (bP1 || bP2 || bP3 || bP4 || bP5 || bP6 || bP7 || bP8) {
        rootUrl = false;
        var elem = $('.res-tabs-project');
        elem.addClass('responsive-tabs__panel--active');
    }

    /*Activity */

    var bA1 = currentUrl.indexOf("listactivities") > -1;
    var bA2 = currentUrl.indexOf("deleteallactivity") > -1;

    if (bA1 || bA2) {

        rootUrl = false;
        var elem = $('.res-tabs-activity');
        elem.addClass('responsive-tabs__panel--active');

    }


    /*baseline*/

    var bB1 = currentUrl.indexOf("listprofiletypes") > -1;
    var bB2 = currentUrl.indexOf("managemilestonetreesettingprofiles") > -1;
    var bB3 = currentUrl.indexOf("addsinglemilestonetreesettingprofile") > -1;
    var bB4 = currentUrl.indexOf("verifysinglemilestonetreeprofile") > -1;
    var bB5 = currentUrl.indexOf("listmilestonetreesettingprofiles") > -1;

    if (bB1 || bB2 || bB3 || bB4 || bB5) {

        rootUrl = false;
        var elem = $('.res-tabs-profile-type');
        elem.addClass('responsive-tabs__panel--active');

    }


    /*Change Requests */

    bC1 = currentUrl.indexOf("listchangerequests") > -1;
    bC2 = currentUrl.indexOf("createchangerequest") > -1;
    bC3 = currentUrl.indexOf("managechangerequests") > -1;
    bC4 = currentUrl.indexOf("removechangerequests") > -1;

    if (bC1 || bC2 || bC3 || bC4) {
        rootUrl = false;
        var elem = $('.res-tabs-change-request');
        elem.addClass('responsive-tabs__panel--active');

    }


    /*notification*/


    bN1 = currentUrl.indexOf("listmessagingsettings") > -1;
    bN2 = currentUrl.indexOf("managemessagingsettings") > -1;
    bN3 = currentUrl.indexOf("managemessagingevents") > -1;

    if (bN1 || bN2 || bN3) {

        rootUrl = false;
        var elem = $('.res-tabs-change-notification');
        elem.addClass('responsive-tabs__panel--active');

    }

    /*reporting*/

    bR1 = currentUrl.indexOf("projectsnewsstand") > -1;
    bR2 = currentUrl.indexOf("projectscreated") > -1;

    if (bR1 || bR2) {

        var repElem = $('.res-tabs-change-reporting');

        //projects tab needs to be deactivated..kludgy ..
        var elem = $('.res-tabs-project');
        elem.removeClass('responsive-tabs__panel--active');

        repElem.addClass('responsive-tabs__panel--active');
        rootUrl = false;

    }


    /*Administration*/

    
    bA1 = currentUrl.indexOf("managemajormilestones") > -1;
    bA2 = currentUrl.indexOf("manageholidays") > -1;
    bA3 = currentUrl.indexOf("listpublicationcodes") > -1;
    bA4 = currentUrl.indexOf("listfieldaliases") > -1;
    bA5 = currentUrl.indexOf("listlinksettings") > -1;
    bA6 = currentUrl.indexOf("managelinksettings") > -1;
    bA7 = currentUrl.indexOf("manageholidaylivechange") > 1;

    if (bA1 || bA2 || bA3 || bA4 || bA5 || bA6 || bA7) {

        var adminElem = $('.res-tabs-change-administation');
        adminElem.addClass('responsive-tabs__panel--active');
        rootUrl = false;

    }



    /*Documentation*/

    bD1 = currentUrl.indexOf("appsettings") > -1;
    bD2 = currentUrl.indexOf("developernotes") > -1;
    bD3 = currentUrl.indexOf("userinformation") > -1;
    if (bD1 || bD2 || bD3) {

        var docElem = $('.res-tabs-change-documentation');
        docElem.addClass('responsive-tabs__panel--active');
        rootUrl = false;
    }

    /*On Load Select Projects Tab */
    if (rootUrl) {

        var elem = $('.res-tabs-project');
        elem.addClass('responsive-tabs__panel--active');

    }


    /*Main Logic Invocation*/

    /* http://www.petelove.com/2013/01/17/responsive-tabs-jquery-script/ */
    RESPONSIVEUI.responsiveTabs();

})



	