using System;
using System.Collections;
using System.Collections.Generic;
using MeshEditor.Editor.Scripts.Base.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshEditorPreference : Editor
{
    private static string Version = "v0.1.0";
    private static Vector2 scrollPositionSettingArea;
    private static Vector2 scrollPositionTitle;

#pragma warning disable 618
    [PreferenceItem("UnityExtensions/MeshEditor")]
#pragma warning restore 618
    public static void PreferencesGUI()
    {
        EditorGUILayout.BeginHorizontal();
        DrawTitleInfo();
        DrawSettingArea();
        EditorGUILayout.EndHorizontal();
    }

    #region DrawGUI

    /// <summary>
    /// 绘制标题信息
    /// </summary>
    private static void DrawTitleInfo()
    {
        var icon = Resources.Load<Texture2D>("Textures/MeshEditorIcon");
        var iconWidth = icon.width / 10;
        var iconHeight = icon.height / 10;
        EditorGUILayout.BeginScrollView(scrollPositionTitle,MeshEditorStylesUtility.FrameStyle,GUILayout.MaxWidth(iconWidth),GUILayout.ExpandWidth(true));
        
        GUI.DrawTexture(new Rect(0, iconHeight * 0.1f, iconWidth, iconHeight), icon, ScaleMode.StretchToFill, true,
            10.0F);
        var titleStyle = MeshEditorStylesUtility.TitleStyle;
        GUI.Label(new Rect(0, iconHeight, iconWidth, iconHeight * 0.1f), "网格编辑器", titleStyle);
        GUI.Label(new Rect(0, iconHeight * 1.1f, iconWidth, iconHeight * 0.1f), Version, titleStyle);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制设置区域
    /// </summary>
    private static void DrawSettingArea()
    {
        scrollPositionSettingArea = EditorGUILayout.BeginScrollView(scrollPositionSettingArea, MeshEditorStylesUtility.FrameStyle, GUILayout.ExpandWidth(true));
        var titleStyle = MeshEditorStylesUtility.TitleStyle;
        GUILayout.Label("施工区", titleStyle);
        GUILayout.Label("配置功能开发中");
        EditorGUILayout.EndScrollView();
    }

    #endregion
}