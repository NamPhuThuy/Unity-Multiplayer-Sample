using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Game;
using GameFramework.Network.Component;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    /*[SerializeField] private float _speed;
    [SerializeField] private float _turnSpeed;*/
    [SerializeField] private Vector2 _minMaxRotationX = new Vector2(90f, -90f);
    [SerializeField] private Transform _camTransform;
    [SerializeField] private NetworkMovementComponent _playerMovement;

    private CharacterController _cc;
    private PlayerControl _playerControl;
    private float _cameraAngle;

    public override void OnNetworkSpawn()
    {
        CinemachineVirtualCamera cvm = _camTransform.gameObject.GetComponent<CinemachineVirtualCamera>();

        /*
         - Ensures that the Cinemachine Virtual Camera associated with _camTransform has a high priority (1) only if the current player is its owner. If the player is not the owner, the camera's priority is set to 0
          
        - If this player die (the camera is destroy), they will see what other team-mate see. Because, other camera in the scene is set Priority to '0' (lower than the owner camera)
         */
        if (IsOwner) 
        {
            cvm.Priority = 1;
        }
        else
        {
            cvm.Priority = 0;
        }
        
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _playerControl = new PlayerControl();
        _playerControl.Enable();

        // Hides the mouse cursor and confines its movement to the game window, making it suitable for games that require direct control of the camera or player using mouse input (like FPS or flight simulators)
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //--Read input
        Vector2 movementInput = _playerControl.Player.Move.ReadValue<Vector2>();
        Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();
        
        /*if (IsLocalPlayer)
        {
            if (IsServer)
            {
                /*
                 (IsServer && IsLocalPlayer) is true if: 
                - The game is using a host-client networking model (not a dedicated server).
                - The script is running on the host client (which is also acting as the server).
                - The script is attached to the game object representing the host player.
                #1#
                
                Move(movementInput);
                RotatePlayer(lookInput);
                RotateCamera(lookInput);
            }
            else
            {
                RotateCamera(lookInput);
                MoveServerRpc(movementInput, lookInput);
            }
        }*/

        if (IsClient && IsLocalPlayer)
        {
            _playerMovement.ProcessLocalPlayerMovement(movementInput, lookInput);
        }
        else
        {
            _playerMovement.ProcessSimulatedPlayerMovement();
        }
    }

    

    /*private void Move(Vector2 movementInput)
    {
        Vector3 movement = movementInput.x * _camTransform.right + movementInput.y * _camTransform.forward;

        //Make sure the player does not move up and down (only in OXZ plane)
        movement.y = 0;

        _cc.Move(movement * (_speed * Time.deltaTime));
    }
    
    private void RotatePlayer(Vector2 lookInput)
    {
        transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * Time.deltaTime);
    }*/

    /*private void RotateCamera(Vector2 lookInput)
    {
        _cameraAngle = Vector3.SignedAngle(transform.forward, _camTransform.forward, _camTransform.right);

        float cameraRotationAmount = lookInput.y * _turnSpeed * Time.deltaTime;
        float newCameraAngle = _cameraAngle - cameraRotationAmount;

        if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
        {
            _camTransform.RotateAround(_camTransform.position, _camTransform.right, -lookInput.y * _turnSpeed * Time.deltaTime);
        }
    }*/

    //This function is called on the client-side but is executed on the server-side
    /*[ServerRpc]
    private void MoveServerRpc(Vector2 movementInput, Vector2 lookInput)
    {
        Move(movementInput);
        RotatePlayer(lookInput);
    }*/
}
