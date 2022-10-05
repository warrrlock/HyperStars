using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WesleyDavies
{
    /// <summary>
    /// Base class for all easing functions.
    /// </summary>
    /// Easing functions based on: https://gist.github.com/cjddmut/d789b9eb78216998e95c
    public abstract class Easing
    {
        public enum Funcs { Linear, Quadratic, QuadraticIn, QuadraticOut, Cubic, CubicIn, CubicOut, Quartic, QuarticIn, QuarticOut }

        public abstract float Ease(float start, float end, float value);

        public abstract Vector2 Ease(Vector2 start, Vector2 end, float value);

        public abstract Vector3 Ease(Vector3 start, Vector3 end, float value);

        public abstract Quaternion Ease(Quaternion start, Quaternion end, float value);

        public abstract Color Ease(Color start, Color end, float value);

        public static Easing CreateEasingFunc(Funcs funcToCreate)
        {
            return funcToCreate switch
            {
                Funcs.Linear => new Linear(),
                Funcs.Quadratic => new Quadratic(),
                Funcs.QuadraticIn => new QuadraticIn(),
                Funcs.QuadraticOut => new QuadraticOut(),
                Funcs.Cubic => new Cubic(),
                Funcs.CubicIn => new CubicIn(),
                Funcs.CubicOut => new CubicOut(),
                _ => throw new Exception("Easing function does not exist.")
            };
        }
    }

    public class Linear : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value);
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            return Vector2.Lerp(start, end, value);
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            return Vector3.Lerp(start, end, value);
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            return Quaternion.Lerp(start, end, value);
        }

        public override Color Ease(Color start, Color end, float value)
        {
            return Color.Lerp(start, end, value);
        }
    }

    public class Quadratic : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * end + start;
            }
            value--;
            return (value * (value - 2) - 1) * -0.5f * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * end + start;
            }
            value--;
            return (value * (value - 2) - 1) * -0.5f * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * end + start;
            }
            value--;
            return (value * (value - 2) - 1) * -0.5f * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            value *= 2f;
            endVector -= startVector;
            if (value < 1f)
            {
                return (0.5f * value * value * endVector + startVector).ToQuaternion();
            }
            value--;
            return ((value * (value - 2) - 1) * -0.5f * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * end + start;
            }
            value--;
            return (value * (value - 2) - 1) * -0.5f * end + start;
        }
    }

    public class QuadraticIn : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            end -= start;
            return value * value * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            end -= start;
            return value * value * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            end -= start;
            return value * value * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            endVector -= startVector;
            return (value * value * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            end -= start;
            return value * value * end + start;
        }
    }

    public class QuadraticOut : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            end -= start;
            return (value - 2) * -value * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            end -= start;
            return (value - 2) * -value * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            end -= start;
            return (value - 2) * -value * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            endVector -= startVector;
            return ((value - 2) * -value * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            end -= start;
            return (value - 2) * -value * end + start;
        }
    }

    public class Cubic : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * value * end + start;
            }
            value -= 2f;
            return (value * value * value + 2f) * 0.5f * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1f)
            {
                return 0.5f * value * value * value * end + start;
            }
            value -= 2f;
            return (value * value * value + 2f) * 0.5f * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1)
            {
                return 0.5f * value * value * value * end + start;
            }
            value -= 2;
            return (value * value * value + 2) * 0.5f * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            value *= 2f;
            endVector -= startVector;
            if (value < 1)
            {
                return (0.5f * value * value * value * endVector + startVector).ToQuaternion();
            }
            value -= 2;
            return ((value * value * value + 2) * 0.5f * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            value *= 2f;
            end -= start;
            if (value < 1)
            {
                return 0.5f * value * value * value * end + start;
            }
            value -= 2;
            return (value * value * value + 2) * 0.5f * end + start;
        }
    }

    public class CubicIn : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            end -= start;
            return value * value * value * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            end -= start;
            return value * value * value * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            end -= start;
            return value * value * value * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            endVector -= startVector;
            return (value * value * value * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            end -= start;
            return value * value * value * end + start;
        }
    }

    public class CubicOut : Easing
    {
        public override float Ease(float start, float end, float value)
        {
            value--;
            end -= start;
            return (value * value * value + 1) * end + start;
        }

        public override Vector2 Ease(Vector2 start, Vector2 end, float value)
        {
            value--;
            end -= start;
            return (value * value * value + 1) * end + start;
        }

        public override Vector3 Ease(Vector3 start, Vector3 end, float value)
        {
            value--;
            end -= start;
            return (value * value * value + 1) * end + start;
        }

        public override Quaternion Ease(Quaternion start, Quaternion end, float value)
        {
            Vector4 startVector = start.ToVector4();
            Vector4 endVector = end.ToVector4();
            value--;
            endVector -= startVector;
            return ((value * value * value + 1) * endVector + startVector).ToQuaternion();
        }

        public override Color Ease(Color start, Color end, float value)
        {
            value--;
            end -= start;
            return (value * value * value + 1) * end + start;
        }
    }
}