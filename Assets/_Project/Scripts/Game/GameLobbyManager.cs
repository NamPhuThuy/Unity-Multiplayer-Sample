
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

using Unity.Services.Lobbies.Models;
using Game.Data;
using GameFramework.Core;
using GameFramework.Events;
using GameFramework.Manager;
using UnityEngine;

namespace Game
{
    public class GameLobbyManager : Singleton<GameLobbyManager>
    {
        /// <summary>
        /// Store all datas of players in the lobby
        /// </summary>
        private List<LobbyPlayerData> _lobbyPlayerDatas = new List<LobbyPlayerData>();

        private LobbyPlayerData _localLobbyPlayerData;

        [Header("Map Selection")] 
        private LobbyData _lobbyData;

        public bool IsHost => _localLobbyPlayerData.Id == LobbyManager.Instance.GetHostId();

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

            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");
            
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(0);

            bool succeeded = await LobbyManager.Instance.CreateLobby(4, true, _localLobbyPlayerData.Serialize(), _lobbyData.Serialize());
            return succeeded;
        }

        public async Task<bool> JoinLobby(string code)
        {
            /*Dictionary<string, string> playerData = new Dictionary<string, string>()
        {
            {"GamerTag", "JoinPlayer"}
        };*/

            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "JoinPlayer");

            bool succeeded = await LobbyManager.Instance.JoinLobby(code, _localLobbyPlayerData.Serialize());
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

                if (lobbyPlayerData.Id == AuthenticationService.Instance.PlayerId)
                {
                    _localLobbyPlayerData = lobbyPlayerData;

                }

                _lobbyPlayerDatas.Add(lobbyPlayerData);
            }

            //Every time there's a new update in the lobby, we reset the data to what in the lobby.Data
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(lobby.Data);
            
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
            return await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id,
                _localLobbyPlayerData.Serialize());
        }

        public int GetMapIndex()
        {
            return _lobbyData.MapIndex;
        }

        public async Task<bool> SetSelectedMap(int currentMapIndex)
        {
            _lobbyData.MapIndex = currentMapIndex;
            return await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());
        }
    }
}
