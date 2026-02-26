using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonoWeakSelectSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static Dictionary<Type, Dictionary<string, GameObject>> goSelect = new Dictionary<Type, Dictionary<string, GameObject>>();

    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                //var founds = GameObject.FindObjectsOfType(typeof(T));
                //var foundsAll = Resources.FindObjectsOfTypeAll(typeof(T));
                var foundsSceneAll = FindSceneObject(typeof(T));
                Type tp = typeof(T);
                goSelect.Remove(tp);
                if (foundsSceneAll != null && foundsSceneAll.Length > 0)
                {
                    Dictionary<string, GameObject> dic = new Dictionary<string, GameObject>();
                    goSelect.Add(tp, dic);
                    foreach (var comp in foundsSceneAll)
                    {
                        string key = ((T)comp).gameObject.name;
                        string temp = key;
                        if (dic.ContainsKey(temp))
                        {
                            int numb = 1;
                            do
                            {
                                temp = $"{key} ({numb})";
                                numb++;
                            } while (dic.ContainsKey(temp));
                        }
                        ((T)comp).gameObject.name = temp;
                        dic.Add(temp, ((T)comp).gameObject);
                    }
                }

                var founds = FindObjectsOfType(typeof(T));
                if (founds.Length > 0)
                {
                    _instance = (T)founds[0];
                }
            }
            return _instance;
        }
    }
    protected virtual void OnDisable()
    {
        _instance = null;
    }

    protected virtual void OnDestroy()
    {
        _instance = null;
    }

    [Button]
    public void SelectData(string name)
    {
        Type tp = typeof(T);
        if (goSelect.ContainsKey(tp) && goSelect[tp].ContainsKey(name))
        {
            foreach (KeyValuePair<string, GameObject> kv in goSelect[tp])
            {
                if (kv.Key == name)
                    continue;
                kv.Value.SetActive(false);
            }
            goSelect[tp][name].SetActive(true);
        }
    }
    [Button]
    void TestShowDataSelectItem()
    {
        Type tp = typeof(T);
        if (goSelect.ContainsKey(tp))
        {
            foreach (KeyValuePair<string, GameObject> kv in goSelect[tp])
            {
                Debug.Log($"Data({tp.Name}) -- {kv.Value}");
            }

            if (_instance != null)
            {
                Debug.Log($"Now Data({tp.Name}) -- {_instance.transform.name}");
            }
        }
    }




    // 获取场景中所有目标对象（包括不激活的对象）不包括Prefabs:
    static List<TComponent> FindSceneObject<TComponent>(string _SceneName = "") where TComponent : UnityEngine.Component
    {
        if (string.IsNullOrEmpty(_SceneName))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            _SceneName = currentScene.name;
        }
        //Debug.Log($"==@SceneName = {_SceneName}");

        List<TComponent> objectsInScene = new List<TComponent>();
        foreach (var go in Resources.FindObjectsOfTypeAll<TComponent>())
        {
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;
#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(go.transform.root.gameObject))// 如果对象位于Scene中，则返回false
                continue;
#endif
            if (_SceneName != go.gameObject.scene.name)
                continue;
            //Debug.LogFormat("gameObject:{0},scene:{1}", go.gameObject.name, go.gameObject.scene.name);
            objectsInScene.Add(go);
        }
        return objectsInScene;
    }

    static UnityEngine.Object[] FindSceneObject(Type type, string _SceneName = "")
    {
        if (string.IsNullOrEmpty(_SceneName))
        {
            Scene currentScene = SceneManager.GetActiveScene();
            _SceneName = currentScene.name;
        }
        //Debug.Log($"==@SceneName = {_SceneName}");//==@SceneName = NewScene

        List<UnityEngine.Object> objectsInScene = new List<UnityEngine.Object>();
        foreach (var go in Resources.FindObjectsOfTypeAll(type))
        {
            Component _go = ((Component)go);
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;
#if UNITY_EDITOR
            if (EditorUtility.IsPersistent(_go.transform.root.gameObject))// 如果对象位于Scene中，则返回false
                continue;
#endif
            if (_SceneName != _go.gameObject.scene.name)
                continue;
            //Debug.LogFormat("gameObject:{0},scene:{1}", _go.gameObject.name, _go.gameObject.scene.name);
            objectsInScene.Add(go);
        }
        return objectsInScene.ToArray();
    }
}
