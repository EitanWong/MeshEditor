using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base
{
    public static class MeshEditorUtility
    {
        public static Assembly assembly => Assembly.Load("Assembly-CSharp");
        public static Assembly assemblyEditor => Assembly.Load("Assembly-CSharp-Editor");

        public static List<MeshEditorPage> GetAllEditorPage()
        {
            List<MeshEditorPage> editorPages = new List<MeshEditorPage>();
            var types = assemblyEditor.GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(MeshEditorPage) && type != typeof(MeshEditorPage))
                {
                    MeshEditorPage ins = (MeshEditorPage) EditorWindow.CreateInstance(type);
                    editorPages.Add(ins);
                }
            }

            return editorPages;
        }
        
        
        /// <summary>
        /// 显示通知在Scene窗口
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="time"></param>
        public static void ShowNotificationOnSceneView(string msg, float time)
        {
            var currentView = SceneView.currentDrawingSceneView;
            if (currentView)
            {
                currentView.ShowNotification(new GUIContent(msg), time);
            }
            else
            {
                var views = SceneView.sceneViews;
                if (views != null && views.Count > 0)
                {
                    var firstView = (EditorWindow) views[0];
                    firstView.ShowNotification(new GUIContent(msg), time);
                }
            }
        }
        
    }
}