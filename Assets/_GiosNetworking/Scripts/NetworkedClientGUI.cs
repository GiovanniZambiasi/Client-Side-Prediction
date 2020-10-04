using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public class NetworkedClientGUI : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] NetworkedClient _networkedClient;
        [SerializeField] CharacterController _characterController = null;
        [Header("Settings")]
        [SerializeField] Rect _rect = new Rect(10, 400, 225, 100);

        void OnGUI()
        {
            if (!_networkedClient.isClient || !_networkedClient.hasAuthority)
                return;
            
            GUI.Box(_rect, string.Empty);

            GUILayout.BeginArea(_rect);
            GUILayout.Label($"Current Tick: {_networkedClient.CurrentTick}");
            GUILayout.Label($"Server Time: {NetworkTime.time}");
            GUILayout.Label($"Delta Time: {_networkedClient.ServerDeltaTime}");
            GUILayout.Label($"Rtt: {NetworkTime.rtt}");
            GUILayout.Label($"Velocity: {_characterController.velocity}");
            GUILayout.EndArea();
        }
    }
}