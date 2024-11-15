
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

using Unity.Services.Lobbies.Models;
using Game.Data;
using GameFramework.Core;
using GameFramework.Events;
using GameFramework.Manager;

namespace Game
{
    public class GameLobbyManager : Singleton<GameLobbyManager>
    {
        /// <summary>
        /// Store all datas of players in the lobby
        /// </summary>
        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += OnLobbyUpdated;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= OnLobbyUpdated;
        }

        public string GetLobbyCode()
        {
            return LobbyManager.Instance.GetLobbyCode();
        }

        public async Task<bool> CreateLobby()
        {
            /*Dictionary<string, string> playerData = new Dictionary<string, string>()
            {
                {"GamerTag", "HostPlayer"}
            };*/

            LobbyPlayerData playerData = new LobbyPlayerData();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");

            bool succeeded = await LobbyManager.Instance.CreateLobby(4, true, playerData.Serialize());
            return succeeded;
        }

        public async Task<bool> JoinLobby(string code)
        {
            /*Dictionary<string, string> playerData = new Dictionary<string, string>()
        {
            {"GamerTag", "JoinPlayer"}
        };*/

            LobbyPlayerData playerData = new LobbyPlayerData();
            playerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");

            bool succeeded = await LobbyManager.Instance.JoinLobby(code, playerData.Serialize());
            return succeeded;
        }

        private void OnLobbyUpdated(Lobby lobby)
        {
            //Ask Lobby Manager to give us the data of the player we want 
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayerData();
            _lobbyPlayerDatas.Clear();

            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.ID == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;

                }

                _lobbyPlayerDatas.Add(lobbyPlayerData);
            }
            
            //Check if is anyone is subcribe?  
            //Used to spawn players that have been subcribed to this event
            Events.LobbyEvents.OnLobbyUpdated?.Invoke();
        }

        public List<LobbyPlayerData> GetPlayers()
        {
            return _lobbyPlayerDatas;
        }

        public async Task<bool> SetPlayerReady()
        {
            _localLobbyPlayerData.IsReady = true;
            return await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.ID,
                _localLobbyPlayerData.Serialize());
        }
    }
}
