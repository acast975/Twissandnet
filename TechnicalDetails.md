# Twissandnet technical details
Here is the explanation for each part of the website:
- Cassandra
- ASP.NET Web Api
- AngularJS

# Cassandra
Cassandra was developed by Facebook to power their Inbox Search. It is based on Google BigTable and Amazon DynamoDB. Cassandra follows CAP Theorem and gives us A and P. 

CAP means:
- Consistency
- Availability
- Partition tolerance

Cassandra does not guarantee consistency - not all data will be up to date in every given moment. However, data will always be available (latest or some older version), and the database is partition tolerant - it will continue working even if a few nodes or whole datacenter is cut of from the rest of the system.

Cassandra does not support joining, complex queries and searching by any column as RDBMS do. Therefore, some level of denormalization might be needed to create the database model. By not supporting these operations, and by using partition key and clustering, Cassandra offers very fast writes and reads. 

Here is the explanation for DB create commands:
```
CREATE KEYSPACE twissandnet WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'};
```
This tells us to create a Keyspace named twissandnet. Keyspace is similar to Database in RDBMS. We also specify replication to use. Replication factor is 1, which means there will be only 1 place where the data is kept. Simple strategy is a strategy which will be used for replication. It means that once data is sent to the first node, determined by partitioner, it will also bi sent to the next nodes (until replication factor is met). 

