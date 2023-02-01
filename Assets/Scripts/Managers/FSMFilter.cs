using System;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    [Serializable]
    public class FSMFilter : IComparable
    {
        public string filterName;
        public Color color;
        public bool focus;
        public bool edit;

        public FSMFilter(string n, Color c)
        {
            filterName = n;
            color = c;
        }
        
        public override string ToString()
        {
            return filterName;
        }
        
        public int CompareTo(object f)
        {
            return string.CompareOrdinal(filterName, ((FSMFilter)f).filterName);
        }
    }

    public class FSMFilterEqualityComparer : IEqualityComparer<FSMFilter>
    {
        public bool Equals(FSMFilter a, FSMFilter b)
        {
            return b != null && a != null && a.filterName.Equals(b.filterName, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(FSMFilter obj)
        {
            return obj.ToString().GetHashCode();
        }
    }

    public class FSMFilterComparer : IComparer<FSMFilter>
    {
        public int Compare(FSMFilter a, FSMFilter b)
        {
            return a.filterName.CompareTo(b.filterName);
        }
    }
}