(function () {
    /* global angular */
    var app = angular.module('app', [
          'ngRoute'
        , 'ngCookies'
        , 'ngSanitize'
        , 'app.router'
        , 'app.account'
        , 'app.friends'
        , 'app.tweets'
    ]);
    /**
    * Status codes are only global codes
    * Local codes like Account codes are in each module
    */
    app.factory('StatusCodes', function () {
        var StatusCodes = {
            NoStatus: 0,
            OK: 1
        };
        var StatusMessages = {
            0: 'No Status',
            1: 'OK'
        };
        return {
            CODES: StatusCodes,
            MESSAGES: StatusMessages
        };
    });

})();


$(function() {
    // Setup drop down menu
    $('.dropdown-toggle').dropdown();
 
    // Fix input element click problem
    $('.dropdown input, .dropdown label').click(function(e) {
        e.stopPropagation();
    });
});
