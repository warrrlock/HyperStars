using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using static UnityEditor.Experimental.GraphView.GraphView;
using Random = UnityEngine.Random;

namespace WesleyDavies
{
    public static class ExtensionMethods
    {
        #region Float

        /// <summary>
        /// Rounds float
        /// </summary>
        /// <param name="value"></param>
        /// <param name="place"></param>
        public static float RoundToPlace(this float value, int place)
        {
            return value;
        }

        /// <summary>
        /// Converts an angle in radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians to convert.</param>
        /// <returns>The given angle in degrees.</returns>
        public static float ToDegrees(this float radians)
        {
            return radians * 180f / Mathf.PI;
        }

        /// <summary>
        /// Converts an angle in degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees to convert.</param>
        /// <returns>The given angle in radians.</returns>
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.PI / 180f;
        }

        public static Vector2 ToDirection(this float angle, bool isRadians = true)
        {
            if (!isRadians)
            {
                angle = angle.ToRadians();
            }
            Vector2 direction;
            direction.x = Mathf.Cos(angle);
            direction.y = Mathf.Sin(angle);
            return direction;
        }
        #endregion

        #region Type
        /// <summary>
        /// Checks if an object's type matches or inherits from a specified type.
        /// </summary>
        /// <param name="type">This object's type.</param>
        /// <param name="baseClass">The type to check against.</param>
        /// <returns>True if object's type matches or inherits from specified type.</returns>
        /// Based off this: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
        public static bool IsSameOrSubclassOf(this Type type, Type baseClass)
        {
            return type == baseClass || type.IsSubclassOf(baseClass);
        }
        #endregion

