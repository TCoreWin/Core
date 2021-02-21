using UnityEditor;
using UnityEngine;

namespace SquareDino.ChackerContrast
{
    class CheckerContrast : EditorWindow
    {
        [MenuItem("Window/SquareDino/CheckerContrast")]
        static void Init()
        {
            CheckerContrast window = (CheckerContrast) GetWindow(typeof(CheckerContrast));
        }

        private void OnGUI()
        {

        }
    }
}