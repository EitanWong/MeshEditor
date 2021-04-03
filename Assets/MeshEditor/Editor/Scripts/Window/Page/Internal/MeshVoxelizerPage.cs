using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base;

public class MeshVoxelizerPage : MeshEditorPage
{
    #region 体素网格相关字段

    private bool meshVoxelizerFold; //是否点开体素网格选项
    private int pixelSize = 16; //像素尺寸
    private static bool IsCustomVoxelMat; //是否自定义体素材质
    private static Material mesh_CustomVoxelMat; //自定义体素材质

    #endregion

    private void Awake()
    {
        PageName = "体素网格";
        PageIcon= Resources.Load<Texture2D>("Textures/MeshVoxelizer");
        PageToolTips = "体素网格生成器\n生成体素网格";
    }


    protected override void OnGUI()
    {
        MeshVoxelizerMenu();
    }
    
    /// <summary>
    /// 体素网格编辑菜单
    /// </summary>
    private void MeshVoxelizerMenu()
    {
        var editMeshRenderer = CheckFields[0].Renderer;
        var editMeshFilter = CheckFields[0].Filter;
        ///————————————————————————————————————————————————————————————————————————————————————————————————————体素网格
        // meshVoxelizerFold = EditorGUILayout.BeginFoldoutHeaderGroup(meshVoxelizerFold, new GUIContent("体素网格"));
        // if (meshVoxelizerFold)
        // {
        pixelSize = EditorGUILayout.IntField(new GUIContent("像素尺寸"), pixelSize);
        if (!editMeshRenderer || !editMeshRenderer.sharedMaterial) //没有网格渲染器，或者 没有材质 直接选择自定义材质
        {
            IsCustomVoxelMat = true;
        }
        else
        {
            IsCustomVoxelMat = EditorGUILayout.Toggle(new GUIContent("自定义体素材质"), IsCustomVoxelMat);
        }

        if (IsCustomVoxelMat) //如果选择自定义
        {
            mesh_CustomVoxelMat =
                (Material) EditorGUILayout.ObjectField(new GUIContent("体素材质"), mesh_CustomVoxelMat,
                    typeof(Material),
                    false);
        }
        else if (editMeshRenderer && editMeshRenderer.sharedMaterial)
        {
            mesh_CustomVoxelMat = editMeshRenderer.sharedMaterial;
        }

        if (mesh_CustomVoxelMat)
            EditorGUILayout.HelpBox(string.Format("当前使用材质:{0}", mesh_CustomVoxelMat.name), MessageType.Info);
        else
        {
            EditorGUILayout.HelpBox(string.Format("未选择体素材质"), MessageType.Warning);
        }

        if (GUILayout.Button("创建体素网格"))
        {
            for (int i = 0; i < CheckFields.Count; i++)
            {
                editMeshFilter = CheckFields[i].Filter;
                if (!IsCustomVoxelMat)
                    mesh_CustomVoxelMat = editMeshRenderer.sharedMaterial;

                var voxelMesh = VoxelMeshBuilder.ConversionVoxelMesh(editMeshFilter.sharedMesh, pixelSize);
                if (voxelMesh)
                {
                    voxelMesh.name = editMeshFilter.sharedMesh.name;
                    GameObject newVoxelGameObj =
                        new GameObject(string.Format("{0}Voxelized", editMeshFilter.transform.name));
                    newVoxelGameObj.transform.position = editMeshFilter.transform.position;
                    newVoxelGameObj.transform.localScale = editMeshFilter.transform.localScale;
                    newVoxelGameObj.transform.rotation = editMeshFilter.transform.rotation;

                    var filter = newVoxelGameObj.AddComponent<MeshFilter>();
                    var renderer = newVoxelGameObj.AddComponent<MeshRenderer>();

                    filter.sharedMesh = voxelMesh;
                    renderer.sharedMaterial = mesh_CustomVoxelMat;
                }
            }
        }
    }
}