namespace GraphApiTestBot.State
{
    public class TokenState
    {
        public TokenState(string accessToken = null)
        {
            AccessToken = accessToken;
        }

        public string AccessToken { get; set; }
    }
}
