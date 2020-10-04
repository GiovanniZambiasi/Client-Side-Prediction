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
        public uint lastProcessedInput;

        public CharacterStateData(Vector3 position, uint lastProcessedInput)
        {
            this.position = position;
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

        public InputData(Vector2 input, uint tick)
        {
            this.input = input;
            this.tick = tick;
        }
    }

    public struct CharacterStateWithTimestamp
    {
        public CharacterStateData stateData;
        public float time;
    }
    
    public struct InputDataWithTimestamp
    {
        public InputData input;
        public float time;
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

        Queue<InputData> _inputQueue = new Queue<InputData>(6);
        //List<CharacterStateWithTimestamp> _statesToSend = new List<CharacterStateWithTimestamp>(6);
        CharacterStateData _latestServerState;
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
                _timeSinceLastTick -= _serverDeltaTime;

                if (isClient && hasAuthority)
                    _prediction.OnTick(_currentTick);
                else if (!isServer)
                    SetState(_latestServerState);
                
                if (isServer)
                    ServerOnUpdate();

                _currentTick++;
            }

            /*var __time = Time.time;

            if (isServer)
            {
                for (int __i = _statesToSend.Count - 1; __i >= 0; __i--)
                {
                    var __state = _statesToSend[__i];
                    if (__time - __state.time >= _artificialLagInSeconds)
                    {
                        TargetSendState(__state.stateData);
                        _statesToSend.RemoveAt(__i);
                    }
                }
            }*/
        }

        public void ProcessMovement(InputData data)
        {
            var __input = new Vector3(data.input.x, 0f, data.input.y);
            var __movement = Vector3.ClampMagnitude(__input, 1f) * _speed;
            __movement += Physics.gravity;

            _characterController.Move(__movement * _serverDeltaTime);
        }

        public void SetState(CharacterStateData state)
        {
            _characterController.enabled = false;
            _characterController.transform.position = _latestServerState.position;
            _characterController.enabled = true;
        }

        [Command]
        public void CmdMove(InputData inputData)
        {
            _inputQueue.Enqueue(inputData);
        }

        [ClientRpc]
        public void RpcSendState(CharacterStateData state)
        {
            _latestServerState = state;
        }

        void ServerOnUpdate()
        {
            if (_inputQueue.Count == 0)
                ProcessMovement(new InputData(Vector2.zero, _currentTick));
            else
            {
                while (_inputQueue.Count > 0)
                {
                    Debug.Log($"Input queue {_inputQueue.Count}");
                    var __input = _inputQueue.Dequeue();

                    ProcessMovement(__input);

                    _lastProcessedInput = __input.tick;
                }
            }

            var __position = new CharacterStateData(_characterController.transform.position, _lastProcessedInput);
            
            /*_statesToSend.Insert(0, new CharacterStateWithTimestamp
            {
                stateData = __position,
                time = Time.time,
            });*/
            
            RpcSendState(__position);
        }
    }
}