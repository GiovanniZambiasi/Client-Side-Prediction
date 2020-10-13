using System;
using System.Text;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public class NetworkedClientGUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] PredictiveClient _predictiveClient = null;
        [SerializeField] NetworkedClient _networkedClient = null;
        [SerializeField] CharacterController _characterController = null;
        [Header("Settings")]
        [SerializeField] Vector2Int _windowSize = new Vector2Int(225, 100);
        [SerializeField] Vector2 _windowViewportPosition = new Vector2(1, 0);

        Camera _camera;
        int _targetFPS = 64;
        bool _showPhantom = true;

        void Awake()
        {
            _targetFPS = NetworkManager.singleton.serverTickRate;
        }

        void OnGUI()
        {
            if (!_networkedClient.isClient || !_networkedClient.hasAuthority)
                return;

            if (_camera == null)
                _camera = Camera.main;
            
            var __screenPoint = _camera.ViewportToScreenPoint(_windowViewportPosition);
            
            __screenPoint.x = Mathf.Clamp(__screenPoint.x, 0, Screen.width - _windowSize.x);
            __screenPoint.y = Mathf.Clamp(__screenPoint.y, 0, Screen.height - _windowSize.y);
            
            var __rect = new Rect(__screenPoint, _windowSize);
            
            GUI.Box(__rect, string.Empty);

            GUILayout.BeginArea(__rect);
            DrawStats();
            GUILayout.Space(10);
            DrawSettings();
            GUILayout.EndArea();
        }

        void DrawSettings()
        {
            GUILayout.Label("Settings");

            var __prev = _showPhantom;
            _showPhantom = GUILayout.Toggle(_showPhantom, "Show phantom");

            if (__prev != _showPhantom)
                _predictiveClient.SetPhantom(_showPhantom);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target FPS:");
            
            var __targetFPS = GUILayout.TextField(_targetFPS.ToString());
            if (int.TryParse(__targetFPS, out var __result))
                _targetFPS = __result;

            if (GUILayout.Button("Apply"))
                Application.targetFrameRate = _targetFPS;
            
            GUILayout.EndHorizontal();
        }

        void DrawStats()
        {
            GUILayout.Label("Stats");
            GUILayout.Label($"Current Tick: {_networkedClient.CurrentTick.ToString()}");
            GUILayout.Label($"Delta Time: {_networkedClient.ServerDeltaTime.ToString()}");
            GUILayout.Label($"Rtt: {NetworkTime.rtt.ToString()}");
            GUILayout.Label($"Velocity: {_characterController.velocity.ToString()}");
        }
    }
}