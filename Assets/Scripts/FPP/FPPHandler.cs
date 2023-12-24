using UnityEngine;
using UnityEditor.Animations;
using UZSG.Player;
using UZSG.Systems;
using UZSG.Items;

namespace UZSG.FPP
{
    public enum FPPStates { None, Equip, Idle, Run, Primary, Secondary, Hold, Dequip }

    /// <summary>
    /// Represents objects that are visible in first-person perspective.
    /// </summary>
    public interface IFPPVisible
    {
        public GameObject FPPModel { get; }
        public AnimatorController Controller { get; }
        public FPPAnimations Anims { get; }
    }

    /// <summary>
    /// Handles the functionalities of the first-person view.
    /// </summary>
    public class FPPHandler : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] PlayerCore _player;
        StateMachine<FPPStates> _sm;
        FPPAnimatable _anim;
        WeaponData _equippedData;
        float transitionDuration = 0.1f;

        void Awake()
        {
            _camera = GetComponent<Camera>();
            _sm = GetComponent<StateMachine<FPPStates>>();
        }

        void Start()
        {
            _sm.InitialState = _sm.States[FPPStates.None];
            _sm.OnStateChanged += StateChangedCallback;
            _player.sm.OnStateChanged += PlayerStateChangedCallback;
            UI.Cursor.Hide();
        }

        void PlayerStateChangedCallback(object sender, StateMachine<PlayerStates>.StateChangedArgs e)
        {
            if (_sm.States.TryGetValue((FPPStates) e.Next, out State<FPPStates> nextState))
            {
                _sm.ToState(nextState);
            }
        }

        void StateChangedCallback(object sender, StateMachine<FPPStates>.StateChangedArgs e)
        {
            if (e.Next == FPPStates.Equip)
            {
                _anim.Play("Equip", 0f);
            }
        }

        /// <summary>
        /// Cache model and data.
        /// </summary>
        public void Load(IFPPVisible obj)
        {            
            GameObject go = Instantiate(obj.FPPModel, _camera.transform);
            _anim = go.GetComponent<FPPAnimatable>();
            _anim.Load(obj);
        }
    }
}
