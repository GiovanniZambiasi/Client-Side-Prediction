using System.Collections;
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
        Scene _copy;
        PhysicsScene _physicsScene;

        void Start()
        {
            _mainScene = gameObject.scene;

            CreateCopiedScene();

            //GetOrCreateIdleScene();
            
            //MoveToScene(_idleScene);
            
            Physics.autoSimulation = false;
        }

        void CreateCopiedScene()
        {
            _copy = SceneManager.LoadScene(_mainScene.name,
                new LoadSceneParameters(LoadSceneMode.Additive, LocalPhysicsMode.Physics3D));

            _physicsScene = _copy.GetPhysicsScene();
            
            Debug.Log("Starting routine");
            
            StartCoroutine(ScenePrepareRoutine(_copy));
        }

        IEnumerator ScenePrepareRoutine(Scene scene)
        {
            yield return null;
            
            var __rootObjects = _copy.GetRootGameObjects();
            
            for (var i = 0; i < __rootObjects.Length; i++)
            {
                var __object = __rootObjects[i];
                var __lights = __object.GetComponentsInChildren<Light>(true);
                var __renderers = __object.GetComponentsInChildren<Renderer>(true);
                
                for (var j = __lights.Length - 1; j >= 0; j--)
                {
                    var __light = __lights[j];
                    Destroy(__light);
                }

                for (int j = __renderers.Length - 1; j >= 0; j--)
                {
                    var __renderer = __renderers[j];
                    Destroy(__renderer);
                }

                if (__object.TryGetComponent<AudioListener>(out var __listener))
                {
                    Destroy(__listener);
                }
            }

            Debug.Log("Moving rb");
            SceneManager.MoveGameObjectToScene(gameObject, _copy);
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
            //MoveToScene(_mainScene);
            
            var __force = new Vector3(input.movement.x, 0f, input.movement.y);
            __force *= _speed * input.deltaTime;
            _rigidbody.AddForce(__force, ForceMode.Impulse);
            
            _physicsScene.Simulate(input.deltaTime);

            //Debug.Log($"State before moving scenes {RecordState(LatestServerState.LastProcessedInputTick).ToString()}");
            
            //MoveToScene(_idleScene);
            
            //Debug.Log($"State after moving scenes {RecordState(LatestServerState.LastProcessedInputTick).ToString()}");
        }

        protected override RigidbodyState RecordState(uint lastProcessedInputTick)
        {
            return new RigidbodyState(_rigidbody.position, _rigidbody.velocity, _rigidbody.angularVelocity, _rigidbody.rotation, lastProcessedInputTick);
        }

        protected override void HandleOtherPlayerState(RigidbodyState state)
        {
            base.HandleOtherPlayerState(state);
            _physicsScene.Simulate(0.0001f);
        }

        /*void GetOrCreateIdleScene()
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
        }*/
        
        [ContextMenu("LogState")]
        void LogRBState()
        {
            LogState();
        }

        [ContextMenu("LongInputQueue")]
        void LogRBInput()
        {
            LogInputQueue();
        }
    }
}