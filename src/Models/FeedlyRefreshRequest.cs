namespace FeedlyOpmlExport.Functions
{
    public class FeedlyRefreshRequest
    {
        private const string FEEDLY_CLIENT_ID = "feedlydev"; // hard-coded for users with Pro accounts

        // ReSharper disable UnusedMember.Global
        // ReSharper disable InconsistentNaming

        // ReSharper disable once MemberCanBePrivate.Global -- used in the http request
        // ReSharper disable once UnusedAutoPropertyAccessor.Global -- used in http request
        public string refresh_token { get; }
        public string client_id =>  FEEDLY_CLIENT_ID;
        public string client_secret => FEEDLY_CLIENT_ID;
        public string grant_type => "refresh_token";
        // ReSharper restore UnusedMember.Global
        // ReSharper restore InconsistentNaming

        public FeedlyRefreshRequest(string refreshToken)
        {
            refresh_token = refreshToken;
        }        
    }
}