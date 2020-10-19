using System;
using UnityEngine;

namespace ClientSidePrediction
{
    public abstract class ClientPrediction : MonoBehaviour
    {
        [Header("Prediction/Settings")] 
        [SerializeField, Tooltip("The number of ticks to be stored in the input and state buffers")]
        uint _bufferSize = 1024;
        [Header("Prediction/References")]
        [SerializeField] NetworkedClient _client = null;
        
        INetworkedClientInput[] _inputBuffer;
        INetworkedClientState _lastProcessedState = default;
        
        protected virtual void Awake()
        {
            _inputBuffer = new INetworkedClientInput[_bufferSize];
        }
 
        public void HandleTick(float deltaTime, uint currentTick, INetworkedClientState latestServerState)
        {
            if(!_client.isServer && (latestServerState != null && (_lastProcessedState == null || !_lastProcessedState.Equals(latestServerState))))
                UpdatePrediction(currentTick, latestServerState);

            var __input = GetInput(deltaTime, currentTick);

            var __bufferIndex = currentTick % _bufferSize;

            _inputBuffer[__bufferIndex] = __input;
            
            _client.SendClientInput(__input);
            
            if(!_client.isServer)
                _client.ProcessInput(__input);
        }

        protected abstract INetworkedClientInput GetInput(float deltaTime, uint currentTick);
        
        void UpdatePrediction(uint currentTick, INetworkedClientState latestServerState)
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