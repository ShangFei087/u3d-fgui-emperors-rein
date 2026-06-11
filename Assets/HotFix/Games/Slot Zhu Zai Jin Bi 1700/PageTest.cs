using FairyGUI;

using GameMaker;

using System;

using System.Collections.Generic;

using UnityEngine;



namespace SlotZhuZaiJinBi1700

{

    public class PageTest : PageBase

    {

        public const string pkgName = "SlotZhuZaiJinBi1700";

        public const string resName = "PageTest";

        private const string PagPrefabBasePath =

            "Assets/GameRes/Games/Slot Zhu Zai Jin Bi 1700/Prefabs/TurnTable/";

        private const string PagDragon = "Dragon.pag";

        private const string PagUfo = "UFO.pag";

        private const string PagLogPrefix = "[1700 PageTest PAG]";

        private const bool PagUseFguiTexture = true;

        private const int PagFguiMaxDisplaySide = 0;

        private const int PagFguiFps = 60;

        private static readonly string[] PagAnchorNames = { "Pag1", "Pag2", "Pag3" };



        private sealed class PagSlot

        {

            public GComponent Anchor;

            public GameObject Clone;

            public PagSlotBinding Binding;

        }



        private readonly Dictionary<string, GameObject> _pagPrefabTemplates = new Dictionary<string, GameObject>();

        private readonly Dictionary<string, PagSlot> _pagSlots = new Dictionary<string, PagSlot>();

        private bool _uiBound;

        private Coroutine _playCoroutine;



        protected override void OnInit()

        {

            base.OnInit();



            int count = PagAnchorNames.Length;

            Action callback = () =>

            {

                if (--count == 0)

                {

                    isInit = true;

                    InitParam();

                }

            };



            for (int i = 0; i < PagAnchorNames.Length; i++)

            {

                string anchorName = PagAnchorNames[i];

                string prefabPath = $"{PagPrefabBasePath}{anchorName}.prefab";

                ResourceManager02.Instance.LoadAsset<GameObject>(prefabPath, (GameObject template) =>

                {

                    if (!_pagPrefabTemplates.ContainsKey(anchorName) && template != null)

                    {

                        _pagPrefabTemplates[anchorName] = template;

                        Debug.Log($"{PagLogPrefix} {anchorName} prefab loaded");

                    }



                    callback();

                });

            }

        }



        public override void OnOpen(PageName name, EventData data)

        {

            base.OnOpen(name, data);

            InitParam();

        }



        public override void OnClose(EventData data = null)

        {

            if (_playCoroutine != null)

            {

                PagCallbackHub.Instance.StopRunCoroutine(_playCoroutine);

                _playCoroutine = null;

            }



            StopAllTestPag();

            ClearButtons();

            ReleaseAllTurnTableWrappers();

            base.OnClose(data);

        }



        public override void OnTop()

        {

            DebugUtils.Log($"i am top {name}");

        }



        public override void InitParam()

        {

            if (!isInit || !isOpen)

            {

                return;

            }



            BindButtons();

            EnsureAllPagSetup();



            if (GetPagBinding("Pag1") != null)

            {

                PagPathHelper.WarmupPagCache(PagCallbackHub.Instance, PagDragon);

                PagPathHelper.WarmupPagCache(PagCallbackHub.Instance, PagUfo);

            }

        }



        private void BindButtons()

        {

            if (_uiBound)

            {

                return;

            }



            BindPagButton("1", PagDragon, "Pag2");

            BindPagButton("2", PagUfo, "Pag2");

            BindPagButton("3", PagDragon, "Pag1", "Pag3");

            BindPagButton("4", PagUfo, "Pag1", "Pag3");

            BindPagButton("5", PagDragon, "Pag1", "Pag2", "Pag3");

            BindPagButton("6", PagUfo, "Pag1", "Pag2", "Pag3");

            _uiBound = true;

        }



        private void BindPagButton(string btnName, string pagFileName, params string[] anchorNames)

        {

            GButton btn = contentPane.GetChild(btnName)?.asButton;

            if (btn == null)

            {

                Debug.LogWarning($"{PagLogPrefix} button missing: {btnName}");

                return;

            }



            btn.onClick.Clear();

            btn.onClick.Add(() => PlayTestPag(pagFileName, anchorNames));

        }



        private void ClearButtons()

        {

            if (!_uiBound || contentPane == null)

            {

                return;

            }



            for (int i = 1; i <= 6; i++)

            {

                contentPane.GetChild(i.ToString())?.asButton?.onClick.Clear();

            }



            _uiBound = false;

        }



        private void EnsureAllPagSetup()

        {

            if (_pagPrefabTemplates.Count < PagAnchorNames.Length)

            {

                Debug.LogWarning($"{PagLogPrefix} Pag prefabs not all ready ({_pagPrefabTemplates.Count}/{PagAnchorNames.Length})");

                return;

            }



            for (int i = 0; i < PagAnchorNames.Length; i++)

            {

                EnsurePagSetup(PagAnchorNames[i]);

            }

        }



        private PagSlot GetOrCreatePagSlot(string anchorName)

        {

            if (_pagSlots.TryGetValue(anchorName, out PagSlot existing))

            {

                return existing;

            }



            PagSlot slot = new PagSlot();

            _pagSlots[anchorName] = slot;

            return slot;

        }



        private void EnsureTurnTableWrapper(string anchorName)

