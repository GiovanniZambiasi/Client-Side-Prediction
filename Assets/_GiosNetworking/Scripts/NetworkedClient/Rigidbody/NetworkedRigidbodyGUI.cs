using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class NetworkedRigidbodyGUI : NetworkedClientGUI
    {
        [Header("Rigidbody/References")]
        [SerializeField] Rigidbody _rigidbody = null;

        protected override void DrawStats()
        {
            base.DrawStats();
            GUILayout.Label($"Velocity: {_rigidbody.velocity.ToString()}");
        }

        protected override void SetPhantomState(GameObject phantom, INetworkedClientState state)
        {
            var __state = (RigidbodyState) state;
            phantom.transform.position = __state.position;
            phantom.transform.rotation = __state.rotation;
        }
    }
}