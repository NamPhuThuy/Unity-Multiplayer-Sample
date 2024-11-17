using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

namespace GameFramework.Network.Component
{
    /// <summary>
    /// This class is used to implement Client prediction technique
    /// </summary>
    public class NetworkMovementComponent : NetworkBehaviour
    {
        [SerializeField] private CharacterController _cc;
        
        [Header("Some vars")]
        [SerializeField] private float _speed = 10f;
        [SerializeField] private float _turnSpeed = 10f;
        [SerializeField] private Transform _camSocket;
        [SerializeField] private GameObject _vcam;

        private Transform _vcamTransform;
        
        /// <summary>
        /// a "tick" refers to a single iteration or step of the game's main loop on the server. It's the fundamental unit of time in which the game's state is updated and simulated. Think of it as a single frame, but specifically on the server side, responsible for managing the game's logic and consistency across all clients.
        /// </summary>
        private int _tick = 0;
        private float _tickRate = 1f / 60;
        private float _tickDeltaTime = 0f; //how much time elapsed since the last tick

        private const int BUFFER_SIZE = 1024; //size of a buffer used for network communication
        //The maximum number of Input client can keep = BUFFER_SIZE
        private InputState[] _inputStates = new InputState[BUFFER_SIZE];
        private TransformState[] _transformStates = new TransformState[BUFFER_SIZE];

        //The latest value of TransfromState that the Server manage (the latest transform on the Server)
        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();
        public TransformState _previousTransformState;

        private void OnEnable()
        {
            //Listen to the variable-change
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _vcamTransform = _vcam.transform;
        }

        private void OnServerStateChanged(TransformState previousvalue, TransformState newvalue)
        {
            _previousTransformState = previousvalue;
        }

        public void ProcessLocalPlayerMovement(Vector2 movementInput, Vector2 lookInput)
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                //Process the Input
                int bufferIndex = _tick % BUFFER_SIZE;
                
                /*IsClient is true on any client, including the host in a host-client setup.
                 !IsServer (not IsServer) is true on all clients except the host in a host-client setup.
                  
                - In a dedicated server setup, they are functionally equivalent.*/


                if (!IsServer)
                {
                    //Send to the server the information of how we move 
                    MovePlayerServerRpc(_tick, movementInput, lookInput);
                    
                    //Then, we move it immediately
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);
                }
                else
                {
                    MovePlayer(movementInput);
                    RotatePlayer(lookInput);

                    TransformState state = new TransformState()
                    {
                        Tick = _tick,
                        Position = transform.position,
                        Rotation = transform.rotation,
                        HasStartedMoving = true
                    };

                    _previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = state;
                }

                InputState inputState = new InputState()
                {
                    Tick = _tick,
                    MovementInput = movementInput,
                    LookInput = lookInput
                };

                TransformState transformState = new TransformState()
                {
                    Tick = _tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                //Store in local
                _inputStates[bufferIndex] = inputState;
                _transformStates[bufferIndex] = transformState;

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }
        
        //Update for all other players in the game that not the local player (player that we controller directly)
        public void ProcessSimulatedPlayerMovement()
        {
            _tickDeltaTime += Time.deltaTime;
            if (_tickDeltaTime > _tickRate)
            {
                if (ServerTransformState.Value.HasStartedMoving)
                {
                    transform.position = ServerTransformState.Value.Position;
                    transform.rotation = ServerTransformState.Value.Rotation;
                }

                _tickDeltaTime -= _tickRate;
                _tick++;
            }
        }
        
        [ServerRpc]
        private void MovePlayerServerRpc(int tick, Vector2 movementInput, Vector2 lookInput)
        {
            //Check if the server receive the message from client or not 
            if (_tick != _previousTransformState.Tick + 1)
            {
                Debug.Log($"Lost the previous message");
                //Tell client to send the message again
            }
            
            MovePlayer(movementInput);
            RotatePlayer(lookInput);

            //Store the current state as the _prevState, then continue the game
            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };

            _previousTransformState = state;
            ServerTransformState.Value = state;
        }

        private void RotatePlayer(Vector2 lookInput)
        {
            _vcamTransform.RotateAround(_vcamTransform.position, _vcamTransform.right, -lookInput.y * _turnSpeed * _tickRate);
            transform.RotateAround(transform.position, transform.up, lookInput.x * _turnSpeed * _tickRate);
        }

        private void MovePlayer(Vector2 movementInput)
        {
            Vector3 movement = movementInput.x * _vcamTransform.right + movementInput.y * _vcamTransform.forward;

            movement.y = 0;
            if (!_cc.isGrounded)
            {
                movement.y = Physics.gravity.y; //= -9.81
            }

            _cc.Move(movement * _speed * _tickRate);
            
        }
    }
}