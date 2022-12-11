using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OGHitbox : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(10, 10);

    public LayerMask layerMask;

    public bool useSphere = false;

    public float radius = 0.5f;

    public Color inactiveColor;
    public Color colliderOpenColor;
    public Color collidingColor;

    private ColliderState _state;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f);


        if (colliders.Length > 0) {
            Debug.Log("hit something");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.DrawCube(Vector2.zero, new Vector2(boxSize.x * 2, boxSize.y * 2));
    }

    private void checkGizmoColor() 
    {
        switch (_state) {

            case ColliderState.closed:
                Gizmos.color = inactiveColor;
                break;

            case ColliderState.colliding:
                Gizmos.color = collidingColor;
                break;

            case ColliderState.open:
                Gizmos.color = colliderOpenColor;
                break;

        }
    }

    public void startCheckingCollision()
    {
        _state = ColliderState.open;
    }

    public void stopCheckingCollision()
    {
        _state = ColliderState.closed;
    }
}
