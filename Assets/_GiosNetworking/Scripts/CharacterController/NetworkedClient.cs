using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class NetworkedClient : NetworkBehaviour
    {
        public CharacterState LatestServerState => _latestServerState;
        public uint CurrentTick => _currentTick;
        public float MinTimeBetweenUpdates => _minTimeBetweenUpdates;

        [Header("References")]
        [SerializeField] CharacterControllerPrediction _prediction = null;
        [SerializeField] CharacterController _characterController = null;
        [Header("Settings")]
        [SerializeField] float _speed = 10f;

        Queue<ClientInput> _inputQueue = new Queue<ClientInput>(6);     // Queue of inputs the server needs to process 
        CharacterState _latestServerState;                                      // Latest frame received by the server
        float _verticalVelocity = 0f; 
        float _minTimeBetweenUpdates = 0f;
        float _timeSinceLastTick = 0f;
        uint _lastProcessedInput = 0;
        uint _currentTick = 0;

        void Awake()
        {
            _minTimeBetweenUpdates = 1f / NetworkManager.singleton.serverTickRate;
        }

        void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _minTimeBetweenUpdates)
            {
                OnTick();
            }
        }

        public void ProcessInput(ClientInput data)
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

        public void SetState(CharacterState state)
        {
            _characterController.enabled = false;
            _characterController.transform.position = _latestServerState.position;
            _verticalVelocity = state.verticalVelocity;
            _characterController.enabled = true;
        }

        [Command(channel = Channels.DefaultUnreliable)]
        public void CmdMove(ClientInput clientInput)
        {
            _inputQueue.Enqueue(clientInput);
        }

        [ClientRpc(channel = Channels.DefaultUnreliable)]
        public void RpcSendState(CharacterState state)
        {
            _latestServerState = state;
        }

        void OnTick()
        {
            if (isClient && hasAuthority)
                _prediction.OnTick(_timeSinceLastTick, _currentTick);
            else if (!isServer)
                SetState(_latestServerState);    // Entity interpolation eventually goes in here
                
            if (isServer)
                ServerOnUpdate();

            _currentTick++;
                
            _timeSinceLastTick = 0f;
        }
        
        void ServerOnUpdate()
        {
            while (_inputQueue.Count > 0)
            {
                var __input = _inputQueue.Dequeue();

                ProcessInput(__input);

                _lastProcessedInput = __input.tick;
            }
            
            var __state = new CharacterState(_characterController.transform.position, _verticalVelocity, _lastProcessedInput);
            
            RpcSendState(__state);
        }
    }
}