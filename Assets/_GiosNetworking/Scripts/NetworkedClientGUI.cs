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
        [SerializeField] Rect _rect = new Rect(10, 400, 225, 100);

        bool _showPhantom = true;
        
        void OnGUI()
        {
            if (!_networkedClient.isClient || !_networkedClient.hasAuthority)
                return;
            
            GUI.Box(_rect, string.Empty);

            GUILayout.BeginArea(_rect);
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
        }

        void DrawStats()
        {
            GUILayout.Label("Stats");
            GUILayout.Label($"Current Tick: {_networkedClient.CurrentTick}");
            GUILayout.Label($"Server Time: {NetworkTime.time}");
            GUILayout.Label($"Delta Time: {_networkedClient.ServerDeltaTime}");
            GUILayout.Label($"Rtt: {NetworkTime.rtt}");
            GUILayout.Label($"Velocity: {_characterController.velocity}");
        }
    }
}