Here is a bit more info on [Simple Strategy](https://docs.datastax.com/en/cassandra/2.0/cassandra/architecture/architectureDataDistributeReplication_c.html).

Next command is
```
USE twissandnet;
```
where we tell the database to use this keyspace for our next commands.

After that, we create Column Families (Tables as of Cassandra 3.0). Column Family is like a table in RDBMS. Primary key can also be composite. Partition key is the first (or the only) part of the Primary key. Partition key is used to calculate on which node the data will be saved. Cassandra uses Partitioner, which calculates Hash value of the Partition key, to get the node where the data will be saved. 

Earlier we mentioned that Cassandra does not allow searching by just any value, and that it is also very fast. That is because Cassandra calculates the Hash key, and knows exactly where to look for the data, on which node it can be if it exists.
```

CREATE TABLE users (
  username text PRIMARY KEY,
  password text
);

CREATE TABLE friends (
  username text PRIMARY KEY,
  friends set<text>
);
```

Next, we have table tweets, which is used to store tweet data. We store id (uuid), tweet text, hashtags, username of the poster, and timestamp. CQL Command looks like this:
```
CREATE TABLE tweets (
	id uuid,
	originaltext text,
	hashtags set<text>,
	timestamp timestamp,
	username text,
	PRIMARY KEY (id, timestamp)
) WITH CLUSTERING ORDER BY (timestamp DESC);              
```
Primary key here is composite, and it got id and timestamp. id is also the partition key here. timestamp is used for clustering order. Clustering order determines in what order will the data be written on the disk. To be able to achieve great speeds, we need to order the data by what we will read the most. In this case, we read all the data whenever we read, so clustering does not make impact. However, if we were to read only id and for example username, we would have decreased speed, because the data is not stored in that order. ```with clustering order by``` is not really neccessary here, since clustering order is already set using ```PRIMARY KEY(id, timestamp)```.

Hashtags set is used to store all the hashtags contained in a tweet. We need to store them to be able to search them. However, we did not set the table for searching using hashtags. Remember, Cassandra's speed is gained by knowing exactly what will be searched, and then hashing the searched values to find the location on the disk. Therefore, we will set up index on hashtags using this command:
```
CREATE INDEX tweet_hashtags    
ON tweets (hashtags);
```

And the last command to create a table we have is not really neccessary, but is used while testing and generally learning how the database works. 
```
CREATE TABLE user_tweets (
  username text,
  tweets set<uuid>,
  PRIMARY KEY (username)
);
```
This table is used to store tweet ids for each tweet user has posted. If we did not have this table however, we would need to add an index to username on ```tweets``` table to be able to search tweets by given user. 

This is the initial info you need to understand Cassandra. Here is a link to [Datastax Documentation](https://docs.datastax.com/en/cassandra/2.0/cassandra/gettingStartedCassandraIntro.html) to help you further.

[Wikipedia](https://en.wikipedia.org/wiki/Apache_Cassandra) is also a good start to read about Cassandra.

### ASP.NET Web Api
Asp.net Web Api is a great way to build api's for your app's. However, it is stateless, meaning, it stores no session. Normally, a key is used to identify the user who sends the request. Since this project is used for learning Cassandra, and not Asp.net Web Api or AngularJS, I have allowed myself to use some shortcuts. Instead of using keys to identify users, I have enabled sessions by changing Global.asax file. Check out ```Application_PostAuthorizeRequest``` method to see how session is enabled. Another thing to notice before we go into the code, on application error, if the error is ```404 not found``` we will return the index page to the user. This is done so when user requests something which is not found, we return the index page. This is useful for saved links. AngularJS will get the url requested, and do the work required. 

### Session Manager
This class found in Twissandnet.Cassandra namespace is used to create session. The session is used to connect to the database.

### Response model
As you might have noticed, there is a Response class in Twissandnet.Models namespace. This class is used to send back responses to the requesting user. I believe that when using API's there should be Status code, and data if status does not indicate errors. This way the servers sends only status codes and data, and the clients are free to use whatever error messages they choose to.

### SessionUtil
Class with all static methods, SessionUtil, is used to check if the user is logged in, to log user in, log out, and access his account data.

### Account class
This class is used to temporarily keep account username and password. It is used in AccountsController and SessionUtil.

### Tweet class
Another class used to temporarily store data. This class reflects the objects sent to client when requesting tweets.

### Account Controller
Account controller contains all functions used for:
 - Accounts
   - Login
   - Logout
   - Register
   - IsLoggedIn - gets user login status
 - Friends
   - GetFriends - gets all logged in user friends
   - GetUserFriends - get all friends for given username
   - AddFriend - adds a friend to the currently logged in user
   - RemoveFriend - removes a friend from the currently logged in user
   - UserExists - checks if given user exists

On the top of the controller, there are few prepared statements. Prepared statements should be used when the same query is being used multiple times with different parameters. In each function when there is a prepared statement used, it is first checked if the statement is null. If it is, it is then created, and used after that.
The same goes for prepared statements in TweetsController.

Note: In this example, password is stored without any modifications into the database. In real life scenario, you should always encrypt the password using one-way encryption and salt. Also, as mentioned, we are using session to keep track of our users. That is a bad practice with WebApi. A good choice here would be JWT.

### Tweets Controller
This is where all of the magic related to tweets happen. 
There are 
  - Public functions - exposed api functions
     - GetUserFeed - returns tweets from the friends of the currently logged in user and the user himself
     - GetPublicTweets - returns all tweets since all tweets are public
     - GetUserTweets - returns tweets of a given user, used when ```/user/:user``` url is requested
     - PostTweet - posts a new tweet as the currently logged in user
     - GetHashtagTweets - returns all tweets containing given hashtag
  - Private functions - functions used internally
    - GetTimestamp - converts given or current datetime to timestamp in milliseconds
    - LoadTweets - loads tweets using given prepared statement. All statements load the same data from the database when requesting tweets, so there is a single function when extracting the data. Follow DRY advice - Dont Repeat Yourself
    - GetUserTweetIds - returns all ids of tweets given user has posted
    - GetTweetsById - returns all tweets by given id

### [AngularJS](http://angularjs.org)
All of the code for the application is in App folder. Vendor folder contains scripts downloaded from the internet, and Components folder contains scripts and html used by our application. This is not a tutorial on angular therefore we will only go briefly through the app. Minimal knowledge of the framework, like data binding, is required to understand how it works. 

### App folder
App folder contains ```app.js``` and ```app.router.js```. 

```app.js``` file is used to initialize our AngularJS aplication. There we put the modules we need for our app. ```app.router.js``` file is used to route the urls. It specifies the url schemas, templates and controllers to use for every route in our app.

Url schemas we have define include 
  - /index
  - /friends
  - /public
  - /user/:usr, where the user is anything passed on, for example, in /user/slash url, usr is defined as 'slash'
  - /hashtag/:tag

Each of these specifies templateUrl, which is a link to the html markup to display when on these urls. ```/friends``` also specifies controller, which we will explain later.

If none of these url schemas are matched, we tell the router to redirect us to ```/index``` using this code:
```
.otherwise({ redirectTo: '/index' });
```

We also define ```StatusCodes``` factory here, which contains only status codes used in the apps that do not belong anywhere else. For example, Status Codes that can be returned from Accounts Controller are in ```Account``` factory. More info on that below.

### Account folder
Contains only one script, ```app.account.js```. This script defines factory ```Account``` which contains all the functions which interact with our API we have previously defined. There is also a controller named ```AccountNavbarCtrl``` which is used to control login/register forms and logout button.

```Account``` factory exposes functions to manage accounts:
  - isLoggedIn - returns boolean that indicates if the user is currently logged in
  - logIn - sends a request to the server to login the user. After recieving the response, we check if the Response Status Code is OK, which means that you are now logged in. Other message that can be recieved is that username/password combination is not correct. On successful login, we re-route the application to load the tweets for the current user
  - register - sends a request to the server. After that checks the response code and displays the message if there are any errors, same as login function
  - logOut - sends a request to the server to logout the current user. There is no data send to the server, and response is not awaited for, we assume this action will not fail on the server side. Does reloading the page to index
  - getUsername - returns username if user is logged in or an empty string if not
  
In this factory, not exposed, we have ```ACCOUNT_STATUS_CODES``` and ```ACCOUNT_STATUS_MESSAGES``` which define messages to display for each status code returned from the server. This way we do not depend on the server returned messages and can implement our own given the status codes. 

Last thing we do in this factory is, when the application is first loaded, we send a request to the server to check if the user is currently logged in. User will be logged in when comes on the site if the session on the server has not yet expired and the user was logged in on last visit. If the user is logged in, set the username to the returned value.

Note: Factories in AngularJS are singleton objects. This means only one instance of each object is created in the app's lifetime. The same object is passed on to each of the controllers, factories and other utilities we are using. Changing the object from one controller will have impact on other controllers. This is a wanted behaviour, and we are using it to share some variables, like the exposed API in this factory.

```AccountNavbarCtrl``` controller only uses ```Account``` factory to login, register and logout users. Its functions are basically just wrappers around Account factory's functions.

There is also a watch set up to monitor any changes made on username fields for login and register forms. Whenever any of those two change, we set the value to not contain any special characters. Ofcourse, validation should also be done on server.

### Friends folder
This folder contains html file which renders view on ```/friends``` url, and the ```app.friends.js``` file contains functions to search for users, add them and remove them as friends. 

Friends.html file is nothing else but html markup code that will be rendered. One thing to take care of here is that form has ```ng-submit="search()"``` which tells AngularJS that instead of submiting the form, it should execute search function. The same goes for ```ng-click="buttonClick()"```. The search and button click functions are defined in ```FriendsCtrl```. How does AngularJS know which controller to use? Well, it is specified in the ```app.router.js``` file, with this code: 
```
[code]
.when('/friends', {
    templateUrl: '/App/Components/Friends/friends.html',
    controller: 'FriendsCtrl'
})
[more code]
```

In ```FriendsCtrl``` controller, we first require Account, StatusCodes, $http and $scope to be injected. Account we will get here is the same account we have in any other controller. It is singleton. Same goes for StatusCodes where we have defined only a few codes, which are not used locally (like Account status codes).

Controllers are not singletons. This controller is loaded each time /friends url is visited. And each time it is loaded, it queries the server to fetch your friends. This is done using this code:
```
$http.get('/api/Accounts/GetFriends')
    .then(function (event) {
        if(event.data.status == StatusCodes.CODES.OK)
            $scope.friends = event.data.result;
    });
```

We defined functions search and buttonClick which are used on form submit and button click. The search function takes your input, which is binded to ```$scope.search.query```, checks if you already have a friend with given name in the list, and if you do displays message and shows button to remove the friend. It also sets the button action which will be used on button click. If the user you search for is not found, it then checks if the username given is not the same as yours. And lastly, if it is not, a request will be sent to the server to check if the user exists. Upon returning the result, a message is displayed and button shown to add user as a friend if he is registered.

The buttonClick function adds or removes friend you have last searched for. What the button does depends on what action is set after searching for the user.

### Index folder
There is only an html file which defines the index view. It uses ```tweets``` directive to display tweets, and ```post-tweet``` directive to display form for posting new tweets. 

Directive is nothing else but shortened, custom defined html element. These elements are defined somewhere else. In this case, both directives we use here are defined in Tweets folder.

### Tweets folder
This folder contains most of the code. ```PostTweet.html``` is the html to render ```post-tweet``` directive. It is hooked up with the directive code in ```app.tweets.js``` file.  The directive code here is only a wrapper to use ```Tweets``` factory (defined below), to post new tweets to the server.

```Tweets.html``` is the html to render tweets returned from the server, in place of ```<tweets></tweets>``` custom html elements. In this template, we specify to order the tweets by timestamp so they are always in correct order, from the newest to the oldest. The directive code is found in ```app.tweets.js``` file. The code here triggers ```Tweets.onLocationChange();``` function every time the directive is displayed. More info on that function comes later.

And lastly, the file ```app.tweets.js```, which contains the following functions:
  - parseTweet - parses tweet and adds hashtags and users to the tweet data. Also parses links to the users so you can type @user and get a link.
  - addTweet - adds tweet to the local array of tweets, only if the tweet should be displayed. For example, if you are looking at tweets with hashtag '#tag1', and the posted tweet does not contain that hashtag, it will not be added
  - postTweet - sends the tweet to the server to be permanently stored
  - implements onLocationChange function, which monitors the url and requests tweets when the url is changed. Whenever the url is changed, we look if it is equal to ```/public```, ```/index```, ```/user/:usr``` or ```/hashtag/:tag```. Based on what the url starts with, we send a request to the server to fetch data. For example, if the url is ```/hashtag/tag1```, we send a request to ```/api/Tweets/GetHashtagTweets``` and add tag1 to the params to search for on the server side. When the request is finished, if the status code is OK, we parse the data and put it in ```tweets``` variable. Our previously defined ```<tweets></tweets>``` directive reads from this variable and displays the data we see.
  
That is the whole application described. Once again, the point of the application is to learn and use Cassandra, not Asp.net Web Api or AngularJS. That is why we only briefly described the last two.

### Homework
After you have read the code, there is some homework for you to be done, to get a better understanding of the app and practice a bit. 
Try this:
  - Change methods on the server to include start timestamp and limit integer, so when you query the data, you don't fetch all of it in a single request. Imagine what would happen if Twitter would show you the whole database they have.
  - Cache results in AngularJS, and utilize the previous function to get only new data. Also, use caching to store tweets for different hashtags, users..
  - Change the database design. You can omit ````user_tweets table```, but that would also require some code changes
  
Feel free to use this code, with respect to the [MIT License](https://opensource.org/licenses/MIT). Have fun coding and hacking!
