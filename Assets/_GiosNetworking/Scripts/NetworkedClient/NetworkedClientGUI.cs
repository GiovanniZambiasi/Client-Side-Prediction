using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public abstract class NetworkedClientGUI : MonoBehaviour
    {
        [Header("Reference")] 
        [SerializeField] GameObject _networkedClient = null;
        [SerializeField] GameObject _phantomPrefab = null;
        [Header("Settings")] 
        [SerializeField] Vector2Int _windowSize = new Vector2Int(225, 225);
        [SerializeField] Vector2 _windowViewportPosition = new Vector2(1, 0);

        INetworkedClient _client;
        NetworkIdentity _clientIdentity;
        GameObject _phantom;
        Camera _camera;
        int _targetFPS = 64;
        bool _drawPhantom = false;

        void Awake()
        {
            _client = _networkedClient.GetComponent<INetworkedClient>();
            _clientIdentity = _networkedClient.GetComponent<NetworkIdentity>();
            
            _targetFPS = NetworkManager.singleton.serverTickRate;
        }

        void OnDestroy()
        {
            if(_phantom!=null)
                Destroy(_phantom);
        }

        void OnGUI()
        {
            if (!_clientIdentity.isClient || !_clientIdentity.hasAuthority)
                return;

            if (_camera == null)
                _camera = Camera.main;

            var __screenPoint = _camera.ViewportToScreenPoint(_windowViewportPosition);

            __screenPoint.x = Mathf.Clamp(__screenPoint.x, 0, Screen.width - _windowSize.x);
            __screenPoint.y = Mathf.Clamp(__screenPoint.y, 0, Screen.height - _windowSize.y);

            var __rect = new Rect(__screenPoint, _windowSize);

            GUI.Box(__rect, string.Empty);

            GUILayout.BeginArea(__rect);
            DrawStats();
            GUILayout.Space(10);
            DrawSettings();
            GUILayout.EndArea();
        }

        void DrawSettings()
        {
            GUILayout.Label("Settings");

            if (!_clientIdentity.isServer)
            {
                _drawPhantom = GUILayout.Toggle(_drawPhantom, "Draw phantom");

                if (_drawPhantom)
                    DrawPhantom();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Target FPS:");

            var __targetFPS = GUILayout.TextField(_targetFPS.ToString());
            if (int.TryParse(__targetFPS, out var __result))
                _targetFPS = __result;

            if (GUILayout.Button("Apply"))
                Application.targetFrameRate = _targetFPS;

            GUILayout.EndHorizontal();
        }

        void DrawPhantom()
        {
            if (_phantom == null)
            {
                if (_phantomPrefab == null)
                    return;
                _phantom = Instantiate(_phantomPrefab);
            }

            SetPhantomState(_phantom, _client.LatestServerState);
        }

        protected virtual void DrawStats()
        {
            GUILayout.Label("Stats");
            GUILayout.Label($"Current Tick: {_client.CurrentTick.ToString()}");
            GUILayout.Label($"Delta Time: {(1f / NetworkManager.singleton.serverTickRate).ToString()}");
            GUILayout.Label($"Rtt: {NetworkTime.rtt.ToString()}");
        }

        protected abstract void SetPhantomState(GameObject phantom, INetworkedClientState state);
    }
}