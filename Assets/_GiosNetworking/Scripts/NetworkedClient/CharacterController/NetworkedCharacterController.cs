using UnityEngine;

namespace ClientSidePrediction.CC
{
    public class NetworkedCharacterController : NetworkedClient<CharacterControllerInput, CharacterControllerState>
    {
        [Header("CharacterController/References")]
        [SerializeField] CharacterController _characterController = null;
        [Header("CharacterController/Settings")]
        [SerializeField] float _speed = 10f;
        float _verticalVelocity = 0f; 

        public override void SetState(CharacterControllerState state)
        {
            _characterController.enabled = false;
            _characterController.transform.position = state.position;
            _verticalVelocity = state.verticalVelocity;
            _characterController.enabled = true;
        }

        public override void ProcessInput(CharacterControllerInput input)
        {
            var __movement = new Vector3(input.input.x, 0f, input.input.y);
            __movement = Vector3.ClampMagnitude(__movement, 1f) * _speed;
            if (!_characterController.isGrounded)
                _verticalVelocity += Physics.gravity.y * input.deltaTime;
            else
                _verticalVelocity = Physics.gravity.y;
            __movement.y = _verticalVelocity;
            _characterController.Move(__movement * input.deltaTime);
        }

        protected override CharacterControllerState RecordState(uint lastProcessedInputTick)
        {
            return new CharacterControllerState(_characterController.transform.position, _verticalVelocity, lastProcessedInputTick);   
        }
    }
}