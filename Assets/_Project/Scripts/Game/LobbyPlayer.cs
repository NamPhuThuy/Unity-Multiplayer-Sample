using System;
using Game.Data;
using TMPro;
using UnityEngine;

namespace Game
{
    public class LobbyPlayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _playerName;
        [SerializeField] private SpriteRenderer _isReadyIndicator;


        private MaterialPropertyBlock _propertyBlock;
        private LobbyPlayerData _data;


        private void Start()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Called whenever we receive a new player
        /// </summary>
        public void SetData(LobbyPlayerData data)
        {
            _data = data;
            _playerName.text = _data.GamerTag;

            if (_data.IsReady)
            {
                if (_isReadyIndicator != null)
                {
                    /*_isReadyIndicator.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor("_BaseColor", Color.green);
                    _isReadyIndicator.SetPropertyBlock(_propertyBlock);*/
                    
                    _isReadyIndicator.color = Color.white;
                }
            }
            
            gameObject.SetActive(true);
        }
        
    }
}