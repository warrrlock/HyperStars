using System;
using UnityEngine;

namespace UI
{
    //should it stay the same between rooms? or have to set it every time
    public class TrainingRoomManager: MonoBehaviour
    {
        [Header("info")]
        [SerializeField] private FightersManager _fightersManager;
        [SerializeField] private float _xDistanceBetweenPlayers;
        
        [Header("Room Positions")]
        [SerializeField] private Transform _leftRoomPosition;
        [SerializeField] private Transform _middleRoomPosition;
        [SerializeField] private Transform _rightRoomPosition;
        
        private bool _infiniteEx;
        private bool _showInputs;
        

        private void Start()
        {
            HandleInfiniteEx();
            // _xDistanceBetweenPlayers =
            //     _fightersManager.player1StartPosition.x + _fightersManager.player2StartPosition.x;
        }

        public void ToggleInfiniteEx()
        {
            _infiniteEx = !_infiniteEx;
            HandleInfiniteEx();
        }
        
        public void SetPositionLeft()
        {
            HandlePositionReset(_leftRoomPosition.position);
        }
        
        public void SetPositionMiddle()
        {
            HandlePositionReset(_middleRoomPosition.position);
        }
        
        public void SetPositionRight()
        {
            HandlePositionReset(_rightRoomPosition.position);
        }
        

        private void HandleInfiniteEx()
        {
            foreach (Fighter fighter in Services.Fighters)
            {
                fighter.BaseStateMachine.InfiniteEx = _infiniteEx;
                if (_infiniteEx) fighter.SpecialMeterManager.FillBars();
                else fighter.SpecialMeterManager.ClearBars();
            }
        }

        private void HandlePositionReset(Vector3 position)
        {
            int side = -1;
            foreach (Fighter fighter in Services.Fighters)
            {
                Vector3 pos = new Vector3(position.x + (side * _xDistanceBetweenPlayers * 0.5f), position.y, position.z);
                fighter.ResetWithPosition(pos);
                side *= -1;
            }
        }
    }
}