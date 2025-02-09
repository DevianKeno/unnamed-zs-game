using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UZSG.UI
{
    [Serializable]
    public struct SceneTransitionOptions
    {
        public float Delay;
        public float Duration;
    }

    public struct TransitionOptions
    {

    }

    /// <summary>
    /// UI Manager for UZSG.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public enum TransitionType {
            FadeBlack
        }

        public bool EnableScreenAnimations = true;
        public float GlobalAnimationFactor = 0.5f;

        [SerializeField] SceneTransitionOptions sceneTransitionOptions;
        public SceneTransitionOptions SceneTransitionOptions => sceneTransitionOptions;
        
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
        Dictionary<string, Sprite> _icons = new();
        /// <summary>
        /// Returns true if any UI element that derives from the <c>Window</c> class is currently opened.
        /// </summary>
        public bool HasActiveWindow => _activeWindows.Count > 0;
        bool _isCursorVisible;
        public bool IsCursorVisible => _isCursorVisible;
        [SerializeField] Canvas canvas;
        /// <summary>
        /// General canvas for all UI elements.
        /// </summary>
        public Canvas Canvas => canvas;
        [SerializeField] Canvas healthBarCanvas;
        /// <summary>
        /// Canvas specifically for entity health bars.
        /// </summary>
        public Canvas HealthBarCanvas => healthBarCanvas;
        [SerializeField] Image screenBlack;


        #region UI Events
        
        /// <summary>
        /// Called whenever the cursor has its visibility toggled.
        /// </summary>
        public event Action<bool> OnCursorToggled;
        /// <summary>
        /// Called when any window is opened.
        /// </summary>
        public event Action<Window> OnAnyWindowOpened;
        /// <summary>
        /// Called when any window is closed.
        /// </summary>
        public event Action<Window> OnAnyWindowClosed;
        
        #endregion

        
        #region Inputs

        PlayerInput input;
        InputAction toggleCursorInput;
        InputAction closeWindowInput;

        #endregion

        Dictionary<string, GameObject> prefabsDict = new();
        Dictionary<string, AssetReference> addressableGuis = new();
        
        internal void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            Game.Console.LogInfo("Initializing UI...");
            LoadUIResources();
            InitializeIcons();

            input = Game.Main.MainInput;
            toggleCursorInput = input.actions.FindAction("Toggle Cursor");
            toggleCursorInput.performed += ToggleCursor;
            
            var uiActionMap = Game.Main.GetActionMap("UI");
            closeWindowInput = uiActionMap.FindAction("Close");
            closeWindowInput.performed += OnInputCloseWindow;
            
            uiActionMap.Enable();

            Game.Main.GetActionMap("Global").Enable();
        }

        void LoadUIResources()
        {
            Game.Console.LogInfo("Loading UI Prefabs...");
            foreach (GameObject element in Resources.LoadAll<GameObject>("Prefabs/UI"))
            {
                prefabsDict[element.name] = element;
            }
        }

        void InitializeIcons()
        {
            Game.Console.LogInfo("Loading Icons...");
            foreach (var sprite in Resources.LoadAll<Sprite>("Textures/Icons"))
            {
                _icons[sprite.name] = sprite;
            }
        }

        void ToggleCursor(InputAction.CallbackContext context)
        {
            SetCursorVisible(!_isCursorVisible);
        }
        
        void OnInputCloseWindow(InputAction.CallbackContext context)
        {
            CloseTopmostWindow();
        }

        
        #region Public methods
        
        public void Dim(float duration)
        {
            screenBlack.CrossFadeAlpha(0.25f, duration, false);
        }

        public void Undim(float duration)
        {
            screenBlack.CrossFadeAlpha(0f, duration, false);
        }

        public void CloseTopmostWindow()
        {
            if (_activeWindows.Count == 0) return;

            var topmostWindow = _activeWindows.Last();
            topmostWindow.Hide();
        }


        #region Transition methods
        // TransitionEffect transitionEffect;

        // public void PlayTransitionHalf(Action callback = null)
        // {
        //     transitionEffect = Create<TransitionEffect>("transition");
        //     transitionEffect.transform.SetParent(transitionCanvas.transform);
        //     transitionEffect.SetOptions(TransitionOptions);
        //     transitionEffect.PlayToHalf(callback);
        // }

        // public void PlayTransitionEnd(Action callback = null)
        // {
        //     transitionEffect.PlayToEnd(callback);
        // }
        #endregion

        public void ToggleCursorVisibility()
        {
            SetCursorVisible(!_isCursorVisible);
        }

        public void SetCursorVisible(bool visible)
        {
            if (visible)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }      
            
            _isCursorVisible = visible;
            Cursor.visible = visible;
            OnCursorToggled?.Invoke(visible);
        }

        public Sprite GetIcon(string id)
        {
            if (_icons.TryGetValue(id, out var sprite))
            {
                return sprite;
            }
            Game.Console.LogWarn($"There's no icon with an id of '{id}'");
            return null;
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
            if (window != null)
            {
                if (_activeWindows.Contains(window))
                {
                    _activeWindows.Remove(window);
                }
            }
        }

        GameObject Create(string prefabName, bool inSafeArea = true)
        {
            if (false == prefabsDict.ContainsKey(prefabName))
            {
                Game.Console.LogWarn($"UI Prefab '{prefabName}' does not exist!", true);
                return null;
            }

            var go = Instantiate(prefabsDict[prefabName], Canvas.transform);
            go.name = prefabName;
            return go;
        }
        
        public delegate void OnLoadAddressableElementCompleted<T>(T element);
        /// <summary>
        /// Create an instance of a UI prefab from an AssetReference.
        /// </summary>
        /// <typeparam name="T">Window script attach to the root.</typeparam>
        public async void CreateFromAddressableAsync<T>(AssetReference assetReference, bool show = true, OnLoadAddressableElementCompleted<T> callback = null) where T : UIElement
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                var msg = $"Unable to instantiate UI element, it does not exist";
                Game.Console.LogWarn(msg, true);
                return;
            }

            var go = Instantiate(handle.Result, Canvas.transform);
            if (false == go.TryGetComponent(out T element))
            {
                Game.Console.LogWarn($"'{assetReference.SubObjectName}' does not have a UIElement component! Discarding...", true);
                Destroy(go);
                return;
            }

            if (element is Window window)
            {
                window.OnOpened += () => { OnAnyWindowOpened?.Invoke(window); };
                window.OnClosed += () => { OnAnyWindowClosed?.Invoke(window); };
            }
            if (!show) element.Hide();

            callback?.Invoke(element);
            return;
        }

        /// <summary>
        /// Create an instance of a UI prefab.
        /// </summary>
        /// <typeparam name="T">Window script attach to the root.</typeparam>
        public T Create<T>(string prefabName, bool show = true) where T : UIElement
        {
            if (false == prefabsDict.ContainsKey(prefabName))
            {
                Game.Console.LogWarn($"Unable to create UI element, it does not exist!", true);
                return default;
            }

            var go = Instantiate(prefabsDict[prefabName], Canvas.transform);
            if (false == go.TryGetComponent(out T element))
            {
                Game.Console.LogWarn($"'{prefabName}' does not have a UIElement component! Discarding...", true);
                Destroy(go);
                return default;
            }

            go.name = prefabName;
            if (element is Window window)
            {
                window.OnOpened += () => { OnAnyWindowOpened?.Invoke(window); };
                window.OnClosed += () => { OnAnyWindowClosed?.Invoke(window); };
            }
            if (!show) element.Hide();
            return element;
        }

        public T Create<T>(string prefabName, Transform parent, bool show = true) where T : UIElement
        {            
            if (false == prefabsDict.ContainsKey(prefabName))
            {
                Game.Console.LogWarn($"Unable to create UI element '{prefabName}', it does not exist!", true);
                return default;
            }

            var go = Instantiate(prefabsDict[prefabName], parent);

            if (false == go.TryGetComponent(out T element))
            {
                Game.Console.LogWarn($"'{prefabName}' does not have a UIElement component! Discarding...", true);
                return default;
            }

            go.name = prefabName;
            if (element is Window window)
            {
                window.OnOpened += () => { OnAnyWindowOpened?.Invoke(window); };
                window.OnClosed += () => { OnAnyWindowClosed?.Invoke(window); };
            }
            if (!show) element.Hide();
            return element;
        }

        public T Create<T>(GameObject prefab, bool show = true) where T : UIElement
        {
            var go = Instantiate(prefab, Canvas.transform);
            if (false == go.TryGetComponent(out T element))
            {
                Game.Console.LogWarn($"'{prefab.name}' does not have a UIElement component! Discarding...", true);
                return default;
            }

            if (element is Window window)
            {
                window.OnOpened += () => { OnAnyWindowOpened?.Invoke(window); };
                window.OnClosed += () => { OnAnyWindowClosed?.Invoke(window); };
            }
            if (!show) element.Hide();
            return element;
        }

        /// <summary>
        /// Creates a UI blocker for a given UI element. Clicking outside the element's bounds invokes the Action onClick.
        /// Only spawns one blocker, and executes one action then destroys itself.
        /// </summary>
        /// <returns>The Blocker gameObject.</returns>
        public GameObject CreateBlocker(UIElement forElement = null, Action onClick = null)
        {
            var blocker = Create("Blocker");
            blocker.transform.SetParent(Canvas.transform);

            if (forElement != null)
            {
                blocker.transform.SetSiblingIndex((forElement as MonoBehaviour).transform.GetSiblingIndex());
                if (forElement is Window w)
                {
                    w.OnClosed += DestroyBlocker;
                }
                if (forElement is Panel p)
                {
                    p.OnClosed += DestroyBlocker;
                }
            }
            else
            {
                blocker.transform.SetAsLastSibling();
            }

            void DestroyBlocker()
            {
                Destroy(blocker);
            }

            blocker.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Destroy(blocker);
                onClick.Invoke();
            });

            return blocker;
        }

        public void DestroyElement(GameObject obj, float delay = 0f)
        {
            if (obj != null)
            {
                Destroy(obj, delay);
            }
        }

        #endregion
    }
}
