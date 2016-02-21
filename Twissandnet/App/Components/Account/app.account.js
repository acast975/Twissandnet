(function () {
    /* global angular */
    var app = angular.module('app.account', []);
    app.factory('Account', function (StatusCodes, $http, $location, $route) {
        var username = undefined;
        var user = {
            isLoggedIn: function () {
                return username !== undefined;
            },
            logIn: function (user) {
                var promise = $http.post('/api/Accounts/LogIn', user)
                .then(function (event) {
                    var response = event.data;
                    if (response.status === StatusCodes.CODES.OK) {
                        username = user.username;
                        $route.reload();
                    }
                    else {
                        response.message = ACCOUNT_STATUS_MESSAGES[response.status];
                    }
                    return response;
                });
                return promise;
            },
            register: function (user) {
                var promise = $http.post('/api/Accounts/Register', user)
                .then(function (event) {
                    var response = event.data;
                    if (response.status === StatusCodes.CODES.OK) {
                        username = user.username;
                        $route.reload();
                    }
                    else {
                        response.message = ACCOUNT_STATUS_MESSAGES[response.status];
                    }
                    return response;
                });
                return promise;
            },
            logOut: function () {
                username = undefined;
                $http.get('/api/Accounts/LogOut');
                $location.path('/');
            },
            getUsername: function () {
                return !user.isLoggedIn() ? "" : username;
            }
        };

        var ACCOUNT_STATUS_MESSAGES = {
            10: 'Username is already taken',
            11: 'Username / password combination not found',
            12: 'User is logged in',
            13: 'User is not logged in'
        };
        var ACCOUNT_STATUS_CODES = {
            UsernameTaken: 10,
            UsernamePassCombinationNotFound: 11,
            UserLoggedIn: 12,
            UserNotLoggedIn: 13
        };

        //check if user is loggin on server
        $http.get('/api/Accounts/IsLoggedIn')
        .then(function (event) {
            var resp = event.data;
            if (resp.status == ACCOUNT_STATUS_CODES.UserLoggedIn) {
                username = resp.result;
            }
        });

        return user;
    });

    app.controller('AccountNavbarCtrl', function ($scope, Account) {
        $scope.Account = Account;
        $scope.loginUser = {
            username: '',
            password: '',
            message:''
        };
        $scope.registerUser = {
            username: '',
            password: '',
            confirmPassword: '',
            message: ''
        };

        $scope.logIn = function () {
            $scope.loginUser.message = '';
            $scope.Account.logIn({
                username: $scope.loginUser.username,
                password: $scope.loginUser.password
            })
            .then(function (response) {
                if (response.status) {
                    $scope.loginUser.username = '';
                    $scope.loginUser.password = '';
                    $scope.loginUser.message = response.message;
                }
                else {
                    $scope.loginUser.message = response.message;
                }
            });
            
            
        }
        $scope.register = function () {
            $scope.registerUser.message = '';
            if ($scope.registerUser.password !== $scope.registerUser.confirmPassword) {
                $scope.registerUser.message = 'Passwords do not match';
                return;
            }
            $scope.Account.register({
                username: $scope.registerUser.username,
                password: $scope.registerUser.password
            })
            .then(function (response) {
                $scope.registerUser.message = response.message;
                if (response.status) {
                    $scope.registerUser.username = '';
                    $scope.registerUser.password = '';
                    $scope.registerUser.confirmPassword = '';
                }
            });
            
        }


        $scope.$watch('[loginUser.username, registerUser.username]', function () {
                $scope.loginUser.username = $scope.loginUser.username.replace(/\W/, '');
                $scope.registerUser.username = $scope.registerUser.username.replace(/\W/, '');
        }, true);
        
    });

})();

