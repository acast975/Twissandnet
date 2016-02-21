using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Twissandnet.Models;
using Twissandnet.Cassandra;
using Cassandra;
using System.Threading.Tasks;

namespace Twissandnet.Controllers
{
    public class AccountsController : ApiController
    {
        ISession session = SessionManager.GetSession();
        static PreparedStatement
                    insertUser = null,
                    loginUser = null,
                    getFriends = null,
                    userExists = null,
                    addFriend = null,
                    removeFriend = null;

        [HttpPost]
        public async Task<Response> Register([System.Web.Mvc.Bind]Account account)
        {
            Response resp = new Response();
            Response userExists = await UserExists(account);
            if ((bool)userExists.result)
            {
                resp.status = Response.StatusCode.UsernameTaken;
                return resp;
            }

            if (insertUser == null)
            {
                insertUser = await session.PrepareAsync("insert into users (username, password)  values (?,?) if not exists");
            }
            var stmt = insertUser.Bind(account.username, account.password);
            var result = await session.ExecuteAsync(stmt);

            SessionUtil.LogIn(account);

            resp.status = Response.StatusCode.OK;
            return resp;
        }

        [HttpPost]
        public async Task<Response> LogIn([System.Web.Mvc.Bind]Account account)
        {
            if (loginUser == null)
            {
                loginUser = await session.PrepareAsync("select * from users where username = ? ");
            }
            var stmt = loginUser.Bind(account.username);
            var result = await session.ExecuteAsync(stmt);
            var row = result.FirstOrDefault();
            if (row != null && row["password"].ToString() == account.password)
            {
                SessionUtil.LogIn(account);
                return new Response() { status = Response.StatusCode.OK };
            }

            return new Response()
            {
                status = Response.StatusCode.UsernamePassCombinationNotFound
            };
        }

        [HttpGet]
        public Response LogOut()
        {
            SessionUtil.LogOut();
            return new Response() { status = Response.StatusCode.OK};
        }

        [HttpGet]
        public Response IsLoggedIn()
        {

            Response resp = new Response();

            if (SessionUtil.IsLoggedIn())
            {
                resp.status = Response.StatusCode.UserLoggedIn;
                resp.result= SessionUtil.GetAccount().username;
            }
            else
            {
                resp.status = Response.StatusCode.UserNotLoggedIn;
            }
            return resp;
        }

        [HttpGet]
        public async Task<Response> GetFriends()
        {
            Response resp = new Response();
            if (SessionUtil.IsLoggedIn())
            {
                resp = await GetUserFriends(SessionUtil.GetAccount().username);
            }
            else
            {
                resp.status = Response.StatusCode.UserNotLoggedIn;
            }
            return resp;
        }

        public static async Task<Response> GetUserFriends(string username)
        {
            if (!SessionUtil.IsLoggedIn())
            {
                return new Response() { status = Response.StatusCode.UserNotLoggedIn };
            }
            var session = SessionManager.GetSession();
            if (getFriends == null)
            {
                getFriends = await session.PrepareAsync("select friends from friends where username = ?");
            }
            var stmt = getFriends.Bind(username);
            var result = await session.ExecuteAsync(stmt);
            Row row = result.FirstOrDefault();
            Response resp = new Response() { status = Response.StatusCode.OK };
            if (row != null)
            {
                resp.result = ((IEnumerable<string>)row["friends"]).ToList();
            }
            else
            {
                resp.result = new List<string>();
            }

            return resp;
        }

        [HttpPost]
        public async Task<Response> UserExists([System.Web.Mvc.Bind]Account user)
        {
            if (userExists == null)
            {
                userExists = await session.PrepareAsync("select * from users where username = ?");
            }
            var stmt = userExists.Bind(user.username);
            var result = await session.ExecuteAsync(stmt);
            Row row = result.FirstOrDefault();
            return new Response()
            {
                status = Response.StatusCode.OK,
                result = row != null
            };
        }

        [HttpPost]
        public async Task<Response> AddFriend([System.Web.Mvc.Bind]Account user)
        {
            //we believe user does exist, since the add button was displayed on the client side
            //and user is loggedIn
            if (addFriend == null)
            {
                addFriend = await session.PrepareAsync("UPDATE friends SET friends = friends + ? WHERE username = ?");
            }
            if (!SessionUtil.IsLoggedIn())
            {
                return new Response()
                {
                    status = Response.StatusCode.UserNotLoggedIn
                };
            }

            var stmt = addFriend.Bind(new List<string>() { user.username }, SessionUtil.GetAccount().username);
            await session.ExecuteAsync(stmt);
            return new Response() { status = Response.StatusCode.OK };
        }

        [HttpPost]
        public async Task<Response> RemoveFriend([System.Web.Mvc.Bind]Account user)
        {
            if (removeFriend == null)
            {
                removeFriend = await session.PrepareAsync("UPDATE friends SET friends = friends - ? WHERE username = ?");
            }
            if (!SessionUtil.IsLoggedIn())
            {
                return new Response() { status = Response.StatusCode.UserNotLoggedIn };
            }
            var stmt = removeFriend.Bind(new List<string>() { user.username }, SessionUtil.GetAccount().username);
            await session.ExecuteAsync(stmt);
            return new Response() { status = Response.StatusCode.OK };
        }

    }
}
