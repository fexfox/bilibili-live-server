using System.Linq;
using CreativeVeinStudio.Simple_Pool_Manager.Helpers;
using CreativeVeinStudio.Simple_Pool_Manager.Models;
using UnityEditor;
using UnityEngine;

namespace CreativeVeinStudio.Simple_Pool_Manager.Editor
{
    [CustomEditor(typeof(SpManager))]
    public class SpEditorLayout : UnityEditor.Editor
    {
        private SpManager _spm;
        private SerializedObject _serializedSpm;
        private SerializedProperty _poolCollection;
        private SerializedProperty _searchTxt;
        private SerializedProperty _prefabs;

        private readonly PoolProperties _poolColInfo = new PoolProperties() {PoolName = "Pool Name - Please change"};

        private Texture2D _spmLogo;
        private Texture2D _spmHeader;
        private Texture2D _spmBg;
        private bool _showCollection;

        private const int MaxInstantiatingQty = 500;

        private void Awake()
        {
            _spmLogo = Resources.Load<Texture2D>("SPMLogo");
            _spmHeader = Resources.Load<Texture2D>("SPM_Header");
            _spmBg = Resources.Load<Texture2D>("SPM_BG");
        }

        private void OnEnable()
        {
            _spm = (SpManager) target;
            _serializedSpm = new SerializedObject(_spm);
            _poolCollection = _serializedSpm.FindProperty("collection");
            _searchTxt = _serializedSpm.FindProperty("searchTxt");
            _prefabs = _serializedSpm.FindProperty("prefabs");
        }

        public override void OnInspectorGUI()
        {
            _serializedSpm.Update();

            // ****************************************
            // Horizontal SPManager Section
            // ****************************************
            EditorGUILayout.LabelField("", "", new GUIStyle() {normal = {background = _spmLogo}},
                GUILayout.Height(EditorGUIUtility.currentViewWidth * 0.10f),
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 24));

            // ****************************************
            // BOX AREA - Properties Section - Add & Clear list
            // ****************************************
            UIAddPoolItem();

            // ****************************************
            // Pool Collection List Display Section
            // ****************************************
            UIPoolCollection();
            UIPoolListDisplay();

            // apply changes done to the properties
            _serializedSpm.ApplyModifiedProperties();
        }