        #region Array
        /// <summary>
        /// Shuffles an array in place.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The array to shuffle.</param>
        public static void Shuffle<T>(this T[] deck)
        {
            for (int i = 0; i < deck.Length; i++)
            {
                int randomIndex = PickRandomIndex(deck);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
        }

        /// <summary>
        /// Picks a random element from an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The array to pick from.</param>
        /// <returns>A random element from the given array.</returns>
        public static T PickRandom<T>(this T[] deck)
        {
            int randomIndex = Random.Range(0, deck.Length);
            return deck[randomIndex];
        }

        /// <summary>
        /// Picks a random index of an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The array to pick from.</param>
        /// <returns>A random index of the given array.</returns>
        public static int PickRandomIndex<T>(this T[] deck)
        {
            return Random.Range(0, deck.Length);
        }
        #endregion

        #region Float Array
        /// <summary>
        /// Finds the average of the given array.
        /// </summary>
        /// <param name="floats">The array to average.</param>
        /// <returns>The average of the given array.</returns>
        public static float Average(this float[] floats)
        {
            float total = 0f;
            for (int i = 0; i < floats.Length; i++)
            {
                total += floats[i];
            }
            return total / floats.Length;
        }

        /// <summary>
        /// Finds the average of the given array from firstIndex to lastIndex.
        /// </summary>
        /// <param name="floats">The array to average.</param>
        /// <param name="firstIndex">The zero-based index of the array to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the array to stop averaging at.</param>
        /// <returns>The average of the given array from firstIndex to lastIndex.</returns>
        public static float Average(this float[] floats, int firstIndex = 0, int lastIndex = int.MaxValue)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > floats.Length)
            {
                lastIndex = floats.Length;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += floats[i];
            }
            return total / floats.Length;
        }

        //TODO: do this!
        public static float Sum(this float[] floats)
        {
            return default;
        }
        #endregion

        #region Int Array
        /// <summary>
        /// Finds the average of the given array, rounded.
        /// </summary>
        /// <param name="ints">The array to average.</param>
        /// <returns>The average of the given array, rounded.</returns>
        public static int Average(this int[] ints)
        {
            float total = 0f;
            for (int i = 0; i < ints.Length; i++)
            {
                total += ints[i];
            }
            return Mathf.RoundToInt(total / ints.Length);
        }

        /// <summary>
        /// Finds the average of the given array.
        /// </summary>
        /// <param name="ints">The array to average.</param>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <returns>The average of the given array.</returns>
        public static int Average(this int[] ints, bool isRounded = true)
        {
            float total = 0f;
            for (int i = 0; i < ints.Length; i++)
            {
                total += ints[i];
            }
            if (isRounded)
            {
                return Mathf.RoundToInt(total / ints.Length);
            }
            return Mathf.FloorToInt(total / ints.Length);
        }

        /// <summary>
        /// Finds the average of the given array from firstIndex to lastIndex, rounded.
        /// </summary>
        /// <param name="ints">The array to average.</param>
        /// <param name="isRounded">Should the result be rounded down?</param>
        /// <param name="firstIndex">The zero-based index of the array to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the array to stop averaging at.</param>
        /// <returns>The average of the given array from firstIndex to lastIndex, rounded.</returns>
        public static float Average(this int[] ints, int firstIndex = 0, int lastIndex = int.MaxValue)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > ints.Length)
            {
                lastIndex = ints.Length;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += ints[i];
            }
            return Mathf.RoundToInt(total / ints.Length);
        }

        /// <summary>
        /// Finds the average of the given array from firstIndex to lastIndex.
        /// </summary>
        /// <param name="ints">The array to average.</param>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <param name="firstIndex">The zero-based index of the array to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the array to stop averaging at.</param>
        /// <returns>The average of the given array from firstIndex to lastIndex.</returns>
        public static float Average(this int[] ints, int firstIndex = 0, int lastIndex = int.MaxValue, bool isRounded = true)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > ints.Length)
            {
                lastIndex = ints.Length;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += ints[i];
            }
            if (isRounded)
            {
                return Mathf.RoundToInt(total / ints.Length);
            }
            return Mathf.FloorToInt(total / ints.Length);
        }
        #endregion

        #region List
        /// <summary>
        /// Finds the average of the given list.
        /// </summary>
        /// <param name="floats">The list to average.</param>
        /// <returns>The average of the given list.</returns>
        public static float Average(this List<float> floats)
        {
            float total = 0f;
            for (int i = 0; i < floats.Count; i++)
            {
                total += floats[i];
            }
            return total / floats.Count;
        }

        /// <summary>
        /// Finds the average of the given list from firstIndex to lastIndex.
        /// </summary>
        /// <param name="floats">The list to average.</param>
        /// <param name="firstIndex">The zero-based index of the list to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the list to stop averaging at.</param>
        /// <returns>The average of the given list from firstIndex to lastIndex.</returns>
        public static float Average(this List<float> floats, int firstIndex = 0, int lastIndex = int.MaxValue)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > floats.Count)
            {
                lastIndex = floats.Count;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += floats[i];
            }
            return total / floats.Count;
        }

        /// <summary>
        /// Finds the average of the given list, rounded.
        /// </summary>
        /// <param name="ints">The list to average.</param>
        /// <returns>The average of the given list, rounded.</returns>
        public static int Average(this List<int> ints)
        {
            float total = 0f;
            for (int i = 0; i < ints.Count; i++)
            {
                total += ints[i];
            }
            return Mathf.RoundToInt(total / ints.Count);
        }

        /// <summary>
        /// Finds the average of the given list.
        /// </summary>
        /// <param name="ints">The list to average.</param>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <returns>The average of the given list.</returns>
        public static int Average(this List<int> ints, bool isRounded = true)
        {
            float total = 0f;
            for (int i = 0; i < ints.Count; i++)
            {
                total += ints[i];
            }
            if (isRounded)
            {
                return Mathf.RoundToInt(total / ints.Count);
            }
            return Mathf.FloorToInt(total / ints.Count);
        }

        /// <summary>
        /// Finds the average of the given list from firstIndex to lastIndex, rounded.
        /// </summary>
        /// <param name="ints">The list to average.</param>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <param name="firstIndex">The zero-based index of the list to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the list to stop averaging at.</param>
        /// <returns>The average of the given list from firstIndex to lastIndex, rounded.</returns>
        public static float Average(this List<int> ints, int firstIndex = 0, int lastIndex = int.MaxValue)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > ints.Count)
            {
                lastIndex = ints.Count;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += ints[i];
            }
            return Mathf.RoundToInt(total / ints.Count);
        }

        /// <summary>
        /// Finds the average of the given list from firstIndex to lastIndex.
        /// </summary>
        /// <param name="ints">The list to average.</param>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <param name="firstIndex">The zero-based index of the list to start averaging at.</param>
        /// <param name="lastIndex">The zero-based index of the list to stop averaging at.</param>
        /// <returns>The average of the given list from firstIndex to lastIndex.</returns>
        public static float Average(this List<int> ints, int firstIndex = 0, int lastIndex = int.MaxValue, bool isRounded = true)
        {
            if (firstIndex < 0)
            {
                throw new Exception("First index cannot be less than zero.");
            }
            if (lastIndex < 0)
            {
                throw new Exception("Last index cannot be less than zero.");
            }
            if (lastIndex < firstIndex)
            {
                throw new Exception("Last index must be greater than or equal to first index.");
            }
            if (lastIndex > ints.Count)
            {
                lastIndex = ints.Count;
            }
            float total = 0f;
            for (int i = firstIndex; i < lastIndex; i++)
            {
                total += ints[i];
            }
            if (isRounded)
            {
                return Mathf.RoundToInt(total / ints.Count);
            }
            return Mathf.FloorToInt(total / ints.Count);
        }

        /// <summary>
        /// Shuffles a list in place.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The list to shuffle.</param>
        public static void Shuffle<T>(this List<T> deck)
        {
            for (int i = 0; i < deck.Count; i++)
            {
                int randomIndex = PickRandomIndex(deck);
                (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
            }
        }

        /// <summary>
        /// Picks a random element from a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The list to pick from.</param>
        /// <returns>A random element from the given list.</returns>
        public static T PickRandom<T>(this List<T> deck)
        {
            int randomIndex = Random.Range(0, deck.Count);
            return deck[randomIndex];
        }

        /// <summary>
        /// Picks a random index of a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deck">The list to pick from.</param>
        /// <returns>A random index of the given list.</returns>
        public static int PickRandomIndex<T>(this List<T> deck)
        {
            return Random.Range(0, deck.Count);
        }
        #endregion

        #region Vector2
        /// <summary>
        /// Converts Polar Coordinates to Cartesian Coordinates.
        /// </summary>
        /// <param name="polarCoords">The polar coordinates to convert. X must be theta and y must be r.</param>
        /// <param name="convertFromDegrees">Is theta in degrees?</param>
        /// <returns>The given coordinates in Cartesian form.</returns>
        public static Vector2 ToCartesian(this Vector2 polarCoords, bool convertFromDegrees = false)
        {
            float theta = convertFromDegrees ? polarCoords.x.ToRadians() : polarCoords.x;
            float r = polarCoords.y;
            Vector2 cartesianCoords;
            cartesianCoords.x = r * Mathf.Cos(theta);
            cartesianCoords.y = r * Mathf.Sin(theta);
            return cartesianCoords;
        }

        /// <summary>
        /// Converts Cartesian Coordinates to Polar Coordinates.
        /// </summary>
        /// <param name="cartesianCoords">The cartesian coordinates to convert.</param>
        /// <returns>The given coordinates in Polar form. X is theta and y is r.</returns>
        public static Vector2 ToPolar(this Vector2 cartesianCoords)
        {
            float x = cartesianCoords.x;
            float y = cartesianCoords.y;
            Vector2 polarCoords;
            polarCoords.x = Mathf.Atan2(y, x);
            polarCoords.y = Mathf.Sqrt(Mathf.Pow(x, 2f) + Mathf.Pow(y, 2f));
            return polarCoords;
        }
        #endregion

        #region Vector4
        /// <summary>
        /// Converts Vector4 into Quaternion.
        /// </summary>
        /// <param name="vector4">The vector4 to convert.</param>
        /// <returns>A quaternion with the same values as the given vector4.</returns>
        public static Quaternion ToQuaternion(this Vector4 vector4)
        {
            return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
        }
        #endregion

        #region Quaternion
        /// <summary>
        /// Converts Quaternion into Vector4.
        /// </summary>
        /// <param name="quaternion">The quaternion to convert.</param>
        /// <returns>A vector4 with the same values as the given quaternion.</returns>
        public static Vector4 ToVector4(this Quaternion quaternion)
        {
            return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
        #endregion

        #region MonoBehaviour
        //public static void DilateTime(this MonoBehaviour caller, float factor, float duration)
        //{
        //    caller.StartCoroutine(DilateTime(factor, duration));

        //    static IEnumerator DilateTime(float factor, float duration)
        //    {
        //        if (factor < 0f)
        //        {
        //            throw new System.Exception("Time dilation factor must be a positive non-zero number.");
        //        }
        //        if (factor == 0f)
        //        {
        //            throw new System.Exception("Time dilation factor must be a positive non-zero number. If freezing time is intended, use FreezeTime() instead.");
        //        }
        //        Time.timeScale *= factor;
        //        yield return new WaitForSecondsRealtime(duration);
        //        Time.timeScale /= factor;
        //        yield break;
        //    }
        //}

        //public static void Spin(this MonoBehaviour caller, float speed)
        //{


        //    static IEnumerator Spin()
        //    {
        //        yield break;
        //    }
        //}

        //public static void Spin(this MonoBehaviour caller, float speed, float duration)
        //{


        //    static IEnumerator Spin()
        //    {
        //        yield break;
        //    }
        //}
        #endregion

        #region Collision
        /// <summary>
        /// Gets the contact position of the given collision.
        /// </summary>
        /// <param name="collision">The Collision to check.</param>
        /// <returns>The contact position of the given collision.</returns>
        public static Vector3 ContactPosition(this Collision collision)
        {
            if (collision.contactCount == 1)
            {
                return collision.GetContact(0).point;
            }
            else if (collision.contactCount > 1)
            {
                //if collision has multiple contact points, that means it hit on a flat side of the collider. Thus, contactPoint is only the left side, so we need to bring it to the middle.
                Vector3[] contactPoints = new Vector3[collision.contactCount];
                for (int i = 0; i < collision.contactCount; i++)
                {
                    contactPoints[i] = collision.GetContact(i).point;
                }
                return Mathw.Average(contactPoints);
            }
            else
            {
                throw new System.Exception("Somehow, there were no contact points.");
            }
        }

        /// <summary>
        /// Gets the side of the collider that is hit in the given collision.
        /// </summary>
        /// <param name="collision">The Collision to check.</param>
        /// <param name="useRelativeSide">Can the side be slanted?</param>
        /// <returns>The side of the hit collider. If no side is valid, returns Other.</returns>
        public static Side ContactSide(this Collision collision, bool useRelativeSide = true)
        {
            Vector2 sideNormal = collision.GetContact(0).normal;
            if (useRelativeSide)
            {
                if (Mathf.Abs(sideNormal.x) > Mathf.Abs(sideNormal.y))
                {
                    if (sideNormal.x > 0f)
                    {
                        return Side.Right;
                    }
                    return Side.Left;
                }
                else if (Mathf.Abs(sideNormal.y) > Mathf.Abs(sideNormal.x))
                {
                    if (sideNormal.y > 0f)
                    {
                        return Side.Top;
                    }
                    return Side.Bottom;
                }
            }
            else
            {
                if (sideNormal.y == 1f)
                {
                    return Side.Top;
                }
                else if (sideNormal.y == -1f)
                {
                    return Side.Bottom;
                }
                else if (sideNormal.x == 1f)
                {
                    return Side.Right;
                }
                else if (sideNormal.x == -1f)
                {
                    return Side.Left;
                }
            }
            return Side.Other;
        }
        #endregion

        #region Collision2D
        /// <summary>
        /// Gets the contact position of the given collision.
        /// </summary>
        /// <param name="collision">The Collision2D to check.</param>
        /// <returns>The collision position of the given collision.</returns>
        public static Vector2 ContactPosition(this Collision2D collision)
        {
            if (collision.contactCount == 1)
            {
                return collision.GetContact(0).point;
            }
            else if (collision.contactCount > 1)
            {
                //if collision has multiple contact points, that means it hit on a flat side of the collider. Thus, contactPoint is only the left side, so we need to bring it to the middle.
                Vector2[] contactPoints = new Vector2[collision.contactCount];
                for (int i = 0; i < collision.contactCount; i++)
                {
                    contactPoints[i] = collision.GetContact(i).point;
                }
                return Mathw.Average(contactPoints);
            }
            else
            {
                throw new System.Exception("Somehow, there were no contact points.");
            }
        }

        /// <summary>
        /// Gets the side of the collider that is hit in the given collision.
        /// </summary>
        /// <param name="collision">The Collision2D to check.</param>
        /// <param name="useRelativeSide">Can the side be slanted?</param>
        /// <returns>The side of the hit collider. If no side is valid, returns Other.</returns>
        public static Side ContactSide(this Collision2D collision, bool useRelativeSide = true)
        {
            Vector2 sideNormal = collision.GetContact(0).normal;
            if (useRelativeSide)
            {
                if (Mathf.Abs(sideNormal.x) > Mathf.Abs(sideNormal.y))
                {
                    if (sideNormal.x > 0f)
                    {
                        return Side.Right;
                    }
                    return Side.Left;
                }
                else if (Mathf.Abs(sideNormal.y) > Mathf.Abs(sideNormal.x))
                {
                    if (sideNormal.y > 0f)
                    {
                        return Side.Top;
                    }
                    return Side.Bottom;
                }
            }
            else
            {
                if (sideNormal.y == 1f)
                {
                    return Side.Top;
                }
                else if (sideNormal.y == -1f)
                {
                    return Side.Bottom;
                }
                else if (sideNormal.x == 1f)
                {
                    return Side.Right;
                }
                else if (sideNormal.x == -1f)
                {
                    return Side.Left;
                }
            }
            return Side.Other;
        }
        #endregion

        #region Collider2D

        #endregion

        #region LayerMask
        /// <summary>
        /// Adds the given layers to this LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to add layers to.</param>
        /// <param name="layers">The layers to add to layerMask.</param>
        public static void AddLayers(this LayerMask layerMask, params int[] layers)
        {
            foreach (int layer in layers)
            {
                layerMask |= (1 << layer);
            }
        }

        /// <summary>
        /// Removes the given layers to this LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to remove layers from.</param>
        /// <param name="layers">The layers to remove from layerMask.</param>
        public static void RemoveLayers(this LayerMask layerMask, params int[] layers)
        {
            foreach (int layer in layers)
            {
                layerMask &= ~(1 << layer);
            }
        }
        #endregion

        #region RaycastHit
        /// <summary>
        /// Gets the side of the collider that is hit in the given RaycastHit2D.
        /// </summary>
        /// <param name="hit">The RaycastHit to check.</param>
        /// <param name="useRelativeSide">Can the side be slanted?</param>
        /// <returns>The side of the hit collider. If no side is valid, returns Other.</returns>
        public static Side ContactSide(this RaycastHit hit, bool useRelativeSide = true)
        {
            Vector2 sideNormal = hit.normal;
            if (useRelativeSide)
            {
                if (Mathf.Abs(sideNormal.x) > Mathf.Abs(sideNormal.y))
                {
                    if (sideNormal.x > 0f)
                    {
                        return Side.Right;
                    }
                    return Side.Left;
                }
                else if (Mathf.Abs(sideNormal.y) > Mathf.Abs(sideNormal.x))
                {
                    if (sideNormal.y > 0f)
                    {
                        return Side.Top;
                    }
                    return Side.Bottom;
                }
            }
            else
            {
                if (sideNormal.y == 1f)
                {
                    return Side.Top;
                }
                else if (sideNormal.y == -1f)
                {
                    return Side.Bottom;
                }
                else if (sideNormal.x == 1f)
                {
                    return Side.Right;
                }
                else if (sideNormal.x == -1f)
                {
                    return Side.Left;
                }
            }
            return Side.Other;
        }
        #endregion

        #region RaycastHit2D
        /// <summary>
        /// Gets the side of the collider that is hit in the given RaycastHit2D.
        /// </summary>
        /// <param name="hit">The RaycastHit2D to check.</param>
        /// <param name="useRelativeSide">Can the side be slanted?</param>
        /// <returns>The side of the hit collider. If no side is valid, returns Other.</returns>
        public static Side ContactSide(this RaycastHit2D hit, bool useRelativeSide = true)
        {
            Vector2 sideNormal = hit.normal;
            if (useRelativeSide)
            {
                if (Mathf.Abs(sideNormal.x) > Mathf.Abs(sideNormal.y))
                {
                    if (sideNormal.x > 0f)
                    {
                        return Side.Right;
                    }
                    return Side.Left;
                }
                else if (Mathf.Abs(sideNormal.y) > Mathf.Abs(sideNormal.x))
                {
                    if (sideNormal.y > 0f)
                    {
                        return Side.Top;
                    }
                    return Side.Bottom;
                }
            }
            else
            {
                if (sideNormal.y == 1f)
                {
                    return Side.Top;
                }
                else if (sideNormal.y == -1f)
                {
                    return Side.Bottom;
                }
                else if (sideNormal.x == 1f)
                {
                    return Side.Right;
                }
                else if (sideNormal.x == -1f)
                {
                    return Side.Left;
                }
            }
            return Side.Other;
        }
        #endregion
    }
}