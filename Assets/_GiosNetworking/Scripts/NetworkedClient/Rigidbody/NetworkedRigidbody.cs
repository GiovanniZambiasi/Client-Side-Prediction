using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClientSidePrediction.RB
{
    public class NetworkedRigidbody : NetworkedClient<RigidbodyInput, RigidbodyState>
    {
        [Header("Rigidbody/References")]
        [SerializeField] Rigidbody _rigidbody = null;
        [Header("Rigidbody/Settings")]
        [SerializeField] float _speed = 10f;

        Scene _mainScene;
        Scene _idleScene;
        PhysicsScene _physicsScene;

        void Start()
        {
            _mainScene = gameObject.scene;

            GetOrCreateIdleScene();
            
            MoveToScene(_idleScene);
            
            Physics.autoSimulation = false;
        }

        public override void SetState(RigidbodyState state)
        {
            transform.position = state.position;
            transform.rotation = state.rotation;
            _rigidbody.velocity = state.velocity;
            _rigidbody.angularVelocity = state.angularVelocity;
        }

        public override void ProcessInput(RigidbodyInput input)
        {
            MoveToScene(_mainScene);
            
            var __force = new Vector3(input.movement.x, 0f, input.movement.y);
            __force *= _speed * input.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
            
            _physicsScene.Simulate(input.deltaTime);

            MoveToScene(_idleScene);
        }

        protected override RigidbodyState RecordState(uint lastProcessedInputTick)
        {
            return new RigidbodyState(transform.position, _rigidbody.velocity, _rigidbody.angularVelocity, transform.rotation, lastProcessedInputTick);
        }

        void GetOrCreateIdleScene()
        {
            _idleScene = SceneManager.GetSceneByName("Idle");
            
            if(!_idleScene.IsValid())
                CreateIdleScene();
        }

        void CreateIdleScene()
        {
            var __parameters = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
            _idleScene = SceneManager.CreateScene("Idle", __parameters);
        }

        void MoveToScene(Scene scene)
        {
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }
    }
}