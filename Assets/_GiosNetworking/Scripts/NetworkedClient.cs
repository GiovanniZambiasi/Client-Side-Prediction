using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    [System.Serializable]
    public struct CharacterStateData: IEquatable<CharacterStateData>
    {
        public Vector3 position;
        public float verticalVelocity;
        public uint lastProcessedInput;

        public CharacterStateData(Vector3 position, float verticalVelocity, uint lastProcessedInput)
        {
            this.position = position;
            this.verticalVelocity = verticalVelocity;
            this.lastProcessedInput = lastProcessedInput;
        }

        public bool Equals(CharacterStateData other)
        {
            return position.Equals(other.position) && lastProcessedInput == other.lastProcessedInput;
        }

        public override bool Equals(object obj)
        {
            return obj is CharacterStateData other && Equals(other);
        }
    }

    [System.Serializable]
    public struct InputData
    {
        public uint tick;
        public Vector2 input;
        public float deltaTime;

        public InputData(Vector2 input, uint tick, float deltaTime)
        {
            this.input = input;
            this.tick = tick;
            this.deltaTime = deltaTime;
        }
    }

    public class NetworkedClient : NetworkBehaviour
    {
        public CharacterStateData LatestServerState => _latestServerState;
        public uint CurrentTick => _currentTick;
        public float ServerDeltaTime => _serverDeltaTime;

        [Header("References")]
        [SerializeField] PredictiveClient _prediction = null;
        [SerializeField] CharacterController _characterController = null;
        [Header("Settings")]
        [SerializeField] float _speed = 10f;

        Queue<InputData> _inputQueue = new Queue<InputData>(6);     // Queue of inputs the server needs to process 
        CharacterStateData _latestServerState;                              // Latest frame received by the server
        float _verticalVelocity = 0f; 
        float _serverDeltaTime = 0f;
        float _timeSinceLastTick = 0f;
        uint _lastProcessedInput = 0;
        uint _currentTick = 0;

        void Awake()
        {
            _serverDeltaTime = 1f / NetworkManager.singleton.serverTickRate;
        }

        void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _serverDeltaTime)
            {
                if (isClient && hasAuthority)
                    _prediction.OnTick(_timeSinceLastTick, _currentTick);
                else if (!isServer)
                    SetState(_latestServerState);
                
                if (isServer)
                    ServerOnUpdate();

                _currentTick++;
                
                _timeSinceLastTick = 0f;
            }
        }

        public void ProcessMovement(InputData data)
        {
            var __input = new Vector3(data.input.x, 0f, data.input.y);
            var __movement = Vector3.ClampMagnitude(__input, 1f) * _speed;

            if (!_characterController.isGrounded)
                _verticalVelocity += Physics.gravity.y * data.deltaTime;
            else
                _verticalVelocity = Physics.gravity.y;

            __movement.y = _verticalVelocity;
            
            _characterController.Move(__movement * data.deltaTime);
        }

        public void SetState(CharacterStateData state)
        {
            _characterController.enabled = false;
            _characterController.transform.position = _latestServerState.position;
            _verticalVelocity = state.verticalVelocity;
            _characterController.enabled = true;
        }

        [Command(channel = Channels.DefaultUnreliable)]
        public void CmdMove(InputData inputData)
        {
            _inputQueue.Enqueue(inputData);
        }

        [ClientRpc(channel = Channels.DefaultUnreliable)]
        public void RpcSendState(CharacterStateData state)
        {
            _latestServerState = state;
        }

        void ServerOnUpdate()
        {
            /*if (_inputQueue.Count == 0)
                ProcessMovement(new InputData(Vector2.zero, _currentTick));
            else
            {*/
                while (_inputQueue.Count > 0)
                {
                    var __input = _inputQueue.Dequeue();

                    ProcessMovement(__input);

                    _lastProcessedInput = __input.tick;
                }
            //}

            var __position = new CharacterStateData(_characterController.transform.position, _verticalVelocity, _lastProcessedInput);
            
            /*_statesToSend.Insert(0, new CharacterStateWithTimestamp
            {
                stateData = __position,
                time = Time.time,
            });*/
            
            RpcSendState(__position);
        }
    }
}