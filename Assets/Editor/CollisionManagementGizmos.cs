using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CollisionManagementGizmos : Editor
{
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    static void DrawGizmoForMyScript(CollisionBoxManager collisionBoxManager, GizmoType gizmoType)
    {
        if (!collisionBoxManager.showGizmos) return;
        Gizmos.color = collisionBoxManager.collisionBoxColor;
        Transform transform = collisionBoxManager.transform;
        foreach (Transform child in transform)
        {
            Bounds bounds = child.GetComponent<Collider>().bounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }

        // Collider collider = collisionBoxManager.GetComponent<Collider>();
        // Gizmos.color = collisionBoxManager.collisionBoxColor;
        // Gizmos.DrawCube(collisionBoxManager.transform.position, collider.bounds.size);
    }
}
