using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WesleyDavies
{
    /// <summary>
    /// Collection of useful math functions.
    /// </summary>
    public static class Mathw
    {
        /// <summary>
        /// Finds the average of the given values.
        /// </summary>
        /// <param name="floats">The values to average.</param>
        /// <returns>The average of the given values.</returns>
        public static float Average(params float[] floats)
        {
            float total = 0f;
            for (int i = 0; i < floats.Length; i++)
            {
                total += floats[i];
            }
            return total / floats.Length;
        }

        /// <summary>
        /// Finds the average of the given values, rounded.
        /// </summary>
        /// <param name="ints">The values to average.</param>
        /// <returns>The average of the given values, rounded.</returns>
        public static int Average(params int[] ints)
        {
            int total = 0;
            for (int i = 0; i < ints.Length; i++)
            {
                total += ints[i];
            }
            return Mathf.RoundToInt(total / ints.Length);
        }

        /// <summary>
        /// Finds the average of the given values.
        /// </summary>
        /// <param name="isRounded">Should the result be rounded?</param>
        /// <param name="ints">The values to average.</param>
        /// <returns>The average of the given values.</returns>
        public static int Average(bool isRounded = true, params int[] ints)
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
        /// Finds the average of the given values.
        /// </summary>
        /// <param name="vectors">The values to average.</param>
        /// <returns>The average of the given values.</returns>
        public static Vector2 Average(params Vector2[] vectors)
        {
            Vector2 total = Vector2.zero;
            for (int i = 0; i < vectors.Length; i++)
            {
                total.x += vectors[i].x;
                total.y += vectors[i].y;
            }
            return total / vectors.Length;
        }

        /// <summary>
        /// Finds the average of the given values.
        /// </summary>
        /// <param name="vectors">The values to average.</param>
        /// <returns>The average of the given values.</returns>
        public static Vector3 Average(params Vector3[] vectors)
        {
            Vector3 total = Vector3.zero;
            for (int i = 0; i < vectors.Length; i++)
            {
                total.x += vectors[i].x;
                total.y += vectors[i].y;
                total.z += vectors[i].z;
            }
            return total / vectors.Length;
        }

        //TODO: do this!
        public static float Sum(params float[] floats)
        {
            return default;
        }

        /// <summary>
        /// Linearly interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        /// Action based off: https://forum.unity.com/threads/passing-ref-variable-to-coroutine.379640/
        public static IEnumerator Tlerp(Action<float> changeValue, float start, float end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Mathf.Lerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Interpolates a value over a period of time using an easing function.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <param name="easingFunction">The easing function to apply to the interpolation.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<float> changeValue, float start, float end, float duration, Easing.Funcs easingFunction = Easing.Funcs.Linear)
        {
            Easing function = Easing.CreateEasingFunc(easingFunction);
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(function.Ease(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Linearly interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Vector2> changeValue, Vector2 start, Vector2 end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Vector2.Lerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Interpolates a value over a period of time using an easing function.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <param name="easingFunction">The easing function to apply to the interpolation.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Vector2> changeValue, Vector2 start, Vector2 end, float duration, Easing.Funcs easingFunction = Easing.Funcs.Linear)
        {
            Easing function = Easing.CreateEasingFunc(easingFunction);
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(function.Ease(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Linearly interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Vector3> changeValue, Vector3 start, Vector3 end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Vector3.Lerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Interpolates a value over a period of time using an easing function.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <param name="easingFunction">The easing function to apply to the interpolation.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Vector3> changeValue, Vector3 start, Vector3 end, float duration, Easing.Funcs easingFunction = Easing.Funcs.Linear)
        {
            Easing function = Easing.CreateEasingFunc(easingFunction);
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(function.Ease(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Linearly interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Quaternion> changeValue, Quaternion start, Quaternion end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Quaternion.Lerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Interpolates a value over a period of time using an easing function.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <param name="easingFunction">The easing function to apply to the interpolation.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Quaternion> changeValue, Quaternion start, Quaternion end, float duration, Easing.Funcs easingFunction = Easing.Funcs.Linear)
        {
            Easing function = Easing.CreateEasingFunc(easingFunction);
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(function.Ease(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Linearly interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Color> changeValue, Color start, Color end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Color.Lerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Interpolates a value over a period of time using an easing function.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <param name="easingFunction">The easing function to apply to the interpolation.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tlerp(Action<Color> changeValue, Color start, Color end, float duration, Easing.Funcs easingFunction = Easing.Funcs.Linear)
        {
            Easing function = Easing.CreateEasingFunc(easingFunction);
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(function.Ease(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Spherically interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tslerp(Action<Vector3> changeValue, Vector3 start, Vector3 end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Vector3.Slerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }

        /// <summary>
        /// Spherically interpolates a value over a period of time.
        /// </summary>
        /// <param name="changeValue">The action that updates the value.</param>
        /// <param name="start">The start value of the lerp.</param>
        /// <param name="end">The end value of the lerp.</param>
        /// <param name="duration">How long to lerp for.</param>
        /// <returns>A constantly updated value using the changeValue action.</returns>
        public static IEnumerator Tslerp(Action<Quaternion> changeValue, Quaternion start, Quaternion end, float duration)
        {
            float timer = 0f;
            while (timer < duration)
            {
                changeValue(Quaternion.Slerp(start, end, timer / duration));
                timer += Time.deltaTime;
                yield return null;
            }
            changeValue(end);
            yield break;
        }
    }
}