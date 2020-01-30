using System;
using System.Collections.Generic;

namespace ChattyHub
{
    public class Users : IUsers
    {
        private readonly HashSet<string> _userNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public bool SignIn(string userName)
        {
            lock (_sync)
            {
                return _userNames.Add(userName);
            }
        }
    }
}