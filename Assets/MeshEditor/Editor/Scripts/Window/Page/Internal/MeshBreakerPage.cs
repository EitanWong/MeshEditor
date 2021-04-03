using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EzySlice;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base;
using Random = UnityEngine.Random;

public class MeshBreakerPage : MeshEditorPage
{
    #region 网格破碎相关字段

    private static bool IsCustomCutMat; //是否自定义切面材质
    private static int mesh_iterations = 6; //破碎迭代次数

    private static Material mesh_CustomBreakCutMat; //自定义切面材质
    // private static bool meshBreakFold; //是否点开压缩选项折页

    #endregion

    private void Awake()
    {
        PageName = "网格破碎";
        PageIcon = Resources.Load<Texture2D>("Textures/MeshBreaker");
        PageToolTips = "网格模型破碎制作工具";
    }

    protected override void OnGUI()
    {
        MeshBreakMenu();
    }

    

    /// <summary>
    /// 破碎网格编辑菜单
    /// </summary>
    private void MeshBreakMenu()
    {
        if (CheckFields == null || CheckFields.Count <= 0) return;

        var editMeshRenderer = CheckFields[0].Renderer;
        var editMeshFilter = CheckFields[0].Filter;
        ///————————————————————————————————————————————————————————————————————————————————————————————————————网格破碎
        // meshBreakFold = EditorGUILayout.BeginFoldoutHeaderGroup(meshBreakFold, new GUIContent("网格破碎"));
        // if (meshBreakFold)
        // {
        mesh_iterations = EditorGUILayout.IntField("迭代次数", mesh_iterations);
        EditorGUILayout.HelpBox("迭代次数越多，切片数量越多", MessageType.Info);
        if (!editMeshRenderer || !editMeshRenderer.sharedMaterial) //没有网格渲染器，或者 没有材质 直接选择自定义材质
        {
            IsCustomCutMat = true;
        }
        else
        {
            IsCustomCutMat = EditorGUILayout.Toggle(new GUIContent("自定义切面材质"), IsCustomCutMat);
        }


        if (IsCustomCutMat) //如果选择自定义
        {
            mesh_CustomBreakCutMat =
                (Material) EditorGUILayout.ObjectField(new GUIContent("切面材质"), mesh_CustomBreakCutMat,
                    typeof(Material),
                    false);
        }
        else if (editMeshRenderer && editMeshRenderer.sharedMaterial)
        {
            mesh_CustomBreakCutMat = editMeshRenderer.sharedMaterial;
        }

        if (mesh_CustomBreakCutMat)
            EditorGUILayout.HelpBox(string.Format("当前使用材质:{0}", mesh_CustomBreakCutMat.name), MessageType.Info);
        else
        {
            EditorGUILayout.HelpBox(string.Format("未选择切面材质"), MessageType.Warning);
        }


        if (GUILayout.Button("破碎网格"))
        {
            if (EditorUtility.DisplayDialog("确定要破碎网格模型吗?\n此操作不可逆哦", "", "确定", "取消"))
            {
                //MeshBreaker.BreakMesh(editMeshFilter, mesh_CutCascades);
                if (!mesh_CustomBreakCutMat)
                {
                    mesh_CustomBreakCutMat = new Material(Shader.Find("Standard"));
                }

                foreach (var field in CheckFields)
                {
                    editMeshFilter = field.Filter;
                    editMeshRenderer = field.Renderer;
                    if (mesh_CustomBreakCutMat)
                        mesh_CustomBreakCutMat = field.Renderer.sharedMaterial;
                    if (ShatterObject(editMeshFilter.transform, editMeshFilter.gameObject, mesh_iterations,
                        mesh_CustomBreakCutMat))
                        editMeshRenderer.enabled = false;
                }
                
                ParentWindow.ShowNotification(new GUIContent("网格破碎执行成功"), 1f);
            }
        }

    }


    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// 网格破碎
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="iterations"></param>
    /// <param name="crossSectionMaterial"></param>
    /// <returns></returns>
    public bool ShatterObject(Transform root, GameObject obj, int iterations, Material crossSectionMaterial)
    {
        if (iterations > 0)
        {
            var scaleOffset = obj.transform.localScale;
            var objPosition = obj.transform.position;
            GameObject[] slices = obj.SliceInstantiate(objPosition + (Random.insideUnitSphere * scaleOffset.magnitude),
                objPosition + Random.insideUnitSphere,
                new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f),
                crossSectionMaterial);

            if (slices != null)
            {
                // shatter the shattered!
                for (int i = 0; i < slices.Length; i++)
                {
                    slices[i].transform.SetParent(root);
                    slices[i].transform.localPosition = Vector3.zero;
                    if (ShatterObject(root, slices[i], iterations - 1, crossSectionMaterial))
                    {
                        GameObject.DestroyImmediate(slices[i]);
                    }
                }

                return true;
            }

            return ShatterObject(root, obj, iterations - 1, crossSectionMaterial);
        }

        return false;
    }
}