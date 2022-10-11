using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CollisionBoxManager: MonoBehaviour
{
    private int children = 0;
    public Color collisionBoxColor;
    public bool showGizmos;
    private void Awake()
    {
        children = transform.childCount;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (transform.childCount == children) return;
            children = transform.childCount;
            foreach (Transform child in transform)
            {
                child.gameObject.layer = gameObject.layer;
                Collider c = child.gameObject.GetComponent<Collider>();
                if (c) c.isTrigger = false;
            }
        }
    }
}
