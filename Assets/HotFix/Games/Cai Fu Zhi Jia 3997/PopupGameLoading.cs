using FairyGUI;
using GameMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaiFuZhiJia_3997
{
    public class PopupGameLoading : MachinePageBase
    {
        public new const string pkgName = "CaiFuZhiJia";
        public new const string resName = "PopupGameLoading";

        private const string SpinePrefabsPath =
            "Assets/GameRes/Games/Cai Fu Zhi Jia 3997/Prefabs/PopupGameLoading/SpinePrefabs/";

        // 初始化
        private int _totalResCount = -1;
        private bool _isInitialized = false;

        // 加载条功能
        private Coroutine _corLoading = null;
        private MonoHelper _monoHelper = null;
        private GameObject _monoHelperObj = null;

        // UI组件
        private GSlider _loadSlider = null;

        // 资源加载
        private readonly List<GameObject> _spinePrefabList = new List<GameObject>(); // 存储异步加载的预制体
        private readonly List<GameObject> _cloneSpinePrefabList = new List<GameObject>(); // 存储克隆出来的预制体
        private readonly List<GComponent> _compareAnchorList = new List<GComponent>() { null, null }; // 多分支用做参考的组件

        private readonly List<string> _spinePrefabNameList =
            new List<string>() { "BGSpine.prefab", "CenterSpine.prefab" };

        private readonly List<string> _anchorComNameList = new List<string>() { "anchorBG", "anchorCenter" };


        protected override void OnInit()
        {
            contentPane = UIPackage.CreateObject(pkgName, resName).asCom;
            base.OnInit();

            InitUICom();
            _totalResCount = _spinePrefabNameList.Count;
            LoadPrefabs(SpinePrefabsPath, _spinePrefabNameList, _spinePrefabList, ResLoadedCallBack);
        }

        public override void InitParam()
        {
            ResetView();
            if (!_isInitialized) return;
            StartLoading();
            InitPrefabs(contentPane, _anchorComNameList, _compareAnchorList, _spinePrefabList,
                _cloneSpinePrefabList, null);
        }

        public override void OnOpen(PageName currentPageName, EventData eventData)
        {
            base.OnOpen(currentPageName, eventData);
            InitParam();
        }

        public override void OnClose(EventData eventData = null)
        {
            base.OnClose(eventData);
            ResetView();
        }

        private void InitUICom()
        {
            _loadSlider = contentPane.GetChild("sliderLoading").asSlider;
            _loadSlider.touchable = false;
            _loadSlider.value = 0;
            _loadSlider.max = 1;
        }

        void StartLoading()
        {
            if (_monoHelperObj == null)
            {
                _monoHelperObj = new GameObject("Loading_MonoHelper");
                _monoHelper = _monoHelperObj.AddComponent<MonoHelper>();
            }

            if (_corLoading != null && _monoHelper != null) _monoHelper.StopCoroutine(_corLoading);
            _corLoading = _monoHelper.StartCoroutine(StartLoadingCoroutine());
        }

        IEnumerator StartLoadingCoroutine()
        {
            PageManager.Instance.PreloadPage(PageName.CaiFuZhiJiaPageGameMain, null);
            float currentTime = 0;
            while (currentTime < 5)
            {
                _loadSlider.value = currentTime / 5;
                yield return null;
                currentTime += Time.deltaTime;
            }

            _loadSlider.value = 1;
            CloseSelf(null);
            PageManager.Instance.OpenPage(PageName.CaiFuZhiJiaPageGameMain);
        }

        void ResLoadedCallBack()
        {
            if (--_totalResCount == 0)
            {
                _isInitialized = true;
                InitParam();
            }
        }

        /// <summary>
        /// 加载预制体并未实例化
        /// </summary>
        /// <param name="prefabPath">预制体路径</param>
        /// <param name="prefabNameList">预制体名称List</param>
        /// <param name="prefabList">存储加载预制体的List</param>
        /// <param name="callback">加载完成的回调函数</param>
        private void LoadPrefabs(string prefabPath, List<string> prefabNameList, List<GameObject> prefabList,
            Action callback = null)
        {
            // 重置预制体List，为保证异步加载不打乱加载顺序，改为强制赋值
            prefabList.Clear();
            for (int j = 0; j < prefabNameList.Count; j++)
            {
                prefabList.Add(null);
            }

            for (int i = 0; i < prefabNameList.Count; i++)
            {
                int currentLoadIndex = i;
                string path = prefabPath + prefabNameList[currentLoadIndex];
                ResourceManager02.Instance.LoadAsset<GameObject>(path, (clone) =>
                {
                    prefabList[currentLoadIndex] = clone;
                    callback?.Invoke();
                });
            }
        }

        /// <summary>
        /// 实例化预制体
        /// </summary>
        /// <param name="mainPanel">主面板组件</param>
        /// <param name="gComNameList">需要加载的锚点名称</param>
        /// <param name="compareComList">多分支对比List</param>
        /// <param name="prefabList">加载好的预制体</param>
        /// <param name="clonePrefabList">存储真正实例化的预制体的List</param>
        /// <param name="callback">避免加载的时候对预制体进行其他操作，增加一个委托</param>
        private void InitPrefabs(GComponent mainPanel, List<string> gComNameList, List<GComponent> compareComList,
            List<GameObject> prefabList,
            List<GameObject> clonePrefabList, Action callback)
        {
            clonePrefabList.Clear();
            if (prefabList == null) return;

            for (int i = 0; i < gComNameList.Count; i++)
            {
                GComponent currentCom = mainPanel.GetChild(gComNameList[i]).asCom;
                if (currentCom != compareComList[i])
                {
                    GameCommon.FguiUtils.DeleteWrapper(compareComList[i]);
                    compareComList[i] = currentCom;
                    if (callback == null)
                    {
                        GameObject currentObj = Object.Instantiate(prefabList[i]);
                        clonePrefabList.Add(currentObj);
                        GameCommon.FguiUtils.AddWrapper(currentCom, currentObj);
                    }
                    else
                        callback.Invoke();
                }
            }
        }

        private void ResetView()
        {
            if (_corLoading != null)
            {
                _monoHelper.StopCoroutine(_corLoading);
                _corLoading = null;
            }

            foreach (var obj in _cloneSpinePrefabList.ToArray())
            {
                if (obj != null)
                    Object.Destroy(obj);
            }

            _cloneSpinePrefabList.Clear();

            if (_monoHelperObj != null)
            {
                Object.Destroy(_monoHelperObj);
                _monoHelperObj = null;
                _monoHelper = null;
            }

            if (_loadSlider != null)
                _loadSlider.value = 0;

            for (int i = 0; i < _compareAnchorList.Count; i++)
            {
                _compareAnchorList[i] = null;
            }
        }
    }
}