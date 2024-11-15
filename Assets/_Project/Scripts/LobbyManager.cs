using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : Singleton<LobbyManager>
{
    private Lobby _lobby;
    private Coroutine _heartBeatCoroutine;
    private Coroutine _refreshLobbyCoroutine;
    
    //GETTERS
    public string GetLobbyCode()
    {
        return _lobby?.LobbyCode;
    }
    
    
    public async Task<bool> CreateLobby(int maxPlayers, bool isPrivate, Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = SerializePlayerData(data);
        
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, playerData);
        
        
        CreateLobbyOptions options = new CreateLobbyOptions()
        {
            IsPrivate = isPrivate,
            Player = player
        };

        try
        {
            _lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", maxPlayers, options);
        }
        catch (System.Exception)
        {
            return false;
        }

        _heartBeatCoroutine = StartCoroutine(HeartBeatLobbyCoroutine(_lobby.Id, 6f));
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        
        Debug.Log($"Lobby created with lobby id: {_lobby.Id}");
        Debug.Log($"Lobby code: {_lobby.LobbyCode}");
        return true;
    }

    private IEnumerator HeartBeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        while (true)
        {
            Debug.Log($"HeartBeat: ");
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }
    
    private IEnumerator RefreshLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        while (true)
        {
            Task<Lobby> task = LobbyService.Instance.GetLobbyAsync(lobbyId);
            yield return new WaitUntil(() => task.IsCompleted); //wait until the task (refresh Lobby) is complete

            Lobby newLobby = task.Result;
            //Check if the "newLobby" is actually new
            if (newLobby.LastUpdated > _lobby.LastUpdated)
            {
                _lobby = newLobby;
            }
            
            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }

    private Dictionary<string, PlayerDataObject> SerializePlayerData(Dictionary<string, string> data)
    {
        Dictionary<string, PlayerDataObject> playerData = new Dictionary<string, PlayerDataObject>();

        foreach (var (key, value) in data)
        {
            playerData.Add(key, new PlayerDataObject(
                visibility: PlayerDataObject.VisibilityOptions.Member, //Visible only to member of the lobby
                value: value));
        }

        return playerData;
    }

    public void OnApplicationQuit()
    {
        //When the host quit application => the lobby is deleted
        if (_lobby != null && _lobby.HostId == AuthenticationService.Instance.PlayerId)
        {
            LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
        }
    }


    public async Task<bool> JoinLobby(string code, Dictionary<string, string> playerData)
    {

        JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
        Player player = new Player(AuthenticationService.Instance.PlayerId, null, SerializePlayerData(playerData));

        options.Player = player;
        try
        {
            _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, options);
        }
        catch (System.Exception)
        {
            return false;
        }
        
        _refreshLobbyCoroutine = StartCoroutine(RefreshLobbyCoroutine(_lobby.Id, 1f));
        return true;
    }
}
