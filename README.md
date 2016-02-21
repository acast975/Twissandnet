# Twissandnet
Twissandnet is an example project, created to learn and demonstrate use of Cassandra. The functionallity of this project is somewhat similar to twitter. This project is realized using ASP.NET Web Api, [AngularJS], Cassandra and Bootstrap. Idea is taken from [Twissandra](https://github.com/twissandra/twissandra/).

# Getting started
Cassandra is using Java. Set JAVA_HOME system variable before continuing with this tutorial. 
To get this project working, follow these steps:
  - Download and install [Datastax Cassandra](https://academy.datastax.com/downloads)
  - Set up the database
  - Download the project
  - Start the project

### Downloading and installing Cassandra
Go to the website https://academy.datastax.com/downloads and download Cassandra. Install it and install DevCenter. You might want to enable Cassandra as a service if you want to do active development on it. If not, you can start it from (assuming you have it installed in the default directory)
```
C:\Program Files\DataStax-DDC\apache-cassandra\bin\cassandra.bat
```
Wait a bit to start the database and then proceed to set up the database.
### Setting up the database
Start the DevCenter you have just installed. Once it is opened, create new connection to localhost using:
- Contact host: localhost
- Port: 9042 (the default port)

Click 'Try to establish a connection' to check if the connection and the DB is working.
After setting up the connection, copy contents of [CQL Commands File](Twissandnet/CQL Commands.txt) to DevCenter, and click Execute CQL Script, or press ALT+F11.
You should now have the database all set up.

### Download the project
To download the project, you need to have git installed. Open Command Prompt, go to the directory you want to save the project and type this:
```
git clone https://github.com/PavlovicDzFilip/Twissandnet.git
```
The files will now download in your directory.
### Starting the project
Once you have downloaded the project, go to Twissandnet directory and open the solution. I have developed this using Visual Studio 2015 Community Edition. Once the project is opened, hit F5 to start the website. On your first run, VS will download NuGet packages and install them - this might take a few minutes.
Twissandnet is now running!

## Note
To read the technical details, open [Technical Details](TechnicalDetails.md) readme file. 
There I have some info on how the database and the rest of the system works.

License
----

MIT
   [AngularJS]: <http://angularjs.org>
