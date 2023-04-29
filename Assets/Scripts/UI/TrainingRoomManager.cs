using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Collision")]
        [SerializeField] private GameObject _spritePrefab;
        [SerializeField] private GameObject _linePrefab;
        [SerializeField] private GameObject _collidersParent;
        [SerializeField] private Color _hurtColor;
        [SerializeField] private Color _hitColor;
        [SerializeField] private Color _parryColor;
        [SerializeField] private float _lrWidth;
        
        [Header("Animators")]
        [SerializeField] private Animator _infiniteExAnim;
        [SerializeField] private Animator _colliderAnim;
        [SerializeField] private Animator _inputsAnim;
        
        private bool _infiniteEx;
        private bool _showInputs;
        private bool _showColliders;

        private FavorManager _favorManager;
        private Dictionary<Collider, SpriteRenderer> _fillPair;
        private Dictionary<Collider, LineRenderer> _wiredPair;
        
        private Dictionary<string, Sprite> _inputSprites;
        private Queue<GameObject> _inputQueue = new Queue<GameObject>();
        private static readonly int On = Animator.StringToHash("On");

        private void Awake()
        {
            _inputSprites = new Dictionary<string, Sprite>();
            _favorManager = FindObjectOfType<FavorManager>();
            
            // foreach (var pair in _cmmdListReference.actionSpritePairs)
            // {
            //     _inputSprites.TryAdd(pair.action, pair.sprite);
            // }
            //
            _fillPair = new Dictionary<Collider, SpriteRenderer>();
            _wiredPair = new Dictionary<Collider, LineRenderer>();
        }

        private void Start()
        {
            HandleInfiniteEx();
            SubscribeToInputs();
            CreateColliders();
        }

        private void Update()
        {
            if (!_showColliders) return;
            foreach (var pair in _fillPair)
            {
                Vector3 size = pair.Key.gameObject.transform.lossyScale;
                pair.Value.enabled = pair.Key.enabled;
                pair.Value.transform.localScale = size;
                pair.Value.transform.position = pair.Key.transform.position;
            }

            foreach (var pair in _wiredPair)
            {
                Bounds bounds = pair.Key.bounds;
                Vector3 bl = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z);
                Vector3 br = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z);
                Vector3 tl = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z);
                Vector3 tr = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z);
                pair.Value.SetPosition(0, bl);
                pair.Value.SetPosition(1, br);
                pair.Value.SetPosition(2, tr);
                pair.Value.SetPosition(3, tl);
                pair.Value.SetPosition(4, bl);
            }
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

        public void ResetFavor()
        {
            _favorManager.ResetFavorMeter();
        }

        public void ToggleDebugCollision()
        {
            _showColliders = !_showColliders;
            foreach (var lr in _wiredPair.Values)
            {
                lr.enabled = _showColliders;
            }
            
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

        private void CreateColliders()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                //for each hurt, instantiate a new gameobject and set its colour
                Collider[] colliders = fighter.GetComponentsInChildren<Collider>();
                foreach (Collider box in colliders)
                {
                    int layer = box.gameObject.layer;
                    switch (layer)
                    {
                        case 7:
                        {
                            LineRenderer r = Instantiate(_linePrefab, _collidersParent.transform).GetComponent<LineRenderer>();
                            r.startColor = _hurtColor;
                            r.endColor = _hurtColor;
                            r.startWidth = _lrWidth;
                            r.endWidth = _lrWidth;
                            r.positionCount = 5;
                            r.enabled = false;
                            _wiredPair.TryAdd(box, r);
                            break;
                        }
                        case 6 or 13:
                        {
                            SpriteRenderer spriteRenderer = Instantiate(_spritePrefab, _collidersParent.transform).GetComponent<SpriteRenderer>();
                            spriteRenderer.color = layer == 6 ? _hitColor : _parryColor;
                            spriteRenderer.enabled = false;
                            _fillPair.TryAdd(box, spriteRenderer);
                            break;
                        }
                    }
                }
            }
        }
    }
}