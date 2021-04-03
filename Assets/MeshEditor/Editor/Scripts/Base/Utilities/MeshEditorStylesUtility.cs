using UnityEditor;
using UnityEngine;
using UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base;

namespace MeshEditor.Editor.Scripts.Base.Utilities
{
    public static class MeshEditorStylesUtility
    {
        #region Style

        public static GUIStyle SelectStyle = new GUIStyle("AssetLabel");
        public  static GUIStyle UnSelectStyle = new GUIStyle("AssetLabel Partial");
        public  static GUIStyle FoldStyle = new GUIStyle("PreviewPackageInUse");
        public  static GUIStyle FrameStyle = new GUIStyle("FrameBox");
        public static GUIStyle GroupBoxStyle = new GUIStyle("GroupBox");
        public static GUIStyle TitleStyle = new GUIStyle(EditorGUIUtility.isProSkin ? "LODLevelNotifyText" : "DefaultCenteredLargeText");
        #endregion
    }
}