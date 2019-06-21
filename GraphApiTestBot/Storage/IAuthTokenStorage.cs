// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

namespace GraphApiTestBot.Storage
{
    public interface IAuthTokenStorage
    {
        ConversationAuthToken LoadConfiguration(string id);

        void SaveConfiguration(ConversationAuthToken state);
    }
}