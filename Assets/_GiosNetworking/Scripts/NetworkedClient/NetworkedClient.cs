using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public abstract class NetworkedClient : NetworkBehaviour
    {
        public INetworkedClientState LatestServerState => _latestServerState;
        public uint CurrentTick => _currentTick;

        [Header("Client/References")]
        [SerializeField] ClientPrediction _prediction = null;
        
        Queue<INetworkedClientInput> _inputQueue = new Queue<INetworkedClientInput>(6);
        INetworkedClientState _latestServerState;
        float _minTimeBetweenUpdates = 0f;
        float _timeSinceLastTick = 0f;
        uint _lastProcessedInputTick = 0;
        uint _currentTick = 0;

        void Awake()
        {
            _minTimeBetweenUpdates = 1f / NetworkManager.singleton.serverTickRate;
        }

        void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _minTimeBetweenUpdates)
                HandleTick();
        }

        public abstract void SetState(INetworkedClientState state);

        public abstract void ProcessInput(INetworkedClientInput input);

        public abstract void SendClientInput(INetworkedClientInput input);

        protected void ClientHandleStateReceived(INetworkedClientState state)
        {
            _latestServerState = state;
        }

        protected void ServerHandleInputReceived(INetworkedClientInput input)
        {
            _inputQueue.Enqueue(input);
        }

        protected abstract void SendServerState(uint lastProcessedInputTick);
        
        void ProcessInputs()
        {
            while (_inputQueue.Count > 0)
            {
                var __input = _inputQueue.Dequeue();
                ProcessInput(__input);
                
                _lastProcessedInputTick = __input.Tick;
            }
        }
        
        void HandleTick()
        {
            if (isClient && hasAuthority)
                _prediction.HandleTick(_timeSinceLastTick, _currentTick, _latestServerState);    // Client-side prediction
            else if (!isServer)
                SetState(_latestServerState);                                                    // Entity interpolation *TODO
            
            if(isServer)
                ServerProcessInputsAndSendState();

            _currentTick++;
            _timeSinceLastTick = 0f;
        }

        void ServerProcessInputsAndSendState()
        {
            ProcessInputs();
            SendServerState(_lastProcessedInputTick);   
        }
    }
}