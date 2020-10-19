using Mirror;
using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class NetworkedRigidbody : NetworkedClient
    {
        [Header("Rigidbody/References")]
        [SerializeField] Rigidbody _rigidbody = null;
        [Header("Rigidbody/Settings")]
        [SerializeField] float _speed = 10f;

        [ClientRpc]
        public void RpcSendState(RigidbodyState state)
        {
            ClientHandleStateReceived(state);
        }

        [Command]
        public void CmdSendInput(ClientInput clientInput)
        {
            ServerHandleInputReceived(clientInput);
        }
        
        public override void SetState(INetworkedClientState state)
        {
            var __state = (RigidbodyState)state;
            _rigidbody.position = __state.position;
            _rigidbody.velocity = __state.velocity;
            _rigidbody.rotation = __state.rotation;
            _rigidbody.angularVelocity = __state.angularVelocity;
        }

        public override void ProcessInput(INetworkedClientInput input)
        {
            var __input = (ClientInput) input;
            var __force = new Vector3(__input.movement.x, 0f, __input.movement.y);
            __force *= _speed * __input.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
        }

        public override void SendClientInput(INetworkedClientInput input)
        {
            var __input = (ClientInput)input;
            CmdSendInput(__input);
        }

        protected override void SendServerState(uint lastProcessedInputTick)
        {
            var __state = new RigidbodyState(_rigidbody.position, _rigidbody.velocity, _rigidbody.angularVelocity, _rigidbody.rotation, lastProcessedInputTick);
            RpcSendState(__state);
        }
    }
}