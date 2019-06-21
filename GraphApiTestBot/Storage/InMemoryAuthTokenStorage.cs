// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System.Collections.Generic;

namespace GraphApiTestBot.Storage
{
    public class InMemoryAuthTokenStorage : IAuthTokenStorage
    {
        private static readonly Dictionary<string, ConversationAuthToken> InMemoryDictionary = 
            new Dictionary<string, ConversationAuthToken>();

        public ConversationAuthToken LoadConfiguration(string id)
        {
            if (InMemoryDictionary.ContainsKey(id))
                return InMemoryDictionary[id];
            return (ConversationAuthToken)null;
        }

        public void SaveConfiguration(ConversationAuthToken state)
        {
            InMemoryDictionary[state.Id] = state;
        }
    }
}