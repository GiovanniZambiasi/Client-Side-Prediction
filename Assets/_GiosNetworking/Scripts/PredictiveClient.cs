using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    [System.Serializable]
    public struct CharacterStateData
    {
        public Vector3 position;
        public uint lastProcessedInput;

        public CharacterStateData(Vector3 position, uint lastProcessedInput)
        {
            this.position = position;
            this.lastProcessedInput = lastProcessedInput;
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

    public class PredictiveClient : NetworkBehaviour
    {
        const int BufferSize = 1024;
        
        [SerializeField] CharacterController _characterController = null;
        [SerializeField] MeshFilter _mesh = null;
        [SerializeField] float _speed = 10f;
        [SerializeField] float _positionErrorTolerance = .001f;

        Queue<InputData> _inputQueue = new Queue<InputData>(BufferSize);
        InputData[] _inputBuffer = new InputData[BufferSize];
        CharacterStateData _latestServerState;
        float _serverDeltaTime = 0f;
        float _timeSinceLastTick = 0f;
        uint _lastProcessedInput = 0;
        uint _currentTick = 0;
        bool _hasUnprocessedServerState = false;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 400, 225, 100));
            GUI.Box(new Rect(0, 0, 225, 100), string.Empty);
            GUILayout.Label($"Current Tick: {_currentTick}");   
            GUILayout.Label($"Server Time: {NetworkTime.time}");   
            GUILayout.Label($"Delta Time: {_serverDeltaTime}");   
            GUILayout.Label($"Rtt: {NetworkTime.rtt}");   
            GUILayout.Label($"Velocity: {_characterController.velocity}");   
            GUILayout.EndArea();
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying || isServer) return;
            
            Gizmos.color = Color.red;

            if (!_hasUnprocessedServerState)
                return;
            
            Gizmos.DrawWireMesh(_mesh.mesh, _latestServerState.position + Vector3.up);
        }

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
                    ClientOnTick();
                if (isServer)
                    ServerOnUpdate();
            }
        }

        [Command]
        public void CmdMove(InputData inputData)
        {
            _inputQueue.Enqueue(inputData);
            //ProcessMovement(inputData);
        }

        [TargetRpc]
        public void TargetSetState(CharacterStateData state)
        {
            _hasUnprocessedServerState = true;
            _latestServerState = state;
            //_latestServerState.Enqueue(state);
            
            /*
            transform.position = data.position;

            Debug.Log($"Last processed {lastProcessedInput} | current {_currentTick} | to recalculate {_currentTick - lastProcessedInput} | Frame {Time.frameCount}");
            var __recalculations = _currentTick - lastProcessedInput;
            for (uint __i = 0, __index = (lastProcessedInput + 1) % BufferSize; __i < __recalculations; __i++, __index = (__index + 1) % BufferSize)
            {
                var __input = _inputBuffer[__index];
                ProcessMovement(__input);
            }*/
            
            /*if (__difference.sqrMagnitude >= (_positionErrorTolerance * _positionErrorTolerance))
            {
                _positionBuffer[__index] = data;
                CorrectPositioning(data);
                Debug.LogError($"Prediction Error for tick {data.tick}, recalculating");
            }*/
        }

        void CorrectPositioning(CharacterStateData latestPositionData, uint lastProcessedInput)
        {
            var __startIndex = latestPositionData.lastProcessedInput % BufferSize;
            var __endIndex = _currentTick % BufferSize;

            transform.position = latestPositionData.position;
            
            __startIndex = (__startIndex + 1) % BufferSize;
            
            /*for (var __i = __startIndex; __i != __endIndex; __i = (__i + 1) % BufferSize)
            {
                var __input = _inputBuffer[__i];
                ProcessMovement(__input);
                
                var __position = new CharacterStateData(transform.position, __input.tick);
                _positionBuffer[__i] = __position;
            }*/
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
            TargetSetState(__position);
            _currentTick++;
        }

        void ClientOnTick()
        {
            if (_hasUnprocessedServerState)
            {
                _hasUnprocessedServerState = false;
                _characterController.enabled = false;
                _characterController.transform.position = _latestServerState.position;
                _characterController.enabled = true;

                var __firstTickToReprocess = _latestServerState.lastProcessedInput + 1;

                if (__firstTickToReprocess < _currentTick)
                {
                    var __ticksToPredict = (_currentTick - __firstTickToReprocess);

                    Debug.Log(
                        $"Last processed by server {_latestServerState.lastProcessedInput} | Current tick {_currentTick} | Will predict {__ticksToPredict} ticks");

                    for (uint __i = 0, __index = (_latestServerState.lastProcessedInput + 1) % BufferSize;
                        __i < __ticksToPredict;
                        __i++, __index = (__index + 1) % BufferSize)
                    {
                        var __input = _inputBuffer[__index];
                        ProcessMovement(__input);
                    }
                }
            }

            var __inputs = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            
            var __movementData = new InputData(__inputs, _currentTick);

            var __bufferIndex = _currentTick % BufferSize;
            
            _inputBuffer[__bufferIndex] = __movementData;

            CmdMove(__movementData);

            ProcessMovement(__movementData);
            
            _currentTick++;
        }

        void ProcessMovement(InputData data)
        {
            var __input = new Vector3(data.input.x, 0f, data.input.y);
            var __movement = Vector3.ClampMagnitude(__input, 1f) * _speed;
            __movement += Physics.gravity;
            
            _characterController.Move(__movement * _serverDeltaTime);
        }
    }
}