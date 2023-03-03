using UnityEngine;

namespace Util
{
    [CreateAssetMenu(menuName = "Build Settings")]
    public class BuildSettingIndices: ScriptableObject
    {
        public int mainMenuScene = 0;
        public int selectionScene = 1;
        public int gameScene = 2;
        public int trainingSelectionScene = 3;
        public int trainingScene = 4;
    }
}