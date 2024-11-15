using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _lobbyCodeText;
        [SerializeField] private Button _readyButton;


        private void OnEnable()
        {
            _readyButton.onClick.AddListener(OnReadyPressed);
        }

        private void OnDisable()
        {
            _readyButton.onClick.RemoveListener(OnReadyPressed);
            // _readyButton.onClick.RemoveAllListeners();
        }

        void Start()
        {
            _lobbyCodeText.text = $"Lobby code: {GameLobbyManager.Instance.GetLobbyCode()}";
        }


        private async void OnReadyPressed()
        {
            bool succeeded = await GameLobbyManager.Instance.SetPlayerReady();
            if (succeeded)
            {
                _readyButton.gameObject.SetActive(false);
            }
            
        }
        

        // Update is called once per frame
        void Update()
        {

        }
    }
}
