using UnityEditor;
using UnityEngine;

namespace Battlehub.PackageUtils
{
    public static class Menu 
    {
        [MenuItem("Tools/Battlehub/Package Utils")]
        public static void PackageUtils()
        {
            EditorWindow wnd = EditorWindow.GetWindow<PackageUtilsWindow>();
            wnd.titleContent = new GUIContent("Package Utils");
        }
    }

}
