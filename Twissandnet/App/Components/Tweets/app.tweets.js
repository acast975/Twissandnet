(function () {
    var app = angular.module('app.tweets', []);

    app.factory('Tweets', function (StatusCodes, Account, $http, $location, $routeParams) {

        var urlOrigin = window.location.origin;

        var tweets = {
            tweets: [],
            source: '',
            onLocationChange: function (url) {
                tweets.tweets = [];
                if (!url) {
                    url = window.location.href;
                }
                var params = {};
                url = url.substr(urlOrigin.length + 1);
                var requestUrl = '/api/Tweets/';
                if (url === 'public' || (!Account.isLoggedIn() && url === 'index')) {
                    tweets.source = 'Public Tweets';
                    if(!Account.isLoggedIn())
                        tweets.source += ' - login to view your feed';
                    requestUrl += 'GetPublicTweets';
                }
                else if (url === 'index') {
                    tweets.source = 'Your friends tweets';
                    requestUrl += 'GetUserFeed';
                }
                else if (url.startsWith('user/')) {
                    var user = url.substr(('user/').length);
                    tweets.source = user + '\'s Timeline';
                    requestUrl += 'GetUserTweets';
                    params.user = user;
                }
                else if (url.startsWith('hashtag/')) {
                    var tag = url.substr(('hashtag/').length);
                    tweets.source = 'Tweets containing #' + tag;
                    requestUrl += 'GetHashtagTweets';
                    params.hashtag = tag;
                }
                else {
                    console.warn('should not happen');
                    return;
                }

                if (Object.keys(params).length) {
                    requestUrl += "?";
                    for (var key in params) {
                        requestUrl += key + '=' + params[key] + '&';
                    }
                    //strip &, not really needed
                    requestUrl = requestUrl.substr(0, requestUrl.length - 1);
                }

                $http.get(requestUrl, params)
                .then(function (resp) {
                    if (resp.data.status == StatusCodes.CODES.OK) {
                        tweets.tweets = parseTweets(resp.data.result);
                    }
                    
                });

            },
            postTweet: function (tweet) {
                tweet.username = Account.getUsername();
                tweet.timestamp = Date.now();
                var parsedTweet = parseTweet(tweet);
                $http.post('/api/Tweets/PostTweet', parsedTweet);

                addTweet(parsedTweet);
            },
            type: 'tweets'
        };

        var addTweet = function (tweet) {
            var url = window.location.href;
            url = url.substr(urlOrigin.length + 1);
            var addTweet = true;
            if (url.startsWith('user/')) {
                var user = url.substr(('user/').length);
                addTweet = user == Account.getUsername() ||
                    tweet.users.indexOf(user) > -1;
                
            }
            else if (url.startsWith('hashtag/')) {
                var tag = url.substr(('hashtag/').length);
                addTweet = tweet.hashtags.indexOf(tag) > -1;
            }
            if (addTweet) {
                //add new tweets at the start
                tweets.tweets.unshift(tweet);
            }
        }

        var parseTweets = function (tweets) {
            return tweets.map(parseTweet);
        }

        var parseTweet = function (tweet) {
            //text exists on client side and originalText is on the server
            //this function will parse in both cases
            //object will have originalText and text (with anchors)
            var text;
            if (tweet.text) {
                text = tweet.text;
            }
            else {
                text = tweet.originalText;
            }
            tweet.originalText = text;
            var hashtagPattern = /#([a-zA-Z0-9]+)/gmi;
            var hashtags = text.match(hashtagPattern);
            tweet.hashtags = [];
            if (hashtags) {
                hashtags = hashtags.map(function (hashtag) { return hashtag.substr(1); });
                tweet.hashtags = hashtags;
            }
            var hashtagReplacement = "<a href=\"/hashtag/$1\">#$1</a>";
            text = text.replace(hashtagPattern, hashtagReplacement)

            var userPattern = /@([a-zA-Z0-9]+)/gi;
            var users = text.match(userPattern);
            tweet.users = [];
            if (users) {
                users = users.map(function (user) { return user.substr(1); });
                tweet.users = users;
            }
            var userReplacement = "<a href=\"/user/$1\">@$1</a>";
            text = text.replace(userPattern, userReplacement);
            tweet.text = text;
            return tweet;
        }

        return tweets;
    });


    app.directive('postTweet', function () {
        return {
            restrict: 'E',
            templateUrl: 'App/Components/Tweets/PostTweet.html',
            controller: function ($scope, Tweets, Account) {
                $scope.Account = Account;
                $scope.tweetLength = 140;
                $scope.tweet = '';
                $scope.postTweet = function () {
                    if (!$scope.tweet) {
                        return;
                    }
                    Tweets.postTweet({ text: $scope.tweet });
                    $scope.tweet = '';

                    $scope.$watch('tweet', function () {
                        if ($scope.tweet.length > $scope.tweetLength) {
                            $scope.tweet = $scope.tweet.substr(0, $scope.tweetLength);
                        }
                    });
                }
            }
        }
    });

    app.directive('tweets', function () {
        return {
            restrict: 'E',
            templateUrl: 'App/Components/Tweets/Tweets.html',
            scope: {},
            controller: function ($scope, $location, Tweets) {
                Tweets.onLocationChange();
                //$scope.$on('$locationChangeSuccess', function (p1, newUrl) {
                //    Tweets.onLocationChange(newUrl);
                //});
                $scope.tweetsSource = Tweets;
            }
        };
    });
})();