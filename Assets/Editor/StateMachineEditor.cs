using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using FiniteStateMachine;


// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateMachineEditor : EditorWindow
{
    private List<StateNode> _stateNodes;
    private List<TransitionNode> _transitionNodes;
    private Dictionary<BaseState, Transition> _transitions;

    private GUIStyle _stateNodeStyle;
    private GUIStyle _selectedStateNodeStyle;
    private GUIStyle _transitionNodeStyle;
    private GUIStyle _selectedTransitionNodeStyle;
    private GUIStyle _inPointStyle;
    private GUIStyle _outPointStyle;
    
    private ConnectionPoint _selectedInPoint;
    private ConnectionPoint _selectedOutPoint;
    private string originPath = "Assets/Scriptable Objects/[TEST] editor";

    [MenuItem("Window/State Machine Editor")]
    private static void Init()
    {
        StateMachineEditor window = GetWindow<StateMachineEditor>();
        window.Show();
        window.titleContent = new GUIContent("State Machine Editor");
    }
    
    private void OnEnable()
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
    
    private void CreateExistingAssetNodes()
    {
        _transitions = new Dictionary<BaseState, Transition>();
        _stateNodes = new List<StateNode>();
        _transitionNodes = new List<TransitionNode>();
        
        //TODO: get states and create state node from each state
        string[] guids = AssetDatabase.FindAssets("t:BaseState",new[] { originPath });
        List<BaseState> states = 
            guids.Select(guid => (BaseState)AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(guid), typeof(BaseState))).ToList();
        Dictionary<BaseState, StateNode> nodes = new Dictionary<BaseState, StateNode>();
        foreach (BaseState state in states)
        {
            if (!nodes.TryGetValue(state, out StateNode node))
            {
                node = AddExistingStateNode(state);
                nodes.Add(state, node);
            }
            
            foreach (Transition transition in state.GetTransitions())
            {
                BaseState trueState = transition.TrueState;
                if (!nodes.TryGetValue(trueState, out StateNode n))
                {
                    n = AddExistingStateNode(trueState);
                    nodes.Add(trueState, n);
                }
                
                CreateTransitionNode(nodes[trueState].InPoint, nodes[state].OutPoint, transition, false);
                _transitions.TryAdd(trueState, transition);
            }
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

        _stateNodes.Add(new StateNode(mousePosition, 200, 50, 
            _stateNodeStyle, _selectedStateNodeStyle, 
            _inPointStyle,_outPointStyle, 
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode, this));
    }

    private void OnClickRemoveStateNode(StateNode state)
    {
        state.ClearTransitions();
        _stateNodes.Remove(state);
        DeleteState(state.BaseState);
    }
    
    private void OnClickAddTransitionNode(Vector2 mousePosition)
    {
        //TODO: popup editor, only create if transition passes safety check
        bool pass = false;
        if (pass)
        {
            CreateTransitionNode(_selectedInPoint, _selectedOutPoint, null, false);
        }
    }
    
    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        _selectedInPoint = inPoint;
        if (_selectedOutPoint != null)
            CheckCreateTransition();
    }
    
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        _selectedOutPoint = outPoint;
        if (_selectedInPoint != null)
            CheckCreateTransition();
    }

    private void CheckCreateTransition()
    {
        if (_selectedOutPoint.Node != _selectedInPoint.Node)
        {
            BaseState state = _selectedInPoint.Node.BaseState;
            Transition transition = _transitions?.GetValueOrDefault(state, null);
            CreateTransitionNode(_selectedInPoint, _selectedOutPoint,
                transition ? transition : CreateTransitionAsset($"to_{state.name}", state), true);
        }
        ClearConnectionSelection();
    }

    private void CreateTransitionNode(ConnectionPoint inPoint, ConnectionPoint outPoint, Transition transition, bool isNew)
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
        foreach (var state in _stateNodes)
        {
            if (!state.TransitionNodes.Contains(transitionNode)) continue;
            state.BaseState.DeleteTransition(transitionNode.Transition);
            state.RemoveTransitionNode(transitionNode);
        }
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
        string path = AssetDatabase.GenerateUniqueAssetPath($"{originPath}/unfiltered/{ending}");
        AssetDatabase.CreateAsset(state, path);
        AssetDatabase.SaveAssets();
        return state;
    }
    
    public Transition CreateTransitionAsset(string assetName, BaseState to)
    {
        Transition transition = ScriptableObject.CreateInstance<FiniteStateMachine.Transition>();
        
        _transitions ??= new Dictionary<BaseState, Transition>();
        _transitions.Add(to, transition);

        transition.TrueState = to;

        string ending = $"{assetName}.asset";
        string path = AssetDatabase.GenerateUniqueAssetPath($"{originPath}/transitions/{ending}");
        AssetDatabase.CreateAsset(transition, path);
        AssetDatabase.SaveAssets();
        return transition;
    }
    
    //TODO: delete assets
    private void DeleteState(BaseState state)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(state));
    }
}