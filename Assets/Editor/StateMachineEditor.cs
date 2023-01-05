using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FiniteStateMachine;


// based off of https://oguzkonya.com/creating-node-based-editor-unity/
public class StateMachineEditor : EditorWindow
{
    private List<StateNode> _stateNodes;
    private List<TransitionNode> _transitionNodes;
    
    private GUIStyle _stateNodeStyle;
    private GUIStyle _selectedStateNodeStyle;
    private GUIStyle _transitionNodeStyle;
    private GUIStyle _inPointStyle;
    private GUIStyle _outPointStyle;
    
    private ConnectionPoint _selectedInPoint;
    private ConnectionPoint _selectedOutPoint;

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
            border = new RectOffset(12, 12, 12, 12)
        };
        
        _selectedStateNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D},
            border = new RectOffset(12, 12, 12, 12)
        };


        _transitionNodeStyle = new GUIStyle
        {
            normal = {background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D},
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
    
    private void OnGUI()
    {
        DrawComponents();
        
        DrawConnectionLine(Event.current);
        
        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
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
    
    private void OnClickAddStateNode(Vector2 mousePosition)
    {
        _stateNodes ??= new List<StateNode>();

        _stateNodes.Add(new StateNode(mousePosition, 200, 50, 
            _stateNodeStyle, _selectedStateNodeStyle, 
            _inPointStyle,_outPointStyle, 
            OnClickInPoint, OnClickOutPoint, OnClickRemoveStateNode));
    }

    private void OnClickRemoveStateNode(StateNode state)
    {
        state.ClearTransitions();
        _stateNodes.Remove(state);
        DeleteState(null); //TODO: fill
    }
    
    private void OnClickAddTransitionNode(Vector2 mousePosition)
    {
        //TODO: popup editor, only create if transition passes safety check
    }
    
    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        _selectedInPoint = inPoint;

        if (_selectedOutPoint != null)
        {
            if (_selectedOutPoint.Node != _selectedInPoint.Node)
            {
                CreateTransitionNode();
                ClearConnectionSelection(); 
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }
    
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        _selectedOutPoint = outPoint;

        if (_selectedInPoint != null)
        {
            if (_selectedOutPoint.Node != _selectedInPoint.Node)
            {
                CreateTransitionNode();
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }
    
    private void OnClickRemoveConnection(TransitionNode transitionNode)
    {
        _transitionNodes.Remove(transitionNode);
        DeleteTransition(null); //TODO: fill
    }

    private void CreateTransitionNode()
    {
        _transitionNodes ??= new List<TransitionNode>();
        
        TransitionNode transition = new TransitionNode(_transitionNodeStyle, _selectedInPoint, _selectedOutPoint,
            OnClickRemoveConnection, this);
        _selectedInPoint.Node.AddTransition(transition);
        _selectedOutPoint.Node.AddTransition(transition);
        _transitionNodes.Add(transition);
    }

    private void ClearConnectionSelection()
    {
        _selectedInPoint = null;
        _selectedOutPoint = null;
    }
    
    //TODO: delete assets
    private void DeleteTransition(Transition transition)
    {
        
    }
    
    private void DeleteState(BaseState state)
    {
        
    }
}