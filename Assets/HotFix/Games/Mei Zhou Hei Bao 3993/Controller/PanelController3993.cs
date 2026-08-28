using FairyGUI;
using GameMaker;
using SlotMaker;
using System;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public class PanelController3993 : PanelBaseController
    {
        protected override string PanelPackagePath => "Assets/GameRes/Panel/Panel3993/FGUIs";
        protected override string ShortSpinPrefabPath => "Assets/GameRes/Panel/Panel3993/Prefabs/Eff_ShortSpin.prefab"; 
        protected override string LongSpinPrefabPath => "Assets/GameRes/Panel/Panel3993/Prefabs/Eff_LongSpin.prefab";

        private const string NgWinBorderPath = "Assets/GameRes/Panel/Panel3993/Prefabs/Eff_ng_winborder.prefab";
        private const string SgWinBorderPath = "Assets/GameRes/Panel/Panel3993/Prefabs/Eff_sg_winborder.prefab";

        private GameObject _goNgWinBorder, _goSgWinBorder;
        private GameObject _clonegoNgWinBorder, _clonegoSgWinBorder;
        private GComponent _anchorWinBorder;

        public GComponent AnchorWinBorder => _anchorWinBorder;

        protected override void InitParam()
        {
            base.InitParam();

            _anchorWinBorder = gOwnerPanel.GetChild("anchorWinBorder")?.asCom;
            if (_anchorWinBorder == null)
                return;

            int count = 2;
            Action callback = () =>
            {
                if (--count == 0)
                {
                    BindWinBorders();
                }
            };
            ResourceManager02.Instance.LoadAsset<GameObject>(NgWinBorderPath, prefab =>
            {
                _goNgWinBorder = prefab;
                callback();
            });
            ResourceManager02.Instance.LoadAsset<GameObject>(SgWinBorderPath, prefab =>
            {
                _goSgWinBorder = prefab;
                callback();
            });
        }

        /// <summary> 进入游戏只挂普通框，默认隐藏。 </summary>
        private void BindWinBorders()
        {
            if (_anchorWinBorder == null || _goNgWinBorder == null)
                return;

            BindWinBorder(_goNgWinBorder, ref _clonegoNgWinBorder);
            HideWinBorders();
        }

        /// <summary>
        /// 在 anchorWinBorder 上切换特效：卸掉当前框并销毁，再挂上指定实例。
        /// </summary>
        private void BindWinBorder(GameObject prefab, ref GameObject clone)
        {
            if (_anchorWinBorder == null || prefab == null)
                return;

            GameObject current = GameCommon.FguiUtils.GetWrapperTarget(_anchorWinBorder);
            if (current != null && current == clone)
                return;

            GameCommon.FguiUtils.DeleteWrapper(_anchorWinBorder);
            if (current == _clonegoNgWinBorder)
                _clonegoNgWinBorder = null;
            if (current == _clonegoSgWinBorder)
                _clonegoSgWinBorder = null;

            clone = GameObject.Instantiate(prefab);
            GameCommon.FguiUtils.AddWrapper(_anchorWinBorder, clone);
        }

        /// <summary> 普通游戏：挂普通框。 </summary>
        public void ShowNormalWinBorder()
        {
            BindWinBorder(_goNgWinBorder, ref _clonegoNgWinBorder);
            SetWinBorderVisible(_clonegoNgWinBorder, true);
            SetHolderVisible(true);
        }

        /// <summary> 大奖：删掉普通框，改挂大奖框。 </summary>
        public void ShowBigWinBorder()
        {
            BindWinBorder(_goSgWinBorder, ref _clonegoSgWinBorder);
            SetWinBorderVisible(_clonegoSgWinBorder, true);
            SetHolderVisible(true);
        }

        /// <summary> 只隐藏，不卸载当前挂着的框。 </summary>
        public void HideWinBorders()
        {
            GameObject current = GameCommon.FguiUtils.GetWrapperTarget(_anchorWinBorder);
            SetWinBorderVisible(current, false);
            SetHolderVisible(false);
        }

        private void SetWinBorderVisible(GameObject go, bool visible)
        {
            if (go == null)
                return;

            if (visible)
            {
                go.SetActive(true);
                ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>(true);
                for (int i = 0; i < particles.Length; i++)
                    particles[i].Play(true);
                return;
            }

            ParticleSystem[] list = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < list.Length; i++)
            {
                list[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                list[i].Clear(true);
            }
            go.SetActive(false);
        }

        private void SetHolderVisible(bool visible)
        {
            GGraph holder = _anchorWinBorder?.GetChild("holder")?.asGraph;
            if (holder != null)
                holder.visible = visible;
        }

        protected override void OnPropertyGameState(EventData res = null)
        {
            string gameState = (string)res?.value;

            if (gameState == GameState.Spin)
                win.text = 0.ToString();
            ClearSingleLineText();
        }
    }
}