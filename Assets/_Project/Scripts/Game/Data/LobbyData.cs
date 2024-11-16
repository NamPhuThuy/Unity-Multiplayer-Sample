

using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace Game.Data
{
    public class LobbyData
    {
        private int _mapIndex;
        private string _relayJoinCode;

        public int MapIndex
        {
            get => _mapIndex;
            set => _mapIndex = value;
        }
        
        
        public void Initialize(int mapIndex)
        {
            _mapIndex = mapIndex;
        }

        public void Initialize(Dictionary<string, DataObject> lobbyData)
        {
            UpdateState(lobbyData);
        }

        public void UpdateState(Dictionary<string, DataObject> lobbyData)
        {
            if (lobbyData.ContainsKey("MapIndex"))
            {
                _mapIndex = Int32.Parse(lobbyData["MapIndex"].Value);
            }

            if (lobbyData.ContainsKey("RelayJoinCode"))
            {
                _relayJoinCode = lobbyData["RelayJoinCode"].Value;
            }
        }

        public Dictionary<string, string> Serialize()
        {
            return new Dictionary<string, string>
            {
                {"MapIndex", _mapIndex.ToString()},
                {"RelayJoinCode", _relayJoinCode}
            };
        }

        public void SetRelayJoinCode(string code)
        {
            _relayJoinCode = code;
        }
    }
    
    
}