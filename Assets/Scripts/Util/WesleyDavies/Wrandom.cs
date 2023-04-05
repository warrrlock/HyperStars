using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WesleyDavies
{
    public static class Wrandom
    {
        public static int RollDie(int sideCount, bool isZeroBased = true)
        {
            if (isZeroBased)
            {
                return Random.Range(0, sideCount);
            }
            return Random.Range(1, sideCount + 1);
        }

        public static int RollDice(int sideCount, int dieCount, bool isZeroBased = true)
        {
            int total = 0;
            for (int i = 0; i < dieCount; i++)
            {
                total += RollDie(sideCount, isZeroBased);
            }
            return total;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage">Ex. "15" is 15%.</param>
        /// <returns></returns>
        public static bool RollPercentChance(float percentage) //TODO: this only works with whole number percentages
        {
            int result = RollDie(100, false);
            if (result <= percentage)
            {
                return true;
            }
            return false;
        }

        //public static int RollDiceAsymmetric(bool isZeroBased = true, params int[] sideCounts)
        //{

        //}

        //public static bool StatCheck(int minValue)
        //{

        //}

        public static int AverageDiceRoll(int sideCount, int dieCount, bool isRounded = true, bool isZeroBased = true)
        {
            float total = 0;
            for (int i = 0; i < dieCount; i++)
            {
                total += RollDie(sideCount, isZeroBased);
            }
            Debug.Log(total);
            if (isRounded)
            {
                return Mathf.RoundToInt(total / dieCount);
            }
            return Mathf.FloorToInt(total / dieCount);
        }

        public static float AverageDiceRollFloat(int sideCount, int dieCount, bool isZeroBased = true)
        {
            float total = 0;
            for (int i = 0; i < dieCount; i++)
            {
                total += RollDie(sideCount, isZeroBased);
            }
            Debug.Log(total);
            return total / dieCount;
        }
    }
}