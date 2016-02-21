using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twissandnet.Models
{
    public class Response
    {
        public StatusCode status { get; set; }
        public object result { get; set; }

        public Response()
        {
            status = StatusCode.NoStatus;
        }

        public enum StatusCode
        {
            NoStatus = -1,
            OK = 1,
            //Account status codes
            UsernameTaken  = 10,
            UsernamePassCombinationNotFound = 11,
            UserLoggedIn=12,
            UserNotLoggedIn=13

        }
    }
    
    /*
    Status codes: 
    1 OK (true..)

    -Account codes
    10 - user already exists
    11 - username/password combination not found 
    */
}