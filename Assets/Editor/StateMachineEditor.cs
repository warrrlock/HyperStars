using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;


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
    
    private ConnectionPoint _selectedInPoint;
    private ConnectionPoint _selectedOutPoint;
    private string _originPath = "Assets/Scriptable Objects/[TEST] editor/lisa";
    private string _characterPath = "Assets/Scriptable Objects/[TEST] editor/lisa";

    [MenuItem("Window/State Machine Editor")]
    private static void Init()
    {
        StateMachineEditor window = GetWindow<StateMachineEditor>();
        window.Show();
        window.titleContent = new GUIContent("State Machine Editor");
    }
    
    private void OnEnable()
    {
        CreateStyles();
        CreateExistingAssetNodes();
    }
    
    private void OnGUI()
    {
        DrawComponents();
        
        DrawConnectionLine(Event.current);
        
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
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
    
    private void CreateExistingAssetNodes()
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
            if (!_states.TryGetValue(state, out StateNode node))
            {
                node = AddExistingStateNode(state);
                _states.Add(state, node);
            }
            
            foreach (Transition transition in state.GetTransitions())
            {
                BaseState trueState = transition.TrueState;
                if (!_states.TryGetValue(trueState, out StateNode n))
                {
                    n = AddExistingStateNode(trueState);
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
    }

    private void DrawComponents()
    {
        DrawStateNodes(_stateNodes);
        DrawTransitionNodes(_transitionNodes);
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
    
    private void ProcessNodeEvents(Event e)
    {
        if (_transitionNodes != null)
        {
            for (int i = _transitionNodes.Count - 1; i >= 0; i--)
            {
                if (_transitionNodes[i].ProcessEvents(e))
                    GUI.changed = true;
            }
        }
        if (_stateNodes != null)
        {
            for (int i = _stateNodes.Count - 1; i >= 0; i--)
            {
                if (_stateNodes[i].ProcessEvents(e))
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
        genericMenu.ShowAsContext();
    }
    
    private StateNode AddExistingStateNode(BaseState state)
    {
        _stateNodes ??= new List<StateNode>();
        StateNode node = new StateNode(state,
            _stateNodeStyle, _selectedStateNodeStyle,
            _inPointStyle, _outPointStyle,
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this);
        _stateNodes.Add(node);
        return node;
    }
    
    private void OnClickAddStateNode(Vector2 mousePosition)
    {
        _stateNodes ??= new List<StateNode>();

        StateNode stateNode = new StateNode(mousePosition, 200, 50,
            _stateNodeStyle, _selectedStateNodeStyle,
            _inPointStyle, _outPointStyle,
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this);
        
        _stateNodes.Add(stateNode);
        _states.TryAdd(stateNode.BaseState, stateNode);
    }

    private void OnClickRemoveStateNode(StateNode state)
    {
        state.ClearTransitions();
        GUI.changed = true;
        _stateNodes.Remove(state);
        _states.Remove(state.BaseState);
        DeleteState(state.BaseState);
    }
    
    private void OnClickAddTransitionNode(Vector2 mousePosition)
    {
        //TODO: popup editor, only create if transition passes safety check
        DisplayTransitionPopup();
    }
    
    private static readonly Vector2 PopupSize = new Vector2(200, 150);
    private Rect _buttonRect;
    private void DisplayTransitionPopup()
    {
        Rect editor = position;
        Vector2 mid = new Vector2(editor.width, editor.height) / 2;
        _buttonRect = new Rect(mid - PopupSize / 2, Vector2.zero);
        PopupWindow.Show(_buttonRect, new TransitionPopup(PopupSize, this));
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
        
        BaseState from = outPoint.Node.BaseState;
        if (isNew && !from.GetTransitions().Contains(transition)) from.AddTransition(transition);
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
    }

    private void ClearConnectionSelection()
    {
        _selectedInPoint = null;
        _selectedOutPoint = null;
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
    
    //TODO: create assets
    public BaseState CreateStateAsset(string assetName)
    {
        BaseState state = ScriptableObject.CreateInstance<FiniteStateMachine.State>();
        string ending = $"{assetName}.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath($"{_characterPath}/unfiltered/{ending}");
        AssetDatabase.CreateAsset(state, path);
        AssetDatabase.SaveAssets();
        return state;
    }
    
    public Transition CreateTransitionAsset(BaseState to)
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
    
    //TODO: delete assets
    private void DeleteState(BaseState state)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(state));
        //deleting state should delete associated transition
        if (_transitions.TryGetValue(state, out Transition transition))
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(transition));
            _transitions.Remove(state);
            // foreach (var s in _states.Keys)
            // {
            //     SerializationUtility.ClearAllManagedReferencesWithMissingTypes(
            //         AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(s)));
            // }
        }
    }

    public void SetCharacterPath(string pathFromOrigin)
    {
        _characterPath = String.Concat(_originPath, "/", pathFromOrigin);
    }
}