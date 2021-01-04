using UnityEngine;

namespace ClientSidePrediction.RB
{
    public class NetworkedRigidbody : NetworkedClient<RigidbodyInput, RigidbodyState>
    {
        [Header("Rigidbody/References")]
        [SerializeField] Rigidbody _rigidbody = null;
        [Header("Rigidbody/Settings")]
        [SerializeField] float _speed = 10f;

        PhysicsScene _physicsScene;
        
        void Start()
        {
            var __scene = gameObject.scene;
            
            _physicsScene = __scene.GetPhysicsScene();

            Physics.autoSimulation = false;
            
            //_rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public override void SetState(RigidbodyState state)
        {
            _rigidbody.position = state.position;
            _rigidbody.velocity = state.velocity;
            _rigidbody.rotation = state.rotation;
            _rigidbody.angularVelocity = state.angularVelocity;
        }

        public override void ProcessInput(RigidbodyInput input)
        {
            var __force = new Vector3(input.movement.x, 0f, input.movement.y);
            __force *= _speed * input.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
            
            _physicsScene.Simulate(input.deltaTime);
        }

        protected override RigidbodyState RecordState(uint lastProcessedInputTick)
        {
            return new RigidbodyState(_rigidbody.position, _rigidbody.velocity, _rigidbody.angularVelocity, _rigidbody.rotation, lastProcessedInputTick);
        }
    }
}