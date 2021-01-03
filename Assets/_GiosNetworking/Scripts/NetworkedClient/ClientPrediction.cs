using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public abstract class ClientPrediction<TClientInput, TClientState> : MonoBehaviour 
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
    {
        [Header("Prediction/References")]
        [SerializeField] NetworkIdentity _identity = null;
        [Header("Prediction/Settings")] 
        [SerializeField, Tooltip("The number of ticks to be stored in the input and state buffers")]
        uint _bufferSize = 1024;
        
        NetworkedClient<TClientInput, TClientState> _client = null;
        TClientInput[] _inputBuffer;
        TClientState _lastProcessedState = default;
        
        protected virtual void Awake()
        {
            _client = GetComponent<NetworkedClient<TClientInput, TClientState>>();
            
            if(_client == null)
                Debug.LogError($"Couldn't find client for {name}");

            _inputBuffer = new TClientInput[_bufferSize];
        }
 
        public void HandleTick(float deltaTime, uint currentTick, TClientState latestServerState)
        {
            if(!_identity.isServer && (latestServerState != null && (_lastProcessedState == null || !_lastProcessedState.Equals(latestServerState))))
                UpdatePrediction(currentTick, latestServerState);

            var __input = GetInput(deltaTime, currentTick);

            var __bufferIndex = currentTick % _bufferSize;

            _inputBuffer[__bufferIndex] = __input;
            
            _client.SendClientInput(__input);
            
            if(!_identity.isServer)
                _client.ProcessInput(__input);
        }

        protected abstract TClientInput GetInput(float deltaTime, uint currentTick);
        
        void UpdatePrediction(uint currentTick, TClientState latestServerState)
        {
            _lastProcessedState = latestServerState;
            
            _client.SetState(_lastProcessedState);

            var __firstTickToReprocess = _lastProcessedState.LastProcessedInputTick + 1;

            if (__firstTickToReprocess < currentTick)
            {
                var __ticksToPredict = currentTick - __firstTickToReprocess;
                
                for (uint __i = 0, __index = __firstTickToReprocess  % _bufferSize;
                    __i < __ticksToPredict;
                    __i++, __index = (__index + 1) % _bufferSize)
                {
                    var __input = _inputBuffer[__index];
                    _client.ProcessInput(__input);
                }
            }
        }
    }
}