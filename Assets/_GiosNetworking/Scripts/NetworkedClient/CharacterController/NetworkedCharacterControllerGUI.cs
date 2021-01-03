using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class NetworkedCharacterControllerGUI : NetworkedClientGUI
    {
        [SerializeField] CharacterController _characterController = null;
        
        protected override void DrawStats()
        {
            base.DrawStats();
            GUILayout.Label($"Velocity: {_characterController.velocity.ToString()}");
        }

        protected override void SetPhantomState(GameObject phantom, INetworkedClientState state)
        {
            var __state = (CharacterControllerState) state;
            phantom.transform.position = __state.position;
        }
    }
}