
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

//为了编辑器中再次打开场景直接能看到效果， 所以这里用了ExecuteInEditMode
[ExecuteInEditMode, DisallowMultipleComponent]
public class DuplicateLightmap : MonoBehaviour
{
    public Transform frome;


	public bool isEnableEdit = false;
	public bool updateNow = false;

    void Awake()
    {
        copy(frome, transform);
    }

    //拷贝LightMap信息
    void copy(Transform frome, Transform to)
    {
        if (frome && to)
        {
			
            if (frome.childCount == to.childCount)
            {
                Renderer f = frome.GetComponent<MeshRenderer>();
                Renderer t = to.GetComponent<MeshRenderer>();
                if (f && t)
                {
					t.realtimeLightmapIndex = f.realtimeLightmapIndex;
					t.realtimeLightmapScaleOffset = f.realtimeLightmapScaleOffset;

                    t.lightmapIndex = f.lightmapIndex;
                    t.lightmapScaleOffset = f.lightmapScaleOffset;
                }
                for (int i = 0; i < frome.childCount; i++)
                {
                    if (frome.childCount == to.childCount)
                    {
                        Transform cf = frome.GetChild(i);
                        Transform ct = to.GetChild(i);
                        if (frome.childCount == to.childCount)
                        {
                            f = cf.GetComponent<MeshRenderer>();
                            t = ct.GetComponent<MeshRenderer>();
                            if (f && t)
                            {
								t.realtimeLightmapIndex = f.realtimeLightmapIndex;
								t.realtimeLightmapScaleOffset = f.realtimeLightmapScaleOffset;

                                t.lightmapIndex = f.lightmapIndex;
                                t.lightmapScaleOffset = f.lightmapScaleOffset;
                            }
                            copy(cf, ct);
                        }
                    }
                }
            }
        }
    }



#if UNITY_EDITOR
    [MenuItem("GameObject/Duplicate Object %#d")]
    static void DuplicateObject()
    {
        Transform[] selectionChanged = Selection.GetTransforms(SelectionMode.TopLevel);

        //复制
        SceneView.lastActiveSceneView.Focus();
        EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));


        Transform[] copys = Selection.GetTransforms(SelectionMode.TopLevel);

        if (selectionChanged.Length == copys.Length)
        {
            for (int i = 0; i < copys.Length; i++)
            {

                //限制拷贝出来的子对象不可被编辑，避免美术错误操作
                GameObject root = copys[i].gameObject;
                Transform[] childs = root.GetComponentsInChildren<Transform>();
                foreach (Transform child in childs)
                {
                    if (child.gameObject.GetInstanceID() != root.GetInstanceID())
                    {
                        child.gameObject.hideFlags = HideFlags.NotEditable;
                    }
                }

                DuplicateLightmap selectObj = selectionChanged[i].GetComponent<DuplicateLightmap>();
                int childCount = selectionChanged[i].GetComponentsInChildren<DuplicateLightmap>().Length;
                int parentCount = selectionChanged[i].GetComponentsInParent<DuplicateLightmap>().Length;
                bool isDuplicate = (selectObj && childCount == 1 && parentCount == 1) || (!selectObj && childCount == 0 && parentCount == 0);
                DuplicateLightmap obj = copys[i].GetComponent<DuplicateLightmap>() ?? copys[i].gameObject.AddComponent<DuplicateLightmap>();

                if (isDuplicate)
                {
                    if (selectObj && selectObj.frome)
                    {
                        obj.frome = selectObj.frome;
                    }
                    else
                    {
                        obj.frome = selectionChanged[i];
                    }
                    obj.Refresh();
                }
                else
                {
                    GameObject.DestroyImmediate(copys[i].gameObject);
                    Debug.LogError("不支持循环嵌套");
                }
            }
        }
    }

    [InitializeOnLoadMethod]
    static void StartInitializeOnLoadMethod()
    {
        //烘培完毕后重新刷新
        Lightmapping.completed = delegate()
        {
            DuplicateLightmap[] duplicates = GameObject.FindObjectsOfType<DuplicateLightmap>();
            for (int i = 0; i < duplicates.Length; i++)
            {
                duplicates[i].Refresh();
            }
        };
    }

    static void ChangeEditorFlags(GameObject gameObject, StaticEditorFlags floags)
    {
        if (gameObject)
        {
            GameObjectUtility.SetStaticEditorFlags(gameObject, floags);
            foreach (Transform t in gameObject.transform)
            {
                ChangeEditorFlags(t.gameObject, floags);
            }
        }
    }

    public void Refresh()
    {
        copy(frome, transform);
        ChangeEditorFlags(gameObject,
            StaticEditorFlags.BatchingStatic |
            StaticEditorFlags.OccluderStatic |
            StaticEditorFlags.OccludeeStatic |
            StaticEditorFlags.NavigationStatic |
            StaticEditorFlags.OffMeshLinkGeneration |
            StaticEditorFlags.ReflectionProbeStatic);
    }

	void Update() {
		if (updateNow) {


				Transform[] copys = Selection.GetTransforms (SelectionMode.TopLevel);

				for (int i = 0; i < copys.Length; i++) {


					GameObject root = copys [i].gameObject;
					Transform[] childs = root.GetComponentsInChildren<Transform> ();
					foreach (Transform child in childs) {
						if (child.gameObject.GetInstanceID () != root.GetInstanceID ()) {
							if (isEnableEdit)
								child.gameObject.hideFlags = HideFlags.None;
							else
								child.gameObject.hideFlags = HideFlags.NotEditable;
						}
					}
				}

			updateNow = false;
		}

	}
#endif
}