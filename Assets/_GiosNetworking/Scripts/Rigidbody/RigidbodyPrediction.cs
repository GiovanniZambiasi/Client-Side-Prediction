using System;
using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class RigidbodyPrediction : MonoBehaviour
    {
        const int BufferSize = 1024;

        [SerializeField] NetworkedRigidbody _networkedRigidbody = null;
        [SerializeField] GameObject _phantom = null;
        
        ClientInput[] _inputBuffer = new ClientInput[BufferSize];
        RigidbodyState _lastProcessedState = default;
        bool _showPhantom = true;
        
        void Start()
        {
            _showPhantom = _networkedRigidbody.hasAuthority && !_networkedRigidbody.isServer;
            if (_showPhantom)
            {
                _phantom.transform.SetParent(null);
                _phantom.SetActive(true);
            }
        }

        public void OnTick(float deltaTime, uint currentTick)
        {
            if (!_networkedRigidbody.isServer)
            {
                var __latestServerState = _networkedRigidbody.LatestReveivedState;
                if (!__latestServerState.Equals(_lastProcessedState))
                    UpdatePrediction(currentTick, __latestServerState);
            }

            var __movement = new Vector2();
            __movement.x = Input.GetAxis("Horizontal");
            __movement.y = Input.GetAxis("Vertical");
            
            var __input = new ClientInput(__movement, deltaTime, currentTick);
            var __bufferIndex = currentTick % BufferSize;

            _inputBuffer[__bufferIndex] = __input;

            _networkedRigidbody.CmdSendInput(__input);
            
            if(!_networkedRigidbody.isServer)
                _networkedRigidbody.ProcessInput(__input);
        }

        void UpdatePrediction(uint currentTick, RigidbodyState latestState)
        {
            if (_showPhantom)
            {
                _phantom.transform.position = latestState.position;
                _phantom.transform.rotation = latestState.rotation;
            }
            
            _lastProcessedState = latestState;

            _networkedRigidbody.SetState(_lastProcessedState);
            
            var __firstTickToReprocess = _lastProcessedState.lastProcessedInput + 1;

            if (__firstTickToReprocess < currentTick)
            {
                var __ticksToPredict = (currentTick - __firstTickToReprocess);
                
                for (uint __i = 0, __index = (latestState.lastProcessedInput + 1) % BufferSize;
                    __i < __ticksToPredict;
                    __i++, __index = (__index + 1) % BufferSize)
                {
                    var __input = _inputBuffer[__index];
                    _networkedRigidbody.ProcessInput(__input);
                }
            }
        }
    }
}