        private void UIAddPoolItem()
        {
            EditorGUILayout.BeginVertical(new GUIStyle() {normal = {background = _spmBg}},
                new GUILayoutOption[] {GUILayout.ExpandWidth(true), GUILayout.Height(150)});
            EditorGUILayout.Space(10);

            _poolColInfo.PoolName = EditorGUILayout.TextField("Name",
                _poolColInfo.PoolName, GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal();
            var toggleContent = new GUIContent(_poolColInfo.ActiveFirstItem ? "YES" : "NO");
            var toggleStyle = CustomEditorStyles.ToggleStyle();
            toggleStyle.fontStyle = FontStyle.Bold;
            toggleStyle.normal.background = _poolColInfo.ActiveFirstItem
                ? CustomEditorStyles.MakeTexture(50, 5, Color.HSVToRGB(0.45f, 1f, 0.5f))
                : CustomEditorStyles.MakeTexture(50, 5, Color.gray);
            EditorGUILayout.LabelField("Active First Item");
            if (GUILayout.Button(toggleContent, toggleStyle))
            {
                _poolColInfo.ActiveFirstItem = !_poolColInfo.ActiveFirstItem;
            }

            EditorGUILayout.EndHorizontal();

            _poolColInfo.InstantiateQty = (int) EditorGUILayout.Slider("Instantiate Qty",
                _poolColInfo.InstantiateQty, 1, MaxInstantiatingQty);
            _poolColInfo.ParentTransform = (Transform) EditorGUILayout.ObjectField("Parent Transform",
                _poolColInfo.ParentTransform, typeof(Transform), true);
            // _itemPrefab = (GameObject) EditorGUILayout.ObjectField("Prefab", _itemPrefab, typeof(GameObject), false);
            EditorGUILayout.PropertyField(_prefabs, true);
            _poolColInfo.Items = _spm.prefabs;

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            var buttonStyle = EditorStyles.miniButton;
            buttonStyle.normal.background = CustomEditorStyles.MakeTexture(1, 1, Color.HSVToRGB(0.45f, 1, 0.5f));
            buttonStyle.fixedHeight = 40;
            buttonStyle.stretchWidth = true;
            buttonStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("Add to list", buttonStyle))
            {
                if (string.IsNullOrEmpty(_poolColInfo.PoolName) ||
                    _poolColInfo.PoolName.Contains("Pool Name"))
                {
                    Debug.Log("No name was provided for the Pool Item. Please add a name");
                    return;
                }

                if (_poolColInfo.Items.Count <= 0)
                {
                    Debug.Log("Please provide a prefab to instantiate for this Pool");
                    return;
                }

                if (_poolColInfo.Items.Any(x => x == null))
                {
                    Debug.Log("Please provide a prefab to instantiate for this Pool");
                    return;
                }

                _spm.AddToCollection(_poolColInfo);
                ResetValues();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void UIPoolCollection()
        {
            EditorGUILayout.BeginHorizontal(CustomEditorStyles.CustomBoxStyle(new Vector4(0, 0, 10, 0),
                Vector4.one * 5, _spmHeader));
            EditorGUILayout.LabelField("Pool Collection",
                CustomEditorStyles.CustomLabel(TextAnchor.MiddleLeft, 14, Color.white, Vector4.zero, Vector4.one));
            EditorGUILayout.LabelField($"QTY: {(_spm.collection.Count)}",
                CustomEditorStyles.CustomLabel(TextAnchor.MiddleLeft, 14, Color.white, Vector4.zero, Vector4.one * 5),
                new GUILayoutOption[] {GUILayout.Width(50), GUILayout.ExpandWidth(true)});

            GUI.backgroundColor = Color.red;

            if (_spm.collection.Count > 0)
            {
                if (!_showCollection)
                {
                    GUI.backgroundColor = Color.cyan;
                    if (GUILayout.Button("View List",
                            new GUILayoutOption[] {GUILayout.Width(65), GUILayout.Height(20)}))
                    {
                        _showCollection = true;
                    }
                }
                else
                {
                    GUI.backgroundColor = Color.grey;
                    if (GUILayout.Button("Hide List",
                            new GUILayoutOption[] {GUILayout.Width(65), GUILayout.Height(20)}))
                    {
                        _showCollection = false;
                    }
                }

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button(new GUIContent("Disable All", "Disables all the active items"),
                        GUILayout.ExpandHeight(true)))
                {
                    _spm.DisableAllExistingInPoolObjects();
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(new GUIContent("X", "Clears the entire Pool list"),
                        new GUILayoutOption[] {GUILayout.Width(40), GUILayout.Height(20)}))
                {
                    _spm.collection.Clear();
                }

                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void UIPoolListDisplay()
        {
            if (_poolCollection == null || _poolCollection.arraySize < 0) return;
            if (EditorGUILayout.BeginFadeGroup(_showCollection && _poolCollection.arraySize > 0 ? 1 : 0))
            {
                var pos = EditorGUILayout.BeginHorizontal(
                    CustomEditorStyles.CustomBoxStyle(Vector4.zero, Vector4.one * 5),
                    GUILayout.Height(30));
                EditorGUI.DrawRect(pos, Color.black);

                _searchTxt.stringValue = EditorGUILayout.TextField("Search", _searchTxt.stringValue);
                if (GUILayout.Button("clear", GUILayout.Width(75)))
                {
                    _searchTxt.stringValue = "";
                }

                EditorGUILayout.EndHorizontal();

                if (_searchTxt.stringValue.Length >= 3)
                {
                    SearchResult(_searchTxt.stringValue);
                }
                else
                {
                    ShowAllListItems();
                }
            }

            EditorGUILayout.EndFadeGroup();
        }

        private void ShowAllListItems()
        {
            for (int i = 0; i <= (_poolCollection.arraySize - 1); i++)
            {
                var col = _poolCollection.GetArrayElementAtIndex(i);
                var itemName = col.FindPropertyRelative("poolName");
                var activeFirst = col.FindPropertyRelative("activeFirstItem");
                var instQty = col.FindPropertyRelative("instantiateQty");
                var colItem = col.FindPropertyRelative("item");
                var parentTrans = col.FindPropertyRelative("parentTransform");
                var toggleShowHide = col.FindPropertyRelative("toggleShowHide");

                EditorGUILayout.BeginVertical(
                    CustomEditorStyles.CustomAreaStyle(Vector4.zero, Vector4.one * 5, _spmBg));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{itemName.stringValue}",
                    CustomEditorStyles.CustomLabel(TextAnchor.MiddleLeft, 14, Color.white, Vector4.zero, Vector4.one));
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Toggle View", GUILayout.ExpandHeight(true)))
                {
                    toggleShowHide.boolValue = !toggleShowHide.boolValue;
                }

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Disable", GUILayout.ExpandHeight(true)))
                {
                    _spm.DisablePoolByCategory(itemName.stringValue);
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete Item", GUILayout.ExpandHeight(true)))
                {
                    _spm.collection.RemoveAt(i);
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (toggleShowHide.boolValue)
                {
                    itemName.stringValue = EditorGUILayout.TextField("Name", itemName.stringValue);
                    activeFirst.boolValue = EditorGUILayout.Toggle("Active First Item", activeFirst.boolValue);
                    instQty.intValue =
                        (int) EditorGUILayout.Slider("Instantiate Qty", instQty.intValue, 1, MaxInstantiatingQty);
                    parentTrans.objectReferenceValue = EditorGUILayout.ObjectField("Parent Transform",
                        parentTrans.objectReferenceValue, typeof(Transform), true);
                    // colItem.objectReferenceValue = EditorGUILayout.ObjectField("Prefab", colItem.objectReferenceValue,
                    //     typeof(GameObject), false);
                    EditorGUILayout.PropertyField(colItem, true);
                }

                EditorGUILayout.EndVertical();

                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(5);
            }
        }

        private void SearchResult(string val)
        {
            for (int i = 0; i < _poolCollection.arraySize; i++)
            {
                var col = _poolCollection.GetArrayElementAtIndex(i);
                var poolName = col.FindPropertyRelative("poolName");

                if (!poolName.stringValue.ToLower().Contains(val.ToLower())) continue;
                var activeFirst = col.FindPropertyRelative("activeFirstItem");
                var instQty = col.FindPropertyRelative("instantiateQty");
                var parentTrans = col.FindPropertyRelative("parentTransform");
                var colItem = col.FindPropertyRelative("item");
                var toggleShowHide = col.FindPropertyRelative("toggleShowHide");

                EditorGUILayout.BeginVertical(
                    CustomEditorStyles.CustomAreaStyle(Vector4.zero, Vector4.one * 5, _spmBg));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{poolName.stringValue}",
                    CustomEditorStyles.CustomLabel(TextAnchor.MiddleLeft, 14, Color.white, Vector4.zero, Vector4.one));
                GUI.backgroundColor = Color.cyan;
                if (GUILayout.Button("Toggle View", GUILayout.ExpandHeight(true)))
                {
                    toggleShowHide.boolValue = !toggleShowHide.boolValue;
                }

                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Disable", GUILayout.ExpandHeight(true)))
                {
                    _spm.DisablePoolByCategory(poolName.stringValue);
                }

                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Delete Item", GUILayout.ExpandHeight(true)))
                {
                    _spm.collection.RemoveAt(i);
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (toggleShowHide.boolValue)
                {
                    poolName.stringValue = EditorGUILayout.TextField("Name", poolName.stringValue);
                    activeFirst.boolValue = EditorGUILayout.Toggle("Active First Item", activeFirst.boolValue);
                    instQty.intValue =
                        (int) EditorGUILayout.Slider("Instantiate Qty", instQty.intValue, 1, MaxInstantiatingQty);
                    parentTrans.objectReferenceValue = EditorGUILayout.ObjectField("Parent Transform",
                        parentTrans.objectReferenceValue, typeof(Transform), true);
                    // colItem.objectReferenceValue = EditorGUILayout.ObjectField("Prefab", colItem.objectReferenceValue,
                    //     typeof(GameObject), false);
                    EditorGUILayout.PropertyField(colItem, true);
                }

                EditorGUILayout.EndVertical();

                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(5);
            }
        }

        private void ResetValues()
        {
            _poolColInfo.Items.Clear();
            _poolColInfo.PoolName = "Pool Name - Please change";
            _poolColInfo.ActiveFirstItem = false;
            _poolColInfo.InstantiateQty = 1;
            _poolColInfo.ParentTransform = null;
        }
    }
}