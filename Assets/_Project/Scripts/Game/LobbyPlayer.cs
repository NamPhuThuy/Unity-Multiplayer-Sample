using Game.Data;
using TMPro;
using UnityEngine;

namespace Game
{
    public class LobbyPlayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName;
        private LobbyPlayerData _data;
        
        /// <summary>
        /// Called whenever we receive a new player
        /// </summary>
        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.GamerTag;
            gameObject.SetActive(true);
        }
        
    }
}