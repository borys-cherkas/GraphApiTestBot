// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using System;

namespace GraphApiTestBot.Storage
{
    public class ConversationAuthToken
    {
        public ConversationAuthToken(string id)
        {
            this.Id = id;
        }

        public string Id { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime ExpiresIn { get; set; }
    }
}