
using Unity.Services.Lobbies.Models;

namespace GameFramework.Events
{
    public static class LobbyEvents
    {
        public delegate void LobbyUpdated(Lobby lobby);

        /// <summary>
        /// Events is invoked 
        /// </summary>
        public static LobbyUpdated OnLobbyUpdated;
    }
}
