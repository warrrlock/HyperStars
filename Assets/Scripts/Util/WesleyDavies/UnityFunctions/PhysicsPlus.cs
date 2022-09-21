using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WesleyDavies
{
    namespace UnityFunctions
    {
        /// <summary>
        /// Extra collision functions.
        /// </summary>
        public static class PhysicsPlus
        {
            /// <summary>
            /// Is the given collider touching the ground?
            /// </summary>
            /// <param name="boxCollider">The collider to check.</param>
            /// <param name="skinWidth">The distance beneath the given collider to check.</param>
            /// <returns>True if given collider is touching the ground.</returns>
            public static bool IsGrounded(BoxCollider2D boxCollider, float skinWidth = 0.1f)
            {
                if (!Physics2D.OverlapBox(new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y - (boxCollider.bounds.extents.y + skinWidth)), new Vector2(boxCollider.transform.lossyScale.x, skinWidth), 0f))
                {
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Is the given collider touching the ground?
            /// </summary>
            /// <param name="boxCollider">The collider to check.</param>
            /// <param name="groundMask">The LayerMask of the ground.</param>
            /// <param name="skinWidth">The distance beneath the given collider to check.</param>
            /// <returns>True if given collider is touching the ground.</returns>
            public static bool IsGrounded(BoxCollider2D boxCollider, LayerMask groundMask, float skinWidth = 0.1f)
            {
                if (!Physics2D.OverlapBox(new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y - (boxCollider.bounds.extents.y + skinWidth)), new Vector2(boxCollider.transform.lossyScale.x, skinWidth), 0f, groundMask))
                {
                    return false;
                }
                return true;
            }

            public static bool HasContactSide(BoxCollider2D boxCollider, Side side, float skinWidth = 0.1f)
            {
                if (!Physics2D.OverlapBox(new Vector2(boxCollider.transform.position.x, boxCollider.transform.position.y - (boxCollider.bounds.extents.y + skinWidth)), new Vector2(boxCollider.transform.lossyScale.x, skinWidth), 0f))
                {
                    return false;
                }
                return true;
            }
        }
    }
}