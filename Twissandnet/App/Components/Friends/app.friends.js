(function () {
    /* global angular */
    var app = angular.module('app.friends', []);
    app.controller('FriendsCtrl', function (StatusCodes, $scope, $http, Account) {
        $scope.Account = Account;
        $scope.friends = [];
        $scope.search = {
            query: '',
            message: '',
            buttonText: '',
            buttonAction: undefined
        }

        $http.get('/api/Accounts/GetFriends')
        .then(function (event) {
            if(event.data.status == StatusCodes.CODES.OK)
                $scope.friends = event.data.result;
        });

        $scope.search = function () {
            $scope.search.buttonAction = undefined;
            $scope.search.message = '';
            $scope.search.lastQuery = '';
            var query = $scope.search.query;
            if ($scope.friends.indexOf(query) > -1) {
                //this won't work if friend is added from another tab..
                //friend list is populated on /friends page load
                $scope.search.message = 'User is already your friend';
                $scope.search.buttonText = 'Remove friend';
                $scope.search.buttonAction = 'remove-friend';
                $scope.search.lastQuery = query;
                return;
            }
            if (query == Account.getUsername()) {
                $scope.search.message = 'Yes, you do exist on the website';
                return;
            }
            $http.post('/api/Accounts/UserExists', { username: query })
            .then(function (event) {
                if (event.data.result) {
                    $scope.search.message = 'User is on the website!';
                    if (Account.isLoggedIn()) {
                        $scope.search.buttonText = 'Add Friend';
                        $scope.search.buttonAction = 'add-friend';
                        $scope.search.lastQuery = query;
                    }
                    else {
                        $scope.search.message += ' Login to add a friend.';
                    }
                    
                }
                else {
                    $scope.search.message = 'User not found.';
                }
            });
        }

        $scope.buttonClick = function () {
            if ($scope.search.lastQuery) {
                var query = $scope.search.lastQuery;
                if ($scope.search.buttonAction == 'add-friend') {
                    $scope.friends.push(query);
                    $http.post('/api/Accounts/AddFriend', { username: query });
                    $scope.search.buttonAction = 'remove-friend';
                    $scope.search.buttonText = 'Remove Friend';
                    $scope.search.message = 'Friend added!';
                }
                else if ($scope.search.buttonAction == 'remove-friend') {
                    var index = $scope.friends.indexOf(query);
                    if (index > -1) {
                        $scope.friends.splice(index, 1);
                        $http.post('/api/Accounts/RemoveFriend', { username: query });
                        $scope.search.buttonAction = 'add-friend';
                        $scope.search.buttonText = 'Add Friend';
                        $scope.search.message = 'Friend removed!';
                    }
                    
                }
            }
        }
    });
})();

