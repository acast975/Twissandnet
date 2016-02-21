using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twissandnet.Cassandra
{
    public class SessionManager
    {
        private static ISession session;
        private static object _lock = new object();
        public static ISession GetSession()
        {
            if (session == null)
            {
                lock (_lock)
                {
                    if (session == null)
                    {
                        Cluster cluster = Cluster.Builder().AddContactPoint("localhost").Build();
                        session = cluster.Connect("twissandnet");
                    }
                }
                
            }

            return session;
        }
    }
}