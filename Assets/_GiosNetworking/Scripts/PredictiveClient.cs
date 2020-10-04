using System;
using UnityEngine;

namespace ClientSidePrediction
{
    public class PredictiveClient : MonoBehaviour
    {
        const int BufferSize = 1024;
        
        [SerializeField] NetworkedClient _networkedClient = null;
        
        InputData[] _inputBuffer = new InputData[BufferSize];
        CharacterStateData _lastProcessedState = default;

        void Start()
        {
            Debug.Log($"Start {_networkedClient.hasAuthority}");
        }
        
        public void OnTick(uint currentTick)
        {
            var __lastestServerState = _networkedClient.LatestServerState;
            if (!__lastestServerState.Equals(_lastProcessedState))            // If the latest state received by the server has not been processed
                UpdatePrediction(currentTick, __lastestServerState);

            var __inputs = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            
            var __inputData = new InputData(__inputs, currentTick);

            var __bufferIndex = currentTick % BufferSize;
            
            _inputBuffer[__bufferIndex] = __inputData;

            /*_inputsToSend.Add(new InputDataWithTimestamp
            {
                input =  __inputData,
                time = Time.time,
            });*/
            
            _networkedClient.CmdMove(__inputData);
            if(!_networkedClient.isServer)
                _networkedClient.ProcessMovement(__inputData);
        }
        
        void UpdatePrediction(uint currentTick,  CharacterStateData latestStateData)
        {
            _lastProcessedState = latestStateData;
            
            _networkedClient.SetState(_lastProcessedState);
            
            var __firstTickToReprocess = latestStateData.lastProcessedInput + 1;

            if (__firstTickToReprocess < currentTick)
            {
                var __ticksToPredict = (currentTick - __firstTickToReprocess);

                for (uint __i = 0, __index = (latestStateData.lastProcessedInput + 1) % BufferSize;
                    __i < __ticksToPredict;
                    __i++, __index = (__index + 1) % BufferSize)
                {
                    var __input = _inputBuffer[__index];
                    _networkedClient.ProcessMovement(__input);
                }
            }
        }

    }
}