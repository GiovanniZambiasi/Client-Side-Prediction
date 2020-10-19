using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClientSidePrediction
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] string _rbScene = "Scene_Rigidbody";
        [SerializeField] string _ccScene = "Scene_CharacterController";
        [Header("References")] 
        [SerializeField] Button _rbButton = null;
        [SerializeField] Button _ccButton = null;

        void Awake()
        {
            _rbButton.onClick.AddListener(HandleRBScene);
            _ccButton.onClick.AddListener(HandleCCScene);
        }

        void HandleCCScene() => LoadExample(_ccScene);

        void HandleRBScene() => LoadExample(_rbScene);

        void LoadExample(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}