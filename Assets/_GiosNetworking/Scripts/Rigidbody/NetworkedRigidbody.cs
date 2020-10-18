using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction.RB
{
    public struct RigidbodyState : IEquatable<RigidbodyState>
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 angularVelocity;
        public Quaternion rotation;
        public uint lastProcessedInput;

        public RigidbodyState(Vector3 position, Vector3 velocity, Vector3 angularVelocity, Quaternion rotation, uint lastProcessedInput)
        {
            this.position = position;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity;
            this.rotation = rotation;
            this.lastProcessedInput = lastProcessedInput;
        }

        public bool Equals(RigidbodyState other)
        {
            return position.Equals(other.position) && velocity.Equals(other.velocity) && 
                   angularVelocity.Equals(other.angularVelocity) && rotation.Equals(other.rotation);
        }
    }

    public struct ClientInput
    {
        public Vector2 movement;
        public float deltaTime;
        public uint tick;
        
        public ClientInput(Vector2 movement, float deltaTime, uint tick)
        {
            this.movement = movement;
            this.deltaTime = deltaTime;
            this.tick = tick;
        }
    }

    public class NetworkedRigidbody : NetworkBehaviour
    {
        public RigidbodyState LatestReveivedState => _latestReceivedState;
        
        [Header("References")]
        [SerializeField] Rigidbody _rigidbody = null;
        [SerializeField] RigidbodyPrediction _client = null;
        [Header("Settings")]
        [SerializeField] float _speed = 10f;

        Queue<ClientInput> _inputQueue = new Queue<ClientInput>(16);
        RigidbodyState _latestReceivedState;
        float _serverDeltaTime;
        float _timeSinceLastTick = 0f;
        uint _currentTick = 0;
        uint _lastProcessedInput = 0;
        
        void Awake()
        {
            _serverDeltaTime = 1f / NetworkManager.singleton.serverTickRate;
        }

        void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _serverDeltaTime)
            {
                OnTick();
            }
        }

        void ServerOnUpdate()
        {
            while (_inputQueue.Count > 0)
            {
                var __input = _inputQueue.Dequeue();
                
                ProcessInput(__input);
                
                _lastProcessedInput = __input.tick;
            }
            
            var __state = new RigidbodyState(_rigidbody.position, _rigidbody.velocity, _rigidbody.angularVelocity, _rigidbody.rotation, _lastProcessedInput);
            RpcSendState(__state);
        }

        [ClientRpc]
        public void RpcSendState(RigidbodyState state)
        {
            _latestReceivedState = state;
        }

        [Command]
        public void CmdSendInput(ClientInput clientInput)
        {
            _inputQueue.Enqueue(clientInput);
        }

        public void SetState(RigidbodyState state)
        {
            _rigidbody.position = state.position;
            _rigidbody.velocity = state.velocity;
            _rigidbody.rotation = state.rotation;
            _rigidbody.angularVelocity = state.angularVelocity;
        }

        public void ProcessInput(ClientInput clientInput)
        {
            var __force = new Vector3(clientInput.movement.x, 0f, clientInput.movement.y);
            __force *= _speed * clientInput.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
        }

        void OnTick()
        {
            if(isClient && hasAuthority)
                _client.OnTick(_timeSinceLastTick, _currentTick);
            else if(!isServer)
                SetState(_latestReceivedState);
            
            if (isServer)
                ServerOnUpdate();

            _timeSinceLastTick = 0f;
            _currentTick++;
        }
    }
}