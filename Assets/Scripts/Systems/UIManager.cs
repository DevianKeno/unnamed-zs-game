using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using UZSG.Systems;

namespace UZSG.UI
{
    /// <summary>
    /// UI Manager for UZSG.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Serializable]
        public struct PrefabId
        {
            public string Id;
            public GameObject Prefab;
        }

        [Serializable]
        public struct IconId
        {
            public string Id;
            public Sprite Sprite;
        }

        public bool EnableScreenAnimations = true;
        [SerializeField] Color interactionColor;
        public Color InteractionColor
        {
            get { return interactionColor; }
            set
            {
                var renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
                var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
                List<ScriptableRendererFeature> features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
            }
        }

        bool _isInitialized;
        List<Window> _activeWindows = new();
        Window _currentWindow;

        [SerializeField] Canvas canvas;
        public Canvas Canvas => canvas;

        bool _isCursorVisible;
        public bool IsCursorVisible => _isCursorVisible;


        #region UI Events
        public event Action<bool> OnCursorToggled;
        
        #endregion

        
        #region Inputs
        PlayerInput input;
        InputAction toggleCursorInput;
        InputAction closeCurrentWindowInput;

        #endregion

        Dictionary<string, GameObject> prefabsDict = new();

        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            Game.Console.Log("Initializing UI...");
            InitializeUIPrefabs();

            input = Game.Main.MainInput;
            toggleCursorInput = input.actions.FindAction("Toggle Cursor");
            closeCurrentWindowInput = input.actions.FindAction("Close Current Window");
            toggleCursorInput.performed += ToggleCursor;

            input.actions.FindActionMap("Global").Enable();
        }

        void InitializeUIPrefabs()
        {
            foreach (GameObject element in Resources.LoadAll<GameObject>("Prefabs/UI"))
            {
                prefabsDict[element.name] = element;
            }
        }

        void ToggleCursor(InputAction.CallbackContext context)
        {
            ToggleCursor();
        }

        public void ToggleCursor()
        {
            ToggleCursor(!_isCursorVisible);
        }

        public void ToggleCursor(bool enabled)
        {
            if (enabled)            
                Cursor.lockState = CursorLockMode.None;
            else            
                Cursor.lockState = CursorLockMode.Locked;            
            
            _isCursorVisible = enabled;
            Cursor.visible = enabled;
            OnCursorToggled?.Invoke(enabled);
        }

        internal void AddToActiveWindows(Window window)
        {
            if (window != null)
            {
                _activeWindows.Add(window);
            }
        }

        internal void RemoveFromActiveWindows(Window window)
        {
            if (window != null && _activeWindows.Contains(window))
            {
                _activeWindows.Remove(window);
            }
        }

        internal void SetCurrentWindow(Window window)
        {
            if (window == null) return;
            if (_currentWindow == window) return;

            if (_currentWindow != null)
            {
                _currentWindow.Hide();
            }
            foreach (var w in _activeWindows)
            {
                w.Hide();
            }
            _currentWindow = window;
        }

        public GameObject Create(string prefabName, bool inSafeArea = true)
        {
            if (prefabsDict.ContainsKey(prefabName))
            {
                var go = Instantiate(prefabsDict[prefabName], Canvas.transform);
                go.name = prefabName;
                return go;
            }

            var msg = $"UI Prefab '{prefabName}' does not exist.";
            Game.Console.Log(msg);
            Debug.LogWarning(msg);
            return null;
        }

        public GameObject Create(string prefabName, Transform parent, bool inSafeArea = true)
        {
            if (prefabsDict.ContainsKey(prefabName))
            {
                var go = Instantiate(prefabsDict[prefabName], parent);
                go.name = prefabName;
                return go;
            }

            var msg = $"UI Prefab '{prefabName}' does not exist.";
            Game.Console.Log(msg);
            Debug.LogWarning(msg);
            return null;
        }

        public T Create<T>(string prefabName, bool show = true) where T : Window
        {            
            if (prefabsDict.ContainsKey(prefabName))
            {
                var go = Instantiate(prefabsDict[prefabName], Canvas.transform);
                go.name = prefabName;

                if (go.TryGetComponent(out T element))
                {
                    if (!show) element.Hide();
                    return element;
                }
                return default;
            }

            var msg = $"Unable to create UI element, it does not exist";
            Game.Console.Log(msg);
            Debug.LogWarning(msg);
            return default;
        }

        public T Create<T>(string prefabName, Transform parent, bool show = true) where T : Window
        {            
            if (prefabsDict.ContainsKey(prefabName))
            {
                var go = Instantiate(prefabsDict[prefabName], parent);
                go.name = prefabName;

                if (go.TryGetComponent(out T element))
                {
                    if (!show) element.Hide();
                    return element;
                }
                return default;
            }

            var msg = $"Unable to create UI element, it does not exist";
            Game.Console.Log(msg);
            Debug.LogWarning(msg);
            return default;
        }

        public GameObject CreateBlocker(IUIElement forElement = null, Action onClick = null)
        {
            var blocker = Create("Blocker");
            blocker.transform.SetParent(Canvas.transform);

            if (forElement != null)
            {
                blocker.transform.SetSiblingIndex((forElement as MonoBehaviour).transform.GetSiblingIndex());
            }
            else
            {
                blocker.transform.SetAsLastSibling();
            }

            blocker.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Destroy(blocker);
                onClick.Invoke();
            });

            return blocker;
        }
    }
}
