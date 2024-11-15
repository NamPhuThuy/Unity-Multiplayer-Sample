using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Boostrap : MonoBehaviour
{
     
    async void Start()
    {
        //Used await to make sure the Unity Services is initialized before anyth
        await UnityServices.InitializeAsync();

        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            Debug.Log("Initial successfully");
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            
            
            //Can be sign in with Facebook, Apple, Google, Oculus, Steam,.. or Anonymous             
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            
            
            
            if (AuthenticationService.Instance.IsSignedIn)
            {
                string username = PlayerPrefs.GetString("Username", "Player");
                if (username == "")
                {
                    username = "Player";
                    PlayerPrefs.SetString("Username", "Player");
                }

                SceneManager.LoadSceneAsync("MainMenu");
            }
        }
    }

    private void OnSignedIn()
    {
        Debug.Log($"Signed in, token: {AuthenticationService.Instance.AccessToken}");
        Debug.Log($"Signed in, player id: {AuthenticationService.Instance.PlayerId}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
