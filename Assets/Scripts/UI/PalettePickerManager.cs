using System;
using System.Collections;
using UnityEngine;

namespace UI
{
    class WaitingColourPick
    {
        public int player;
        public int wantedIndex; 
    }
    
    public class PalettePickerManager: MonoBehaviour
    {
        [field: SerializeField] public PalettePicker[] PalettePickers { get; private set; }
        private bool _lock = false;
        private int _waitingPaletteSwap = -1;

        public void ConfirmColour(int player)
        {
            Services.Players[player].ConfirmColour();
        }
        
        public void SetColour(int player, int wanted, int direction, int size)
        {
            if (Services.Players[player].Ready) return;
            StartCoroutine(TrySetColour(player, wanted, direction, size));
        }

        private IEnumerator TrySetColour(int player, int wanted, int direction, int size)
        {
            // Debug.Log($"trying to set colour player:{player}, wanted:{wanted}, size:{size}");
            if (_lock)
            {
                _waitingPaletteSwap = Services.Players[player].PaletteIndex;
                yield return new WaitUntil(() => !_lock);
            }
            _lock = true;
            // Debug.Log($"unlocked trying to set colour player:{player}, wanted:{wanted}");
            
            int index = wanted;
            bool sameChar = Services.Characters[player ^ 1] == Services.Characters[player];
            
            if (sameChar)
            {
                //check if colour is available
                bool colourOpen = Services.Players[player ^ 1].PaletteIndex != wanted || _waitingPaletteSwap == wanted;
                if (!colourOpen)
                {
                    index = (index + direction + size) % size;
                }
            }
            Services.Players[player].PaletteIndex = index;
            PalettePickers[player].SetColour(index, player);
            if (sameChar)
                PalettePickers[player^1].SetColour(Services.Players[player^1].PaletteIndex, player^1);
            // _waitingPaletteSwap = -1;
            _lock = false;
        }
    }
}