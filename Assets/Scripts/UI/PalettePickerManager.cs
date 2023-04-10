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
        private bool _lock;
        private int _waitingPaletteSwap;
        
        public void SetColour(int player, int wanted)
        {
            StartCoroutine(TrySetColour(player, wanted));
        }
        
        private IEnumerator TrySetColour(int player, int wanted)
        {
            if (_lock)
            {
                _waitingPaletteSwap = Services.Players[player].PaletteIndex;
                yield return new WaitUntil(() => !_lock);
            }
            _lock = true;
            
            int index = wanted;
            bool sameChar = Services.Characters[player ^ 1] == Services.Characters[player];
            if (sameChar)
            {
                //check if colour is available
                bool colourOpen = Services.Players[player ^ 1].PaletteIndex != wanted || _waitingPaletteSwap == wanted;

                if (!colourOpen)
                {
                    if (Services.Players[player].PaletteIndex == wanted) //go to next available
                        index++;
                    else //return to previous
                        index = Services.Players[player].PaletteIndex;
                }
            }
            PalettePickers[player].SetColour(index);
            Services.Players[player].PaletteIndex = index;

            _lock = false;
        }
    }
}