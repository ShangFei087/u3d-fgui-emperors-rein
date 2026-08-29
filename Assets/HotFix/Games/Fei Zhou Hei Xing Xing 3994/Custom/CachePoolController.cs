using FairyGUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix.Games.Fei_Zhou_Hei_Xing_Xing_3994.Custom
{
    /// <summary>
    /// GComponent 缓存池。
    /// 缓存/取出由池管理；创建（含挂载自定义 GameObject）由调用方通过工厂传入。
    /// GComponent 下挂载的子 GameObject 跟随父物体一起移动，无需池额外处理。
    /// </summary>
    public class CachePoolController : MonoSingleton<CachePoolController>
    {
        private GameObject _poolRoot;
        private readonly Dictionary<string, Transform> _keyParents = new Dictionary<string, Transform>();
        private readonly Dictionary<string, Stack<GComponent>> _poolDic = new Dictionary<string, Stack<GComponent>>();

        private GameObject PoolRoot
        {
            get
            {
                if (_poolRoot == null)
                {
                    _poolRoot = new GameObject("[BufferPool]");
                    _poolRoot.SetActive(false);
                    DontDestroyOnLoad(_poolRoot);
                }

                return _poolRoot;
            }
        }

        private Transform GetKeyParent(string key)
        {
            if (!_keyParents.TryGetValue(key, out Transform parent))
            {
                GameObject go = new GameObject($"[{key}]");
                go.transform.SetParent(PoolRoot.transform);
                _keyParents[key] = go.transform;
                parent = go.transform;
            }

            return parent;
        }

        /// <summary>回收 GComponent 到池中</summary>
        public void PushCom(string key, GComponent com)
        {
            if (com == null) return;

            com.RemoveFromParent();
            com.visible = false;

            if (com.displayObject?.gameObject != null)
            {
                com.displayObject.gameObject.transform.SetParent(GetKeyParent(key));
            }

            if (!_poolDic.TryGetValue(key, out Stack<GComponent> stack))
            {
                stack = new Stack<GComponent>();
                _poolDic[key] = stack;
            }

            stack.Push(com);
        }

        /// <summary>从池中取出 GComponent，池空返回 null</summary>
        private GComponent GetCom(string key)
        {
            if (!_poolDic.TryGetValue(key, out Stack<GComponent> stack) || stack.Count == 0)
                return null;

            return stack.Pop();
        }

        /// <summary>获取或创建：池中有则取出，无则通过工厂新建。取出后自动添加到 popParentCom 下。</summary>
        public GComponent PopCom(string key, GComponent popParentCom, Func<GComponent> factory)
        {
            GComponent com = GetCom(key) ?? factory?.Invoke();
            if (com != null && popParentCom != null)
            {
                popParentCom.AddChild(com);
            }

            return com;
        }

        /// <summary>清空池，销毁所有已缓存 GComponent</summary>
        public void ClearPool()
        {
            foreach (Stack<GComponent> stack in _poolDic.Values)
            {
                while (stack.Count > 0)
                {
                    GComponent com = stack.Pop();
                    com?.Dispose();
                }
            }

            _poolDic.Clear();
            _keyParents.Clear();

            if (_poolRoot == null)
            {
                return;
            }

            Destroy(_poolRoot);
            _poolRoot = null;
        }
    }
}