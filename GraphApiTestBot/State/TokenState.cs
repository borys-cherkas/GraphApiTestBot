using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphApiTestBot.State
{
    public class TokenState
    {
        public TokenState()
        {
            
        }

        public TokenState(string accessToken)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }
    }
}
