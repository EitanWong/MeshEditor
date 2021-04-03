using System;
using System.Collections.Generic;
using EzySlice;
using MeshEditor.Editor.Scripts.Base.Utilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExtensions.MeshPro.MeshEditor.Editor.Scripts.Base;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace UnityExtensions.MeshPro.MeshEditor.Editor
{
    public class MeshEditorWindow : EditorWindow
    {
        #region 通用字段

        private List<MeshEditorItem> itemsPool = new List<MeshEditorItem>(); //MeshField池
        public List<MeshEditorItem> CheckItems = new List<MeshEditorItem>(); //选中的MeshField
        private static List<MeshEditorPage> editorPages = new List<MeshEditorPage>(); //编辑器页面
        private static Vector2 scrollPos; //Scroll位置
        public static MeshEditorWindow window; //当前窗体

        #endregion

        #region Event

        void OnSelectionChange() => SelectionChangeEvent();

        public Action OnWindowDisable;

        #endregion

        [MenuItem("CONTEXT/MeshFilter/网格模型编辑器")]
        [MenuItem("Window/模型/网格模型编辑器 ^M")]
        static void Init()
        {
            // var width = 300;
            // var height = 500;
            window = (MeshEditorWindow) EditorWindow.GetWindow(
                typeof(MeshEditorWindow),
                true,
                "网格编辑器");
            window.Focus();

            editorPages = MeshEditorUtility.GetAllEditorPage();
            foreach (var page in editorPages)
            {
                page.ParentWindow = window;
                window.OnWindowDisable += page.OnCloseWindowCallFromMainWindow;
            }
        }

        #region EditorBehavior

        private void Awake()
        {
            SceneView.duringSceneGui += OnSceneUpdate;
            AutoAddOpenItem(); //自动添加开启时编辑项
        }

        void OnGUI()
        {
            ExitWindowKeyboardCheck(); //检查退出快捷键
            DrawBasicMeshItemMenu(); //基本网格选择菜单
            DrawEditorPage();
        }

        /// <summary>
        /// SceneUpdate
        /// </summary>
        /// <param name="view"></param>
        private void OnSceneUpdate(SceneView view)
        {
            foreach (var page in editorPages)
            {
                if (page)
                    page.OnSceneGUIUpdateFromMainWindow();
            }
        }

        private void OnDisable()
        {
            OnWindowDisable?.Invoke();
            foreach (var page in editorPages)
            {
                window.OnWindowDisable -= page.OnCloseWindowCallFromMainWindow;
            }

            SceneView.duringSceneGui -= OnSceneUpdate;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void SelectionChangeEvent()
        {
            for (int i = itemsPool.Count - 1; i >= 0; i--)
            {
                var field = itemsPool[i];
                if (field != null)
                {
                    if (!field.Filter)
                    {
                        itemsPool.Remove(field);
                        CheckItems.Remove(field);
                        window.Focus();
                    }
                }
            }
        }

        public void AutoAddOpenItem()
        {
            var OpenSelectObjects = Selection.gameObjects;
            if (OpenSelectObjects == null || OpenSelectObjects.Length <= 0)
                return;

            foreach (var slObj in OpenSelectObjects)
            {
                AddMeshField(slObj);
            }

            // if (window)
            // {
            //     var msg = new GUIContent(string.Format("自动添加{0}个\n网格编辑项目", itemsPool.Count));
            //     window.ShowNotification(msg, 1f);
            //     window.Repaint();
            // }


            //InitMeshReorderableList();
        }

        #endregion

        #region 编辑器菜单绘制创建

        /// <summary>
        /// 绘制基本网格编辑项目菜单
        /// </summary>
        private void DrawBasicMeshItemMenu()
        {
            var poolCount = itemsPool.Count;
            var basicWidth = poolCount > 0 ? position.width * 0.25f : position.width;
            var basicHeight = poolCount > 0 ? position.height * 1f : position.width;
            DropAreaGUI(0, 0, basicWidth, basicHeight, DrawEditorItemScrollView, AddMeshField, OnContextClickHandler);
        }


        /// <summary>
        /// 绘制编辑器功能页面
        /// </summary>
        private void DrawEditorPage()
        {
            if (CheckItems == null || CheckItems.Count <= 0)
            {
                return; //如果没有选择直接返回 不执行下面功能
            }

            GUILayout.BeginArea(new Rect(position.width / 4, 0, position.width * (3f / 4f), position.height));


            foreach (var page in editorPages)
            {
                page.Open = EditorGUILayout.BeginFoldoutHeaderGroup(page.Open,
                    new GUIContent(page.PageName, page.PageIcon, page.PageToolTips),
                    MeshEditorStylesUtility.FoldStyle);
                if (page.Open)
                {
                    EditorGUILayout.BeginVertical(MeshEditorStylesUtility.FrameStyle);
                    if (CheckItems != null && CheckItems.Count > 0)
                        page.OnPageGUIUpdateFromMainWindow();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// 绘制编辑器编辑项目UI
        /// </summary>
        private void DrawEditorItemScrollView()
        {
            if (DrawEditorIntroUI()) //检查是否绘制介绍界面
                return; //如果绘制介绍界面则 直接返回 不执行绘制编辑项目

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, MeshEditorStylesUtility.GroupBoxStyle);
            //StyleInit
            var selectStyle = MeshEditorStylesUtility.SelectStyle;
            var unSelectStyle = MeshEditorStylesUtility.UnSelectStyle;
            //
            for (int i = itemsPool.Count - 1; i >= 0; i--)
            {
                var field = itemsPool[i];
                if (field != null && field.Filter)
                {
                    var current_Obj = field.Filter.gameObject;
                    var vertexCount = field.Filter.sharedMesh.vertexCount;
                    var trianglesCount = field.Filter.sharedMesh.triangles.Length;
                    var hasInSelect = CheckItems.Contains(field);
                    var contant = String.Empty;

                    if (hasInSelect)
                    {
                        contant = string.Format("{0}√ [{1}] 「⁂:{2}  ▲:{3}」",
                            CheckItems.FindIndex(f => f == field) + 1,
                            current_Obj.name, vertexCount,
                            trianglesCount);
                    }
                    else
                    {
                        contant = string.Format("[{0}] 「⁂:{1}  ▲:{2}」", current_Obj.name, vertexCount,
                            trianglesCount);
                    }

                    var check = EditorGUILayout.ToggleLeft(new GUIContent(contant), field.Check,
                        field.Check ? selectStyle : unSelectStyle);

                    if (check && !hasInSelect)
                    {
                        CheckField(field);
                    }
                    else if (!check && hasInSelect)
                    {
                        UncheckField(field);
                        //Checkfields.Remove(field);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }


        /// <summary>
        /// 绘制编辑器介绍街面
        /// </summary>
        /// <returns></returns>
        private bool DrawEditorIntroUI()
        {
            if (itemsPool.Count <= 0)
            {
                EditorGUILayout.Space(position.height / 2, true);
                var headStyle = MeshEditorStylesUtility.TitleStyle;
                EditorGUILayout.LabelField("请从Hierarchy中", headStyle);
                EditorGUILayout.LabelField("拖动GameObject到此处", headStyle);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 绘制右键菜单
        /// </summary>
        /// <param name="evt"></param>
        private void OnContextClickHandler(Event evt)
        {
            if (itemsPool.Count <= 0) return; //如果没有任何编辑项
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("移除选中项"), false, RemoveAllSelect, "deleteItem");
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("选中全部"), false, CheckAllField, "CheckAll");
            menu.AddItem(new GUIContent("取消全部"), false, UnCheckAllField, "UnCheckAll");
            //menu.AddSeparator ("");
            //menu.AddItem (new GUIContent ("SubMenu/MenuItem3"), false, null, "item 3");
            menu.ShowAsContext();
            evt.Use();
        }

        #endregion

        #region 内部方法

        private void DropAreaGUI(float x, float y, float width, float height, Action DrawCallBack,
            Action<Object> DropCallBack, Action<Event> OnContextClick)
        {
            Event evt = Event.current;
            //Rect drop_area = GUILayoutUtility.GetRect(x, width, y, height, GUILayout.ExpandWidth(expandWidth));
            Rect drop_area = new Rect(x, y, width, height);
            GUILayout.BeginArea(drop_area);
            DrawCallBack?.Invoke();
            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            // Do On Drag Stuff here
                            DropCallBack?.Invoke(dragged_object);
                        }
                    }

                    break;
                case EventType.ContextClick:
                    OnContextClick?.Invoke(evt);
                    break;
            }

            GUILayout.EndArea();
        }


        #region 快捷键HotKey

        private void ExitWindowKeyboardCheck()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                var editorWindow = EditorWindow.mouseOverWindow;
                if (editorWindow && !editorWindow.docked && editorWindow == this)
                {
                    Close();
                    Event.current.keyCode = KeyCode.None;
                }
            }
        }

        #endregion

        #region CUAD操作

        /// <summary>
        /// 添加网格字段
        /// </summary>
        /// <param name="gmObj"></param>
        private void AddMeshField(GameObject gmObj)
        {
            if (gmObj != null)
            {
                MeshFilter editMeshFilter = null;
                MeshRenderer editMeshRenderer = null;
                if (gmObj.GetComponentInChildren<MeshFilter>())
                    editMeshFilter = gmObj.GetComponentInChildren<MeshFilter>();
                if (gmObj.GetComponentInChildren<MeshRenderer>())
                    editMeshRenderer = gmObj.GetComponentInChildren<MeshRenderer>();
                if (editMeshFilter != null && editMeshRenderer != null)
                    itemsPool.Add(MeshEditorItem.Create(editMeshFilter, editMeshRenderer));
            }
        }

        /// <summary>
        /// 添加网格项目
        /// </summary>
        /// <param name="obj"></param>
        private void AddMeshField(Object obj)
        {
            AddMeshField((GameObject) obj);
        }

        /// <summary>
        /// 移除网格项目
        /// </summary>
        /// <param name="item"></param>
        private bool RemoveMeshField(MeshEditorItem item)
        {
            if (UncheckField(item))
            {
                itemsPool.Remove(item);
                return true;
            }

            return false;
        }


        /// <summary>
        /// 添加选择项
        /// </summary>
        /// <param name="item"></param>
        private bool CheckField(MeshEditorItem item)
        {
            var hasInSelect = CheckItems.Contains(item);
            if (!hasInSelect)
            {
                item.Check = true;
                CheckItems.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除选中
        /// </summary>
        /// <param name="item"></param>
        private bool UncheckField(MeshEditorItem item)
        {
            var hasInSelect = CheckItems.Contains(item);
            if (hasInSelect)
            {
                foreach (var page in editorPages)
                {
                    var canExit = page.OnUnCheckFromMainWindow();
                    if (!canExit)
                    {
                        return false;
                    }
                }

                item.Check = false;
                if (!item.Check)
                {
                    CheckItems.Remove(item);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region MenuCallBack

        /// <summary>
        /// 勾选所有项目
        /// </summary>
        private void CheckAllField(object userdata)
        {
            int count = 0;
            foreach (var field in itemsPool)
                if (CheckField(field))
                    count++;
            window.ShowNotification(new GUIContent(String.Format("已选中{0}个", count)));
        }

        private void UnCheckAllField(object userdata)
        {
            int count = 0;
            foreach (var field in itemsPool)
                if (UncheckField(field))
                    count++;
            window.ShowNotification(new GUIContent(String.Format("已取消{0}个", count)));
        }

        /// <summary>
        /// 移除所有选中项
        /// </summary>
        private void RemoveAllSelect(object userdata)
        {
            int count = 0;
            for (int i = CheckItems.Count - 1; i >= 0; i--)
            {
                var field = CheckItems[i];
                if (field != null)
                {
                    if (RemoveMeshField(field))
                        count++;
                }
            }

            window.ShowNotification(new GUIContent(String.Format("已移除{0}个", count)));
        }

        #endregion

        #endregion
    }
}