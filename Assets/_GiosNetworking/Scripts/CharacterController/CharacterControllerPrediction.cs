using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class CharacterControllerPrediction : MonoBehaviour
    {
        const int BufferSize = 1024;
        
        [SerializeField] NetworkedClient _networkedClient = null;
        [SerializeField] GameObject _phantom = null; // Used to display the last synced position from the server
        
        ClientInput[] _inputBuffer = new ClientInput[BufferSize];
        CharacterState _lastProcessedState = default;
        bool _showPhantom = false;

        void Start()
        {
            _showPhantom = _networkedClient.hasAuthority && !_networkedClient.isServer;
            if (_showPhantom)
            {
                _phantom.transform.SetParent(null);
                _phantom.SetActive(true);
            }
        }
        
        public void OnTick(float deltaTime, uint currentTick)
        {
            var __lastestServerState = _networkedClient.LatestServerState;
            if (!__lastestServerState.Equals(_lastProcessedState))            // If the latest state received by the server has not been processed
                UpdatePrediction(currentTick, __lastestServerState);

            var __inputs = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            
            var __inputData = new ClientInput(__inputs, currentTick, deltaTime);

            var __bufferIndex = currentTick % BufferSize;
            
            _inputBuffer[__bufferIndex] = __inputData;

            _networkedClient.CmdMove(__inputData);
            if(!_networkedClient.isServer)
                _networkedClient.ProcessInput(__inputData);
        }

        public void SetPhantom(bool isActive)
        {
            _showPhantom = isActive;
            _phantom.SetActive(isActive);
        }
        
        void UpdatePrediction(uint currentTick, CharacterState latestState)
        {
            if(_showPhantom)
                _phantom.transform.position = latestState.position + Vector3.up;
            
            _lastProcessedState = latestState;
            
            _networkedClient.SetState(_lastProcessedState);
            
            var __firstTickToReprocess = latestState.lastProcessedInput + 1;

            if (__firstTickToReprocess < currentTick)
            {
                var __ticksToPredict = (currentTick - __firstTickToReprocess);

                for (uint __i = 0, __index = (latestState.lastProcessedInput + 1) % BufferSize;
                    __i < __ticksToPredict;
                    __i++, __index = (__index + 1) % BufferSize)
                {
                    var __input = _inputBuffer[__index];
                    _networkedClient.ProcessInput(__input);
                }
            }
        }
    }
}