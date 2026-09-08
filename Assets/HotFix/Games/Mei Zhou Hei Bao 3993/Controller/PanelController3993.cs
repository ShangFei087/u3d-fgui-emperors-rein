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
        /// <summary>常驻普通赢分框实例，只显隐不销毁。</summary>
        private GameObject _clonegoNgWinBorder;
        /// <summary>常驻大奖赢分框实例，只显隐不销毁。</summary>
        private GameObject _clonegoSgWinBorder;
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
                    EnsureWinBorderClones();
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

        /// <summary>进页时各 Instantiate 一份并默认挂普通框（隐藏）。</summary>
        private void EnsureWinBorderClones()
        {
            if (_anchorWinBorder == null)
                return;

            if (_clonegoNgWinBorder == null && _goNgWinBorder != null)
                _clonegoNgWinBorder = GameObject.Instantiate(_goNgWinBorder);
            if (_clonegoSgWinBorder == null && _goSgWinBorder != null)
                _clonegoSgWinBorder = GameObject.Instantiate(_goSgWinBorder);

            if (_clonegoNgWinBorder == null)
                return;

            // 初始只挂普通框；大奖框常驻在场景外，切换时用 ChangeWrapperTarget，不 Destroy
            GameObject current = GameCommon.FguiUtils.GetWrapperTarget(_anchorWinBorder);
            if (current == null)
                GameCommon.FguiUtils.AddWrapper(_anchorWinBorder, _clonegoNgWinBorder);
            else if (current != _clonegoNgWinBorder)
                GameCommon.FguiUtils.ChangeWrapperTarget(_anchorWinBorder, _clonegoNgWinBorder, true);

            HideWinBorders();
        }

        /// <summary>把指定常驻实例挂到 anchor（不销毁另一份）。</summary>
        private void SwitchWinBorder(GameObject clone)
        {
            if (_anchorWinBorder == null || clone == null)
                return;

            EnsureWinBorderClones();

            GameObject current = GameCommon.FguiUtils.GetWrapperTarget(_anchorWinBorder);
            if (current == clone)
                return;

            if (current == null)
                GameCommon.FguiUtils.AddWrapper(_anchorWinBorder, clone);
            else
                GameCommon.FguiUtils.ChangeWrapperTarget(_anchorWinBorder, clone, true);

            GameCommon.FguiUtils.RefreshWrapper(_anchorWinBorder);
        }

        /// <summary>普通游戏：显示常驻普通框。</summary>
        public void ShowNormalWinBorder()
        {
            SwitchWinBorder(_clonegoNgWinBorder);
            SetWinBorderVisible(_clonegoNgWinBorder, true);
            if (_clonegoSgWinBorder != null)
                SetWinBorderVisible(_clonegoSgWinBorder, false);
            SetHolderVisible(true);
        }

        /// <summary>大奖：显示常驻大奖框。</summary>
        public void ShowBigWinBorder()
        {
            SwitchWinBorder(_clonegoSgWinBorder);
            SetWinBorderVisible(_clonegoSgWinBorder, true);
            if (_clonegoNgWinBorder != null)
                SetWinBorderVisible(_clonegoNgWinBorder, false);
            SetHolderVisible(true);
        }

        /// <summary>只隐藏，不卸载/销毁常驻框。</summary>
        public void HideWinBorders()
        {
            SetWinBorderVisible(_clonegoNgWinBorder, false);
            SetWinBorderVisible(_clonegoSgWinBorder, false);
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
