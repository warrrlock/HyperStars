using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    //should it stay the same between rooms? or have to set it every time
    public class TrainingRoomManager: MonoBehaviour
    {
        [Header("info")]
        [SerializeField] private FightersManager _fightersManager;
        [SerializeField] private float _xDistanceBetweenPlayers;
        [SerializeField] private CmmdListReference _cmmdListReference;
        
        [Header("Room Positions")]
        [SerializeField] private Transform _leftRoomPosition;
        [SerializeField] private Transform _middleRoomPosition;
        [SerializeField] private Transform _rightRoomPosition;

        [Header("Input Prefabs")] 
        [SerializeField] private int _maxInputs;
        [SerializeField] private GameObject _inputParent;
        [SerializeField] private GameObject _inputContainer;
        [SerializeField] private GameObject _inputImage;

        [SerializeField] private Animator _infiniteExAnim;
        [SerializeField] private Animator _colliderAnim;
        [SerializeField] private Animator _inputsAnim;
        private bool _infiniteEx;
        private bool _showInputs;
        private bool _showColliders;
        private Dictionary<string, Sprite> _inputSprites;
        private Queue<GameObject> _inputQueue = new Queue<GameObject>();
        private static readonly int On = Animator.StringToHash("On");

        private void Awake()
        {
            _inputSprites = new Dictionary<string, Sprite>();
            foreach (var pair in _cmmdListReference.actionSpritePairs)
            {
                _inputSprites.TryAdd(pair.action, pair.sprite);
            }
        }

        private void Start()
        {
            HandleInfiniteEx();
            SubscribeToInputs();
        }

        private void OnDestroy()
        {
            UnsubscribeToInputs();
        }

        public void SetSwitchValues()
        {
            _inputsAnim.Play(_showInputs ? "On" : "Off");
            _inputsAnim.SetBool(On, _showInputs);
            
            _colliderAnim.Play(_showColliders ? "On" : "Off");
            _colliderAnim.SetBool(On, _showColliders);
            
            _infiniteExAnim.Play(_infiniteEx ? "On" : "Off");
            _infiniteExAnim.SetBool(On, _infiniteEx);
        }

        public void ToggleInfiniteEx()
        {
            _infiniteEx = !_infiniteEx;
            HandleInfiniteEx();
        }
        
        public void SetPositionLeft()
        {
            HandlePositionReset(_leftRoomPosition.position);
        }
        
        public void SetPositionMiddle()
        {
            HandlePositionReset(_middleRoomPosition.position);
        }
        
        public void SetPositionRight()
        {
            HandlePositionReset(_rightRoomPosition.position);
        }
        
        public void ToggleShowInputs()
        {
            _showInputs = !_showInputs;
        }
        
        private void HandleInfiniteEx()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.BaseStateMachine.InfiniteEx = _infiniteEx;
                if (_infiniteEx) fighter.SpecialMeterManager.FillBars();
                else fighter.SpecialMeterManager.ClearBars();
            }
        }

        private void HandlePositionReset(Vector3 position)
        {
            int side = -1;
            foreach (Fighter fighter in Services.Fighters)
            {
                Vector3 pos = new Vector3(position.x + (side * _xDistanceBetweenPlayers * 0.5f), position.y, position.z);
                fighter.ResetWithPosition(pos);
                side *= -1;
            }
        }

        private void SubscribeToInputs()
        {
            Fighter fighter = Services.Fighters[0];
            foreach (var pair in fighter.InputManager.Actions)
            { 
                pair.Value.perform += AddInputToPlayer0;
            }
        }
        
        private void UnsubscribeToInputs()
        {
            foreach (var pair in Services.Fighters[0].InputManager.Actions)
            {
                pair.Value.perform -= AddInputToPlayer0;
            }
        }
        
        private void AddInputToPlayer0(InputManager.Action a)
        {
            if (!_showInputs) return;
            Debug.Log(a.name);
            Fighter fighter = Services.Fighters[0];
            if (!_inputSprites.TryGetValue(a.name, out Sprite sprite)) return;
            
            GameObject container = Instantiate(_inputContainer, _inputParent.transform);
            Sprite direction = null;
            Sprite action = null;
            
            if (!sprite) { //no sprite, needs a particular sprite
                //MOVE: get direction, then sprite from move left or move right
                switch (a.name)
                {
                    case "Move":
                        direction = GetDirection(fighter.MovingDirection);
                        break;
                    case "Side Jump":
                        if (fighter.MovingDirection == Fighter.Direction.Left) direction = _inputSprites["Jump Left"];
                        else direction = _inputSprites["Jump Right"];
                        break;
                    case "Dash Left":
                        direction = _inputSprites["Move Left"];
                        action = _inputSprites["Dash"];
                        break;
                    case "Dash Right":
                        direction = _inputSprites["Move Right"];
                        action = _inputSprites["Dash"];
                        break;
                    case "Side Special":
                        direction = GetDirection(fighter.FacingDirection);
                        action = _inputSprites["Special"];
                        break;
                    case "CommandLight":
                        direction = GetDirection(fighter.FacingDirection);
                        action = _inputSprites["Light"];
                        break;
                    case "CommandMedium":
                        direction = GetDirection(fighter.FacingDirection);
                        action = _inputSprites["Medium"];
                        break;
                    default:
                        return;
                }
            }
            if (direction) Instantiate(_inputImage, container.transform).GetComponent<Image>().sprite = direction;
            if (action) Instantiate(_inputImage, container.transform).GetComponent<Image>().sprite = action;
            if (sprite) Instantiate(_inputImage, container.transform).GetComponent<Image>().sprite = sprite;
            _inputQueue.Enqueue(container);
            if (_inputQueue.Count >= _maxInputs)
            {
                _inputQueue.TryDequeue(out GameObject o);
                if (o) Destroy(o);
            }
        }

        private Sprite GetDirection(Fighter.Direction dir)
        {
            if (dir == Fighter.Direction.Left) return _inputSprites["Move Left"];
            return _inputSprites["Move Right"];
        }
    }
}