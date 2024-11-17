using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Game;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _turnSpeed = 10f;
    [SerializeField] private Vector2 _minMaxRotationX;
    [SerializeField] private Transform _camTransform;

    private CharacterController _cc;
    private PlayerControl _playerControl;
    private float _cameraAngle;

    private void Reset()
    {
        _speed = 10f;
        _turnSpeed = 10f;
        _minMaxRotationX = new Vector2(-90f, 90f);
    }

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
        if (IsLocalPlayer)
        {
            if (_playerControl.Player.Move.inProgress)
            {
                Vector2 movementInput = _playerControl.Player.Move.ReadValue<Vector2>();
                Vector3 movement = movementInput.x * _camTransform.right + movementInput.y * _camTransform.forward;

                movement.y = 0;

                _cc.Move(movement * (_speed * Time.deltaTime));
            }

            if (_playerControl.Player.Look.inProgress)
            {
                Vector2 lookInput = _playerControl.Player.Look.ReadValue<Vector2>();
                transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * Time.deltaTime);

                RotateCamera(lookInput.y);
            }
        }
        
    }

    private void RotateCamera(float lookInputY)
    {
        _cameraAngle = Vector3.SignedAngle(transform.forward, _camTransform.forward, _camTransform.right);

        float cameraRotationAmount = lookInputY * _turnSpeed * Time.deltaTime;
        float newCameraAngle = _cameraAngle - cameraRotationAmount;

        if (newCameraAngle <= _minMaxRotationX.x && newCameraAngle >= _minMaxRotationX.y)
        {
            _camTransform.RotateAround(_camTransform.position, _camTransform.right, -lookInputY * _turnSpeed * Time.deltaTime);
        }
    }
}
