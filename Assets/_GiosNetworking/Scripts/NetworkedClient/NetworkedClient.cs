using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public abstract class NetworkedClient<TClientInput, TClientState> : MonoBehaviour, INetworkedClient 
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState 
    {
        public INetworkedClientState LatestServerState => _messenger.LatestServerState;
        public uint CurrentTick => _currentTick;

        [Header("Client/References")] 
        [SerializeField] NetworkIdentity _identity = null;

        INetworkedClientMessenger<TClientInput, TClientState> _messenger;
        ClientPrediction<TClientInput, TClientState> _prediction = null;
        Queue<TClientInput> _inputQueue = new Queue<TClientInput>(6);       // *TODO maybe move the queue to the INetworkedClientMessenger. This would avoid the need for an event, but would still require the dev to declare and maintain the queue
        float _minTimeBetweenUpdates = 0f;
        float _timeSinceLastTick = 0f;
        uint _lastProcessedInputTick = 0;
        uint _currentTick = 0;

        void Awake()
        {
            _prediction = GetComponent<ClientPrediction<TClientInput, TClientState>>();
            
            if(_prediction == null)
                Debug.LogError($"Couldn't find client for {name}");

            _messenger = GetComponent<INetworkedClientMessenger<TClientInput, TClientState>>();
            
            if(_messenger == null)
                Debug.LogError($"Couldn't find sender for {name}");
            else
            {
                _messenger.OnInputReceived += HandleInputReceived;
            }
            
            _minTimeBetweenUpdates = 1f / NetworkManager.singleton.serverTickRate;
        }

        void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _minTimeBetweenUpdates)
                HandleTick();
        }

        public abstract void SetState(TClientState state);

        public abstract void ProcessInput(TClientInput input);

        public void SendClientInput(TClientInput input)
        {
            _messenger.SendInput(input);
        }

        protected abstract TClientState RecordState(uint lastProcessedInputTick);
        
        void HandleInputReceived(TClientInput input)
        {
            _inputQueue.Enqueue(input);
        }
        
        void HandleTick()
        {
            if (_identity.isClient && _identity.hasAuthority)
                _prediction.HandleTick(_timeSinceLastTick, _currentTick, _messenger.LatestServerState);    // Client-side prediction
            else if (!_identity.isServer)
                HandleOtherPlayerState(_messenger.LatestServerState);                                       // Entity interpolation *TODO
                
            if(_identity.isServer)
                ServerProcessInputsAndSendState();

            _currentTick++;
            _timeSinceLastTick = 0f;
        }

        protected virtual void HandleOtherPlayerState(TClientState state)
        {
            SetState(state);
        }

        void ServerProcessInputsAndSendState()
        {
            ProcessInputs();
            SendState();
        }

        void ProcessInputs()
        {
            while (_inputQueue.Count > 0)
            {
                var __input = _inputQueue.Dequeue();
                ProcessInput(__input);
                
                _lastProcessedInputTick = __input.Tick;
            }
        }

        void SendState()
        {
            var __state = RecordState(_lastProcessedInputTick);
            _messenger.SendState(__state);
        }

        protected void LogState()
        {
            Debug.Log(LatestServerState.ToString());
        }

        protected void LogInputQueue()
        {
            var __log = $"Input queue count: {_inputQueue.Count.ToString()}\n";
            
            for (var i = 0; i < _inputQueue.Count; i++)
            {
                var __input = _inputQueue.ElementAt(i);
                __log += $"{__input.ToString()}\n";
            }

            Debug.Log(__log);
        }
    }
}