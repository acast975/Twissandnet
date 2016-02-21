(function () {
    /* global angular */
    var app = angular.module('app.router', ['ngRoute']);

    app.controller('RouterCtrl', function ($location, $routeParams) {
        console.log($location.path)
    });
    //configure routes
    app.config(function ($routeProvider, $locationProvider) {

        $routeProvider.caseInsensitiveMatch = true;

        $routeProvider
		//*/
        .when('/index', {
            templateUrl: '/App/Components/Index/Index.html'
        })
        .when('/friends', {
            templateUrl: '/App/Components/Friends/friends.html',
            controller: 'FriendsCtrl'
        })
        .when('/public', {
            templateUrl: '/App/Components/Index/Index.html'
        })
        .when('/user/:usr', {
            templateUrl: '/App/Components/Index/Index.html'
        })
        .when('/hashtag/:tag', {
            templateUrl: '/App/Components/Index/Index.html'
        })
		.otherwise({ redirectTo: '/index' });

        $locationProvider.html5Mode(true);
    });
})();