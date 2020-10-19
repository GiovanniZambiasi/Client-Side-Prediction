using Mirror;
using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class NetworkedCharacterController : NetworkedClient
    {
        [Header("CharacterController/References")]
        [SerializeField] CharacterController _characterController = null;
        [Header("CharacterController/Settings")]
        [SerializeField] float _speed = 10f;
        float _verticalVelocity = 0f; 

        public override void SetState(INetworkedClientState state)
        {
            var __state = (CharacterState)state;
            _characterController.enabled = false;
            _characterController.transform.position = __state.position;
            _verticalVelocity = __state.verticalVelocity;
            _characterController.enabled = true;
        }

        public override void ProcessInput(INetworkedClientInput input)
        {
            var __castedInput = (CharacterInput)input;
            
            var __movement = new Vector3(__castedInput.input.x, 0f, __castedInput.input.y);
            __movement = Vector3.ClampMagnitude(__movement, 1f) * _speed;
            if (!_characterController.isGrounded)
                _verticalVelocity += Physics.gravity.y * __castedInput.deltaTime;
            else
                _verticalVelocity = Physics.gravity.y;
            __movement.y = _verticalVelocity;
            _characterController.Move(__movement * __castedInput.deltaTime);
        }

        public override void SendClientInput(INetworkedClientInput input)
        {
            var __input = (CharacterInput) input;
            CmdSendInput(__input);
        }

        [Command(channel = Channels.DefaultUnreliable)]
        public void CmdSendInput(CharacterInput input)
        {
            ServerHandleInputReceived(input);
        }

        [ClientRpc(channel = Channels.DefaultUnreliable)]
        public void RpcSendState(CharacterState state)
        {
            ClientHandleStateReceived(state);
        }

        protected override void SendServerState(uint lastProcessedInputTick)
        {
            var __state = new CharacterState(_characterController.transform.position, _verticalVelocity, lastProcessedInputTick);
            RpcSendState(__state);
        }
    }
}