        {

            if (!_pagPrefabTemplates.TryGetValue(anchorName, out GameObject prefabTemplate) || prefabTemplate == null)

            {

                return;

            }



            GComponent localPag = contentPane.GetChild(anchorName)?.asCom;

            if (localPag == null)

            {

                Debug.LogWarning($"{PagLogPrefix} Pag anchor missing: {anchorName}");

                return;

            }



            PagSlot slot = GetOrCreatePagSlot(anchorName);

            if (slot.Anchor == localPag && slot.Clone != null)

            {

                return;

            }



            slot.Binding?.Dispose();

            slot.Binding = null;



            GameCommon.FguiUtils.DeleteWrapper(slot.Anchor);

            if (slot.Clone != null)

            {

                UnityEngine.Object.Destroy(slot.Clone);

            }



            slot.Anchor = localPag;

            slot.Clone = UnityEngine.Object.Instantiate(prefabTemplate);

            slot.Clone.name = $"TT_{anchorName}";

            GameCommon.FguiUtils.AddWrapper(slot.Anchor, slot.Clone);

            Debug.Log($"{PagLogPrefix} TurnTable wrapper attached to {anchorName}");

        }



        private void ReleaseAllTurnTableWrappers()

        {

            foreach (KeyValuePair<string, PagSlot> pair in _pagSlots)

            {

                PagSlot slot = pair.Value;

                slot.Binding?.Dispose();

                slot.Binding = null;

                GameCommon.FguiUtils.DeleteWrapper(slot.Anchor);

                if (slot.Clone != null)

                {

                    UnityEngine.Object.Destroy(slot.Clone);

                }

            }



            _pagSlots.Clear();

        }



        private void EnsurePagSetup(string anchorName)

        {

            EnsureTurnTableWrapper(anchorName);

            PagSlot slot = GetOrCreatePagSlot(anchorName);

            if (slot.Anchor == null)

            {

                return;

            }



            if (slot.Binding == null)

            {

                slot.Binding = new PagSlotBinding($"TT_{anchorName}");

            }



            slot.Binding.Attach(slot.Anchor);

        }



        private PagSlotBinding GetPagBinding(string anchorName)

        {

            EnsurePagSetup(anchorName);

            return _pagSlots.TryGetValue(anchorName, out PagSlot slot) ? slot.Binding : null;

        }



        private void PlayTestPag(string pagFileName, params string[] anchorNames)

        {

            if (anchorNames == null || anchorNames.Length == 0)

            {

                Debug.LogWarning($"{PagLogPrefix} Play skipped: no anchor");

                return;

            }



            if (_playCoroutine != null)

            {

                PagCallbackHub.Instance.StopRunCoroutine(_playCoroutine);

            }



            EnsureAllPagSetup();



            var bindings = new List<PagSlotBinding>(anchorNames.Length);

            for (int i = 0; i < anchorNames.Length; i++)

            {

                PagSlotBinding binding = GetPagBinding(anchorNames[i]);

                if (binding != null)

                {

                    bindings.Add(binding);

                }

            }



            _playCoroutine = PagGroupPlayer.PlayOnSlots(

                pagFileName,

                bindings,

                TryBuildPagLayoutExtra,

                PagUseFguiTexture,

                PagFguiMaxDisplaySide,

                PagFguiFps,

                PagLogPrefix);

        }



        private void StopTestPag(string anchorName)

        {

            if (!_pagSlots.TryGetValue(anchorName, out PagSlot slot) || slot.Binding == null)

            {

                return;

            }



            slot.Binding.Stop(PagUseFguiTexture);

        }



        private void StopAllTestPag()

        {

            for (int i = 0; i < PagAnchorNames.Length; i++)

            {

                StopTestPag(PagAnchorNames[i]);

            }

        }



        private static bool TryBuildPagLayoutExtra(GComponent pagAnchor, out string extra, out string debugReason)

        {

            extra = null;

            debugReason = "unknown";



            if (pagAnchor == null)

            {

                debugReason = "Pag anchor is null";

                return false;

            }



            GGraph holder = pagAnchor.GetChild("holder")?.asGraph;

            GLoader example = pagAnchor.GetChild("example")?.asLoader;



            float localW = holder != null && holder.width > 0f ? holder.width : (example != null ? example.width : 200f);

            float localH = holder != null && holder.height > 0f ? holder.height : (example != null ? example.height : 200f);

            if (localW <= 0f || localH <= 0f)

            {

                debugReason = $"invalid size holder={holder?.width}x{holder?.height} example={example?.width}x{example?.height}";

                return false;

            }



            float rootW = GRoot.inst.width;

            float rootH = GRoot.inst.height;

            if (rootW <= 0f || rootH <= 0f)

            {

                debugReason = $"invalid GRoot size {rootW}x{rootH}";

                return false;

            }



            float normW = Screen.width > 0f ? Screen.width : rootW;

            float normH = Screen.height > 0f ? Screen.height : rootH;



            GObject layoutTarget = holder != null && holder.width > 0f ? (GObject)holder : pagAnchor;

            Rect globalRect = layoutTarget.LocalToGlobal(new Rect(0f, 0f, localW, localH));

            float x = Mathf.Clamp01(globalRect.xMin / normW);

            float y = Mathf.Clamp01(globalRect.yMin / normH);

            float w = Mathf.Clamp(globalRect.width / normW, 0.01f, 1f - x);

            float h = Mathf.Clamp(globalRect.height / normH, 0.01f, 1f - y);



            if (w * h < 0.01f)

            {

                debugReason = $"rect too small w={w:F4} h={h:F4}";

                return false;

            }



            extra = string.Format(System.Globalization.CultureInfo.InvariantCulture,

                "{0:F4},{1:F4},{2:F4},{3:F4}", x, y, w, h);

            debugReason = $"ok target={layoutTarget.name} global={globalRect}";

            return true;

        }

    }

}


