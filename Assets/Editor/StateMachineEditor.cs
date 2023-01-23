using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FiniteStateMachine;
using Managers;
using Object = UnityEngine.Object;


// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateMachineEditor : EditorWindow
{
    private List<StateNode> _stateNodes;
    private Dictionary<BaseState, StateNode> _states;
    private List<TransitionNode> _transitionNodes;
    private Dictionary<BaseState, Transition> _transitions;
    public IReadOnlyDictionary<BaseState, StateNode> StateToNode => _states;

    private GUIStyle _stateNodeStyle;
    private GUIStyle _selectedStateNodeStyle;
    private GUIStyle _transitionNodeStyle;
    private GUIStyle _selectedTransitionNodeStyle;
    private GUIStyle _inPointStyle;
    private GUIStyle _outPointStyle;
    private Vector2 _stateNodeSize = new Vector2(150, 50);
    
    private ConnectionPoint _selectedInPoint;
    private ConnectionPoint _selectedOutPoint;

    private TransitionPopup _transitionPopup;
    private static readonly Vector2 PopupSize = new Vector2(200, 150);

    private Character _character;
    private CharacterManager.CharacterSelection _characterSelection;
    private CharacterManager _characterManager;
    private int _selectedCharacterIndex = 0;
    private readonly string[] _characterOptions = Enum.GetNames(typeof(CharacterManager.CharacterSelection))
        .Where(o => !o.Equals("None", StringComparison.OrdinalIgnoreCase)).ToArray();

    private List<StateNode> _filteredStateNodes;
    private List<TransitionNode> _filteredTransitionNodes;
    private FSMFilterDropdown _fsmFilterDropdown;
    private Rect _filterDropdownRect;
    private List<FSMFilter> _selectedFilters;
    public IReadOnlyList<FSMFilter> SelectedFilters => _selectedFilters;

    private enum StateType
    {
        Select, State, ComboState, InAirState, HurtState
    }
    private string _popupStateName = "";
    private StateType _popupSelection = StateType.Select;
    private StateNodePopup _stateNodePopup;
    
    private string _popupActionName = "";
    private FSMPopup _actionPopup;
    private AssetFinderWindow _assetFinder;
    
    private string _originPath = "Assets/Scriptable Objects/[TEST] editor";
    private string _characterPath;
    public string CharacterPath => _characterPath;

    
    [MenuItem("Window/State Machine Editor")]
    private static void Init()
    {
        StateMachineEditor window = GetWindow<StateMachineEditor>();
        window.Show();
        window.titleContent = new GUIContent("State Machine Editor");
    }
    
    private void OnEnable()
    {
        try
        {
            LoadCharacterManager();
            SetCharacter(CharacterManager.CharacterSelection.None);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            this.Close();
        }
        CreateStyles();
        CreateNodesForExistingAssets();
    }
    
    private void OnGUI()
    {
        DrawComponents();
        DrawConnectionLine(Event.current);
        
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }
    
    public bool CheckNewName(string newName, Type t)
    {
        if (newName.Equals(""))
        {
            Debug.LogError("Enter a name for the new asset.");
            return false;
        }
        if (CheckExists(newName, t))
        {
            Debug.LogError($"{t.Name}, {newName} already exists for this character. Please enter another name.");
            return false;
        }
        return true;
    }

    private void CreateStyles()
    {
        _stateNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12),
            alignment = TextAnchor.MiddleCenter,
        };
        
        _selectedStateNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12),
            alignment = TextAnchor.MiddleCenter,
        };

        _transitionNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12)
        };
        
        _selectedTransitionNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12)
        };
        
        _inPointStyle = new GUIStyle
        {
            normal ={background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D},
            active = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D},
            border = new RectOffset(4, 4, 12, 12)
        };

        _outPointStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D},
            active ={background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D},
            border = new RectOffset(4, 4, 12, 12)
        };
    }
    
    private void CreateNodesForExistingAssets()
    {
        _stateNodes = new List<StateNode>();
        _states = new Dictionary<BaseState, StateNode>();
        _transitions = new Dictionary<BaseState, Transition>();
        _transitionNodes = new List<TransitionNode>();
        
        string[] guids = AssetDatabase.FindAssets("t:BaseState",new[] { _characterPath });
        List<BaseState> states = 
            guids.Select(guid => (BaseState)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guid), typeof(BaseState))).ToList();
        
        foreach (BaseState state in states)
        {
            state.ReloadFilters(_character);
            if (!_states.TryGetValue(state, out StateNode node))
            {
                node = AddNodeForExistingState(state);
                _states.Add(state, node);
            }

            if (state.GetTransitions() == null) continue;
            foreach (Transition transition in state.GetTransitions())
            {
                BaseState trueState = transition.TrueState;
                if (!_states.TryGetValue(trueState, out StateNode n))
                {
                    n = AddNodeForExistingState(trueState);
                    _states.Add(trueState, n);
                }
                CreateTransitionNode(_states[state].OutPoint, _states[trueState].InPoint, transition, false);
                _transitions.TryAdd(trueState, transition);
            }
        }
        
        guids = AssetDatabase.FindAssets("t:Transition",new[] { _characterPath });
        List<Transition> transitions = 
            guids.Select(guid => (Transition)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guid), typeof(Transition))).ToList();
        foreach (var transition in transitions)
        {
            _transitions.TryAdd(transition.TrueState, transition);
        }

        _filteredStateNodes = _stateNodes;
        _filteredTransitionNodes = _transitionNodes;
    }
    
    private void FilterNodes()
    {
        HashSet<StateNode> stateNodes = new HashSet<StateNode>();
        HashSet<TransitionNode> transitionNodes = new HashSet<TransitionNode>();

        foreach (FSMFilter filter in _selectedFilters)
        {
            foreach (var sNode in _stateNodes)
            {
                if (sNode.ContainsFilter(filter)) stateNodes.Add(sNode);
                foreach (var tNode in sNode.TransitionNodes)
                    if (stateNodes.Contains(tNode.InPoint.Node) && stateNodes.Contains(tNode.OutPoint.Node)) transitionNodes.Add(tNode);
            }
        }

        _filteredStateNodes = stateNodes.ToList();
        _filteredTransitionNodes = transitionNodes.ToList();
        Repaint();
    }

    private void ClearFilters()
    {
        _selectedFilters.Clear();
        _filteredStateNodes = _stateNodes;
        _filteredTransitionNodes = _transitionNodes;
    }
    
    private void DrawComponents()
    {
        DrawTabs();
        DrawFilterDropdown();
        DrawActionFinder();
        DrawStateNodes(_filteredStateNodes);
        DrawTransitionNodes(_filteredTransitionNodes);
    }

    private void DrawTabs()
    {
        int index = _selectedCharacterIndex;
        _selectedCharacterIndex = GUILayout.Toolbar(_selectedCharacterIndex, _characterOptions);
        if (index != _selectedCharacterIndex)
        {
            SetCharacter(_characterOptions[_selectedCharacterIndex]);
            Repaint();
        }
    }
    
    private void DrawFilterDropdown()
    {
        EditorGUILayout.BeginHorizontal();
        if (EditorGUILayout.DropdownButton(new GUIContent("Filters"), FocusType.Passive, GUILayout.Width(100)))
        {
            PopupWindow.Show(_filterDropdownRect, CreateFilterDropdown());
        }
        if (Event.current.type == EventType.Repaint) _filterDropdownRect = GUILayoutUtility.GetLastRect();
        if (GUILayout.Button("clear", EditorStyles.miniButton, GUILayout.Width(50)))
            ClearFilters();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStateNodes(List<StateNode> nodes)
    {
        if (nodes == null) return;
        for(int i = 0; i < nodes.Count; i++)
            nodes[i].Draw();
    }
    
    private void DrawTransitionNodes(List<TransitionNode> nodes)
    {
        if (nodes == null) return;
        for(int i = 0; i < nodes.Count; i++)
            nodes[i].Draw();
    }
    
    private void DrawConnectionLine(Event e)
    {
        if (_selectedInPoint != null && _selectedOutPoint == null)
        {
            Handles.DrawBezier(
                _selectedInPoint.Rect.center,
                e.mousePosition,
                _selectedInPoint.Rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (_selectedOutPoint != null && _selectedInPoint == null)
        {
            Handles.DrawBezier(
                _selectedOutPoint.Rect.center,
                e.mousePosition,
                _selectedOutPoint.Rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    private void DrawActionFinder()
    {
        if (GUILayout.Button("Actions", GUILayout.Width(100)))
        {
            OpenAssetFinder<StateAction>(OpenAction);
        }
    }
    
    public void DrawGeneralButton(string label, Action action)
    {
        if (GUILayout.Button(label, GUILayout.Width(100))) action();
    }
    
    private void ProcessNodeEvents(Event e)
    {
        if (_filteredTransitionNodes != null)
        {
            for (int i = _filteredTransitionNodes.Count - 1; i >= 0; i--)
            {
                if (_filteredTransitionNodes[i].ProcessEvents(e))
                    GUI.changed = true;
            }
        }
        if (_filteredStateNodes != null)
        {
            for (int i = _filteredStateNodes.Count - 1; i >= 0; i--)
            {
                if (_filteredStateNodes[i].ProcessEvents(e))
                    GUI.changed = true;
            }
        }
    }

    private void ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                Selection.objects = null;
                ClearSelectionExceptState(null);
                break;
            case EventType.MouseUp:
                if (e.button == 1)
                    ProcessContextMenu(e.mousePosition);
                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                    OnDrag(e.delta);
                break;
        }
    }
    
    private void OnDrag(Vector2 delta)
    {
        if (_stateNodes != null)
        {
            for (int i = 0; i < _stateNodes.Count; i++)
                _stateNodes[i].Drag(delta);
        }

        GUI.changed = true;
    }
    
    private void ProcessContextMenu(Vector2 mousePosition)
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddStateNode(mousePosition));
        genericMenu.AddItem(new GUIContent("Add transition"), false, () => OnClickAddTransitionNode(mousePosition));
        CreateActionMenu(genericMenu, "Add action");
        genericMenu.ShowAsContext();
    }
    
    private StateNode AddNodeForExistingState(BaseState state)
    {
        _stateNodes ??= new List<StateNode>();
        StateNode node = new StateNode(state,
            _stateNodeStyle, _selectedStateNodeStyle,
            _inPointStyle, _outPointStyle,
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this);
        _stateNodes.Add(node);
        state.NodeInfo.rect.size = _stateNodeSize;
        return node;
    }
    
    private void OnClickAddStateNode(Vector2 mousePosition)
    {
        _stateNodes ??= new List<StateNode>();
        
        //TODO: showing naming parameter, and do not create asset if empty name
        if (_stateNodePopup) _stateNodePopup.Close();
        _stateNodePopup = StateNodePopup.ActivatePopup(new [] 
            {
                (Action)(() => DrawStateNaming("name")), DrawStateSelection,
                () => DrawGeneralButton("create", () => CreateState(mousePosition, _stateNodeSize)) 
            }, Vector2.zero, new Vector2(500,150),
            new StateNode(
                _stateNodeStyle, _selectedStateNodeStyle,
                _inPointStyle, _outPointStyle,
                OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this));
    }

    private void HandleCreateStateNode(StateNode stateNode)
    {
        if (!stateNode.BaseState)
        {
            Debug.Log("new state node not created");
            return;
        }
        _stateNodes.Add(stateNode);
        _states.Add(stateNode.BaseState, stateNode);
        _filteredStateNodes.Add(stateNode);
        Debug.Log($"Successfully created a new state, {stateNode.BaseState.name}");
    }

    private void OnClickRemoveStateNode(StateNode state)
    {
        state.ClearTransitions();
        GUI.changed = true;
        _stateNodes.Remove(state);
        _states.Remove(state.BaseState);
        _filteredStateNodes.Remove(state);
        DeleteStateAsset(state.BaseState);
    }
    
    private void OnClickAddTransitionNode(Vector2 mousePosition)
    {
        //TODO: popup editor, only create if transition passes safety check
        DisplayTransitionPopup();
    }
    
    private void DisplayTransitionPopup()
    {
        if (_transitionPopup) _transitionPopup.Close();
        _transitionPopup = TransitionPopup.CreateTransitionPopup(PopupSize, this);
    }

    public void CheckCreateTransition(ConnectionPoint outPoint, ConnectionPoint inPoint)
    {
        if (outPoint.Node != inPoint.Node)
        {
            BaseState state = inPoint.Node.BaseState;
            Transition transition = _transitions?.GetValueOrDefault(state, null);
            CreateTransitionNode(outPoint, inPoint,
                transition ? transition : CreateTransitionAsset(state), true);
        }
        ClearConnectionSelection();
    }

    private void CreateTransitionNode(ConnectionPoint outPoint, ConnectionPoint inPoint, Transition transition, bool isNew)
    {
        _transitionNodes ??= new List<TransitionNode>();
        if (inPoint.Node.TransitionNodes.Count(t => t.InPoint == inPoint && t.OutPoint == outPoint) > 0) return;
        
        TransitionNode transitionNode = new TransitionNode(_transitionNodeStyle, _selectedTransitionNodeStyle,
            inPoint, outPoint, OnClickRemoveConnection, this, transition);
        
        inPoint.Node.AddTransitionNode(transitionNode);
        outPoint.Node.AddTransitionNode(transitionNode);
        _transitionNodes.Add(transitionNode);
        _filteredTransitionNodes?.Add(transitionNode);
        
        BaseState from = outPoint.Node.BaseState;
        if (isNew && !from.GetTransitions().Contains(transition)) from.AddTransition(transition);
    }
    
    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        _selectedInPoint = inPoint;
        if (_selectedOutPoint != null)
            CheckCreateTransition(_selectedOutPoint, _selectedInPoint);
    }
    
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        _selectedOutPoint = outPoint;
        if (_selectedInPoint != null)
            CheckCreateTransition(_selectedOutPoint, _selectedInPoint);
    }
    
    private void ClearConnectionSelection()
    {
        _selectedInPoint = null;
        _selectedOutPoint = null;
    }
    
    private void OnClickRemoveConnection(TransitionNode transitionNode)
    {
        _transitionNodes.Remove(transitionNode);
        StateNode state;
        //from
        state = transitionNode.OutPoint.Node;
        state.BaseState.DeleteTransition(transitionNode.Transition);
        state.RemoveTransitionNode(transitionNode);
        //to
        state = transitionNode.InPoint.Node;
        state.RemoveTransitionNode(transitionNode);

        _filteredTransitionNodes.Remove(transitionNode);
    }

    public void ClearSelectionExceptState(StateNode node)
    {
        foreach (StateNode n in _stateNodes)
            if (n != node) n?.Deselect();
        foreach (TransitionNode n in _transitionNodes)
            n?.Deselect();
    }
    
    public void ClearSelectionExceptTransition(TransitionNode node)
    {
        foreach (StateNode n in _stateNodes)
            n?.Deselect();
        foreach (TransitionNode n in _transitionNodes)
            if (n != node) n?.Deselect();
        
    }

    private void OnClickAddAction(Type type)
    {
        _actionPopup = FSMPopup.ActivatePopup(
            new []
            {
                (Action)(() => DrawActionNaming("action name")),
                () => DrawGeneralButton("create", () => CreateAction(type))
            }, 
            new Vector2(300, 150), "Create action");
    }
    
    private void OpenAssetFinder<T>(Action<T> action) where T : class
    {
        if (_assetFinder) _assetFinder.Close();
        _assetFinder = AssetFinderWindow.CreateFinderWindow(Vector2.zero, new Vector2(500, 300),
            _characterPath, typeof(T), (Object o) => action(o as T));
    }

    private void OpenAction(StateAction action)
    {
        Selection.activeObject = action;
    }

    private void CreateAction(Type type)
    {
        _popupActionName = _popupActionName.Trim();
        if (!CheckNewName(_popupActionName, typeof(StateAction))) return;
        StateAction action = CreateStateActionAsset(_popupActionName, type);
        _popupActionName = "";
        _actionPopup.Close();
    }
    
    private void DrawActionNaming(string label)
    {
        EditorGUILayout.BeginVertical();
        _popupActionName = EditorGUILayout.TextField(label, _popupActionName);
        EditorGUILayout.EndVertical();
    }

    private void CreateActionMenu(GenericMenu menu, string path)
    {
        Type baseType = typeof(FiniteStateMachine.StateAction);
        Assembly assembly = Assembly.GetAssembly(baseType);
        var subTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
        foreach (Type t in subTypes)
            menu.AddItem(new GUIContent($"{path}/{t.Name}"), false, () => OnClickAddAction(t));
    }

    private void DrawStateNaming(string label)
    {
        EditorGUILayout.BeginVertical();
        _popupStateName = EditorGUILayout.TextField(label, _popupStateName);
        EditorGUILayout.EndVertical();
    }

    private void DrawStateSelection()
    {
        _popupSelection = (StateType)EditorGUILayout.EnumPopup("State type", _popupSelection);
    }

    private void CreateState(Vector2 nodePosition, Vector2 size)
    {
        _popupStateName = _popupStateName.Trim();
        if (!CheckNewName(_popupStateName, typeof(BaseState))) return;
        
        switch (_popupSelection)
        {
            case StateType.ComboState:
                _stateNodePopup.StateNode.BaseState = CreateStateAsset<ComboState>(_popupStateName);
                break;
            case StateType.InAirState:
                _stateNodePopup.StateNode.BaseState = CreateStateAsset<InAirState>(_popupStateName);
                break;
            case StateType.HurtState:
                _stateNodePopup.StateNode.BaseState = CreateStateAsset<HurtState>(_popupStateName);
                break;
            default:
                _stateNodePopup.StateNode.BaseState = CreateStateAsset<FiniteStateMachine.State>(_popupStateName);
                break;
        }
        _stateNodePopup.StateNode.BaseState.NodeInfo.rect = new Rect(nodePosition, size);
        
        HandleCreateStateNode(_stateNodePopup.StateNode);
        ResetStatePopup(nodePosition);
        SaveChanges();
    }

    private void ResetStatePopup(Vector2 mousePosition)
    {
        _stateNodePopup.StateNode = new StateNode(mousePosition, 200, 50,
            _stateNodeStyle, _selectedStateNodeStyle,
            _inPointStyle, _outPointStyle,
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this);
        _popupStateName = "";
        _popupSelection = StateType.Select;
    }

    private bool CheckExists(string checkName, Type t)
    {
        if (t == typeof(BaseState)) return (_states.Keys.Count(s => s.name.Equals(checkName, StringComparison.OrdinalIgnoreCase)) > 0);
        return false;
    }

    private void SetCharacterPath(string pathFromOrigin)
    {
        _characterPath = String.Concat(_originPath, "/", pathFromOrigin);
    }

    private void SetCharacterTab(string selected)
    {
        _selectedCharacterIndex = Array.IndexOf(_characterOptions, selected);
    }

    private void SetCharacter(string characterString)
    {
        foreach (CharacterManager.CharacterSelection enumObj in Enum.GetValues(typeof(CharacterManager.CharacterSelection)))
        {
            if (!characterString.Equals(enumObj.ToString())) continue;
            SetCharacter(enumObj);
        }
    }

    private void SetCharacter(CharacterManager.CharacterSelection selection)
    {
        if (_characterManager.Characters.Count <= 0)
            throw new Exception("no characters exist in the character manager." +
                                "Please ensure there is at least one character in existence.");
        
        CharacterManager.CharacterSelection temp = selection;
        selection = selection == CharacterManager.CharacterSelection.None
            ? _characterManager.Characters.First().Key
            : selection;
        
        if (!_characterManager.Characters.TryGetValue(selection, out _character))
            throw new Exception($"character {selection.ToString()} does not exist in the manager." +
                                "Please ensure there is such a selection in existence.");
        
        _characterSelection = selection;
        SetCharacterPath(_character.name);
        if (temp == CharacterManager.CharacterSelection.None)
            SetCharacterTab(selection.ToString());
        CreateNodesForExistingAssets();
        _selectedFilters = new List<FSMFilter>();
    }

    private FSMFilterDropdown CreateFilterDropdown()
    {
        return new FSMFilterDropdown(_character.Filters, new FSMFilterComparer(), 
            AddFilter, RemoveFilter, this);
    }

    private void AddFilter(FSMFilter f)
    {
        _selectedFilters.Add(f);
        _selectedFilters.Sort(new FSMFilterComparer());
        FilterNodes();
    }

    private void RemoveFilter(FSMFilter f)
    {
        _selectedFilters.Remove(f);
        if (_selectedFilters.Count > 0)
            FilterNodes();
        else
        {
            _filteredStateNodes = _stateNodes;
            _filteredTransitionNodes = _transitionNodes;
        }
    }

    private StateAction CreateStateActionAsset(string assetName, Type type)
    {
        StateAction action = ScriptableObject.CreateInstance(type) as StateAction;
        action.character = _characterSelection;

        string ending = $"{assetName}.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath($"{_characterPath}/actions/{ending}");
        AssetDatabase.CreateAsset(action, path);
        AssetDatabase.SaveAssets();
        return action;
    }
    
    private BaseState CreateStateAsset<T>(string assetName) where T : BaseState
    {
        BaseState state = ScriptableObject.CreateInstance<T>();
        state.character = _characterSelection;

        string ending = $"{assetName}.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath($"{_characterPath}/unfiltered/{ending}");
        AssetDatabase.CreateAsset(state, path);
        AssetDatabase.SaveAssets();
        
        foreach (FSMFilter filter in _selectedFilters)
            state.AddFilter(filter, _characterPath);
        return state;
    }

    private Transition CreateTransitionAsset(BaseState to)
    {
        string assetName = $"to_{to.name}";
        Transition transition = ScriptableObject.CreateInstance<FiniteStateMachine.Transition>();
        
        _transitions ??= new Dictionary<BaseState, Transition>();
        _transitions.TryAdd(to, transition); //TODO: error if cannot add! uses should be in try catch

        transition.TrueState = to;

        string ending = $"{assetName.Replace(' ', '_')}.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath($"{_characterPath}/transitions/{ending}");
        AssetDatabase.CreateAsset(transition, path);
        AssetDatabase.SaveAssets();
        return transition;
    }
    
    private void DeleteStateAsset(BaseState state)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(state));
        //deleting state should delete associated transition
        if (_transitions.TryGetValue(state, out Transition transition))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(transition));
            _transitions.Remove(state);
        }
    }
    
    private void LoadCharacterManager()
    {
        _characterManager = (CharacterManager)AssetDatabase.LoadAssetAtPath($"{_originPath}/[TEST]character manager.asset", typeof(CharacterManager));
        if (!_characterManager)
            throw new Exception("no character manager exits at " +
                                $"{_originPath}/[TEST]character manager. " +
                                "Please ensure the character manager has not been moved or deleted");
    }
}