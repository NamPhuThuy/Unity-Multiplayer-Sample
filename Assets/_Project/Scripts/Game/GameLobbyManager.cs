
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;

using Unity.Services.Lobbies.Models;
using Game.Data;
using GameFramework.Core;
using GameFramework.Events;
using GameFramework.Manager;
using GameFramwork.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [Header("Relay")]
        [SerializeField] private int _maxNumPlayers = 4;
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
            //Initialize infor of the Host
            _localLobbyPlayerData = new LobbyPlayerData();
            _localLobbyPlayerData.Initialize(AuthenticationService.Instance.PlayerId, "HostPlayer");
            
            //Initialize infor of the Lobby
            _lobbyData = new LobbyData();
            _lobbyData.Initialize(0);

            bool succeeded = await LobbyManager.Instance.CreateLobby(_maxNumPlayers, true, _localLobbyPlayerData.Serialize(), _lobbyData.Serialize());
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

        private async void OnLobbyUpdated(Lobby lobby)
        {
            //Ask Lobby Manager to give us the data of the player we want 
            List<Dictionary<string, PlayerDataObject>> playerData = LobbyManager.Instance.GetPlayerData();
            _lobbyPlayerDatas.Clear();

            int numOfPlayerReady = 0;
            
            //Loop through all players in current Lobby
            foreach (Dictionary<string, PlayerDataObject> data in playerData)
            {
                LobbyPlayerData lobbyPlayerData = new LobbyPlayerData();
                lobbyPlayerData.Initialize(data);

                if (lobbyPlayerData.IsReady)
                {
                    numOfPlayerReady++;
                }
                
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

            //If everyone is ready
            if (numOfPlayerReady == lobby.Players.Count)
            {
                Events.LobbyEvents.OnLobbyReady?.Invoke();
            }

            if (_lobbyData.RelayJoinCode != default)
            {
                //Join the Relay
                await JoinRelayServer(_lobbyData.RelayJoinCode);
                
                Debug.Log($"Scene name: {_lobbyData.SceneName}");
                SceneManager.LoadSceneAsync(_lobbyData.SceneName);
            }
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

        public async Task<bool> SetSelectedMap(int currentMapIndex, string sceneName)
        {
            _lobbyData.MapIndex = currentMapIndex;
            _lobbyData.SceneName = sceneName;
            return await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());
        }

        public async Task StartGame()
        {
            //Retrieve the unique join code of Relay connection
            string relayJoinCode = await RelayManager.Instance.CreateRelay(_maxNumPlayers);
            _lobbyData.RelayJoinCode = relayJoinCode;
            
            //Update the lobbyData
            await LobbyManager.Instance.UpdateLobbyData(_lobbyData.Serialize());

            //Retrieve the allocationId and connectionData
            string allocationId = RelayManager.Instance.GetAllocationId();
            string connectionData = RelayManager.Instance.GetConnectionData();
            
            //Update player data
            await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);

            SceneManager.LoadSceneAsync(_lobbyData.SceneName);
        }
        
        private async Task<bool> JoinRelayServer(string relayJoinCode)
        {
            await RelayManager.Instance.JoinRelay(relayJoinCode);
            
            string allocationId = RelayManager.Instance.GetAllocationId();
            string connectionData = RelayManager.Instance.GetConnectionData();
            await LobbyManager.Instance.UpdatePlayerData(_localLobbyPlayerData.Id, _localLobbyPlayerData.Serialize(), allocationId, connectionData);
            
            return true;
        }
    }
}
