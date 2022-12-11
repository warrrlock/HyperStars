using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace WesleyDavies
{
    namespace UnityFunctions
    {
        /// <summary>
        /// Juice functions.
        /// </summary>
        public static class Juice
        {
            [Tooltip("The original time scale before it was frozen.")]
            private static float _originalTimeScale = 1f;

            public static IEnumerator ShakeScreen(Camera camera, float magnitude, float speed, float duration, params Axis[] axes)
            {
                if (axes.Distinct().Count() != axes.Length)
                {
                    throw new System.Exception("Duplicate axes were specified. An axis should only be specified as a parameter once.");
                }
                Vector3 startPosition = camera.transform.position;
                float timeShaking = 0f;
                switch (axes.Length)
                {
                    case 0:
                        throw new System.Exception("No axes were specified.");
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }
                while (timeShaking <= duration)
                {
                    //TODO: use Random.onUnitSphere for 3 axes
                    //TODO: use Random.insideUnitCircle.normalized for 2 axes (since there is no onUnitCircle)

                    //Vector3 cameraDefaultPositionLocal = transform.InverseTransformPoint(startPosition);
                    //float cameraPositionX = cameraDefaultPositionLocal.x + (duration - timeShaking) / duration * magnitude * Mathf.Sin(timeShaking * speed);
                    //Vector3 deltaCameraPosition = transform.TransformPoint(new Vector3(cameraPositionX, 0f, 0f));
                    //camera.transform.position = deltaCameraPosition;
                    timeShaking += Time.deltaTime;
                    yield return null;
                }
                camera.transform.position = startPosition;
                yield break;
            }

            public static void DilateTime(this MonoBehaviour caller, float factor, float duration)
            {
                caller.StartCoroutine(DilateTime(factor, duration));

                static IEnumerator DilateTime(float factor, float duration)
                {
                    if (factor < 0f)
                    {
                        throw new System.Exception("Time dilation factor must be a positive non-zero number.");
                    }
                    if (factor == 0f)
                    {
                        throw new System.Exception("Time dilation factor must be a positive non-zero number. If freezing time is intended, use FreezeTime() instead.");
                    }
                    Time.timeScale *= factor;
                    yield return new WaitForSecondsRealtime(duration);
                    Time.timeScale /= factor;
                    yield break;
                }
            }

            ///// <summary>
            ///// Dilates game time by a given factor for a given duration.
            ///// </summary>
            ///// <param name="factor">The factor to dilate game time by. A factor greater than 1 speeds up time; a factor less than 1 slows down time.</param>
            ///// <param name="duration">The amount of time in seconds to dilate game time for.</param>
            ///// <returns></returns>
            ///// <exception cref="System.Exception"></exception>
            //public static IEnumerator DilateTime(float factor, float duration)
            //{
            //    if (factor < 0f)
            //    {
            //        throw new System.Exception("Time dilation factor must be a positive non-zero number.");
            //    }
            //    if (factor == 0f)
            //    {
            //        throw new System.Exception("Time dilation factor must be a positive non-zero number. If freezing time is intended, use FreezeTime() instead.");
            //    }
            //    Time.timeScale *= factor;
            //    yield return new WaitForSecondsRealtime(duration);
            //    Time.timeScale /= factor;
            //    yield break;
            //}

            //TODO: make DilateTime() and FreezeTime() coroutines that use easing in and out

            /// <summary>
            /// Dilates game time by a given factor.
            /// </summary>
            /// <param name="factor">The factor to dilate game time by. A factor greater than 1 speeds up time; a factor less than 1 slows down time.</param>
            /// <exception cref="System.Exception"></exception>
            public static void DilateTime(float factor)
            {
                if (factor < 0f)
                {
                    throw new System.Exception("Time dilation factor must be a positive non-zero number.");
                }
                if (factor == 0f)
                {
                    throw new System.Exception("Time dilation factor must be a positive non-zero number. If freezing time is intended, use FreezeTime() instead.");
                }
                Time.timeScale *= factor;
            }

            /// <summary>
            /// Freezes game time for a given duration.
            /// </summary>
            /// <param name="duration">The amount of time in seconds to freeze game time for.</param>
            /// <returns></returns>
            public static IEnumerator FreezeTime(float duration)
            {
                if (Time.timeScale != 0f)
                {
                    _originalTimeScale = Time.timeScale;
                }
                Time.timeScale = 0f;
                yield return new WaitForSecondsRealtime(duration);
                Time.timeScale = _originalTimeScale;
                yield break;
            }

            /// <summary>
            /// Freezes game time indefinitely.
            /// </summary>
            public static void FreezeTime()
            {
                if (Time.timeScale != 0f)
                {
                    _originalTimeScale = Time.timeScale;
                }
                Time.timeScale = 0f;
            }

            /// <summary>
            /// Unfreezes game time. Only works if game time is currently frozen.
            /// </summary>
            /// <exception cref="System.Exception"></exception>
            public static void UnfreezeTime()
            {
                if (Time.timeScale != 0f)
                {
                    return;
                    //throw new System.Exception("Time is not currently frozen.");
                }
                Time.timeScale = _originalTimeScale;
            }
        }
    }
}