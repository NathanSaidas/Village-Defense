namespace Gem
{
    namespace Networking
    {
        public enum RequestType
        {
            //General Network Requests
            Authentication,
            AvailableServers,
            AvailableAuthenticationServers,
            ConnectionList,
            ConnectionAuthentication,
            ConnectionGame,
            Kick,
            Disconnect,

            //Main menu
            SelectHero,
            StartGame,

            //Gameplay
            Spawn,
            Ownership,
            Destroy,
            IssueAbility
        }
    }
}