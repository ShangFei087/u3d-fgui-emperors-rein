using FairyGUI;
using GameCommon;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    /// <summary>大奖盘单图标滚动状态。</summary>
    public enum RewardElementState
    {
        /// <summary>静止。</summary>
        None,
        /// <summary>循环滚动。</summary>
        Roll,
        /// <summary>准备停止。</summary>
        Stop,
        /// <summary>回滚。</summary>
        BackRoll,
    }

    /// <summary>
    /// 大奖小游戏单图标。FairyGUI Y 向下，循环滚动 / 停止 / 回滚。
    /// 停稳可见格：FguiPool SymbolBonus + SmallGameNum 挂 number 骨骼（同主盘 anchorFrame 流程）。
    /// </summary>
    public class RewardElement3993
    {
        /// <summary>单格高度，循环滚动换图阈值。</summary>
        private const float NodeHeight = 300f;
        /// <summary>滚动速度（像素/秒）。</summary>
        private const float MinSpeed = 2000f;
        /// <summary>回滚时长。</summary>
        private const float BackRollTime = 0.1f;
        /// <summary>普通 bonus 静态图。</summary>
        private const string BonusIconUrl = "ui://MeiZhouHeiBao/ng_sym13_Bonus";
        /// <summary>MINI 彩金静态图。</summary>
        private const string JpMiniIconUrl = "ui://MeiZhouHeiBao/ng_sym14_GoldCoin_MINI";
        /// <summary>MAJOR 彩金静态图。</summary>
        private const string JpMajorIconUrl = "ui://MeiZhouHeiBao/ng_sym15_GoldCoin_MAJOR";
        /// <summary>MINOR 彩金静态图。</summary>
        private const string JpMinorIconUrl = "ui://MeiZhouHeiBao/ng_sym16_GoldCoin_MINOR";
        /// <summary>Bonus Spine 上挂分数的骨骼路径。</summary>
        private const string BonusBonePath =
            "Anchor/Spine Mecanim GameObject (ng_sym14_Bonus)/SkeletonUtility-SkeletonRoot/root/All/coin/number";
        /// <summary>收集动画状态名。</summary>
        private const string CollectAnimName = "collect";
        /// <summary>锁定待机动画状态名。</summary>
        private const string IdleAnimName = "idle";

        /// <summary>Bonus Spine 对象池 key。</summary>
        private static string RewardBonusPoolKey =>
            System.IO.Path.GetFileNameWithoutExtension(CustomModel.Instance.symbolRewardBonusEffect);

        /// <summary>图标根节点。</summary>
        private readonly GComponent _root;
        /// <summary>所属 15 轴。</summary>
        private readonly RewardRoll3993 _rewardRoll;
        /// <summary>Spine/图挂点。</summary>
        private readonly GComponent _animator;
        /// <summary>滚动中的静态图。</summary>
        private readonly GLoader _loader;
        /// <summary>普通 bonus 分数文本。</summary>
        private readonly GTextField _txtNum;
        /// <summary>收集后遮罩图。</summary>
        private readonly GLoader _mask;
        /// <summary>循环滚动的 5 档 Y 坐标。</summary>
        private readonly float[] _nodePosList = new float[5];

        /// <summary>节点槽位 id（iconIndex+1），对应 _nodePosList。</summary>
        private int _id;
        /// <summary>轴内序号，0 为停稳可见格。</summary>
        private int _iconIndex;
        /// <summary>所属轴下标 0~14。</summary>
        private int _rollIndex;
        /// <summary>当前滚动速度。</summary>
        private float _rollSpeed;
        /// <summary>当前滚动状态。</summary>
        private RewardElementState _state = RewardElementState.None;
        /// <summary>回滚 Tweener。</summary>
        private GTweener _tweener;

        /// <summary>池化取出的 Spine 特效节点。</summary>
        private GComponent _effectCom;
        /// <summary>锁定格 Spine 播放器。</summary>
        private AnimPlayer _animPlayer;
        /// <summary>挂在 Bonus 骨骼上的分数组件。</summary>
        private GComponent _numCom;
        /// <summary>收集光效挂点。</summary>
        private GComponent _glowCom;
        /// <summary>当前 Spine 对象池 key。</summary>
        private string _effectPoolKey;
        /// <summary>彩金类型，-1 表示非彩金。</summary>
        private int _jpType = -1;

        /// <summary>缓存 FGUI 子节点：animator、image、txtNum、mask。</summary>
        public RewardElement3993(GComponent root, RewardRoll3993 rewardRoll)
        {
            _root = root;
            _rewardRoll = rewardRoll;
            _animator = root.GetChild("animator")?.asCom;
            _loader = root.GetChild("image") as GLoader;
            if (_loader == null && _animator != null)
                _loader = _animator.GetChild("image") as GLoader;

            _txtNum = root.GetChild("txtNum") as GTextField;
            if (_txtNum != null)
                _txtNum.visible = false;

            _mask = root.GetChild("mask") as GLoader;
            SetMaskVisible(false);
        }

        /// <summary>设置轴/槽位、循环 Y 坐标并清空显示。</summary>
        public void Init(int rollIndex, int iconIndex)
        {
            _rollIndex = rollIndex;
            _iconIndex = iconIndex;
            _id = iconIndex + 1;

            _nodePosList[0] = NodeHeight;
            _nodePosList[1] = 0f;
            _nodePosList[2] = -NodeHeight;
            _nodePosList[3] = -NodeHeight * 2f;
            _nodePosList[4] = -NodeHeight * 3f;

            KillTween();
            _root.y = _nodePosList[_id];
            _state = RewardElementState.None;
            SetBlank();
        }

        /// <summary>开转前给非可见格随机图标。</summary>
        public void InitData()
        {
            if (_iconIndex == 0)
                return;
            SetRandomSprite();
        }

        /// <summary>进入循环滚动，清 Spine 与遮罩。</summary>
        public void StartRoll()
        {
            KillTween();
            ClearSpineEffect();
            SetMaskVisible(false);
            _state = RewardElementState.Roll;
            _rollSpeed = MinSpeed;
        }

        /// <summary>开始回滚到目标 Y，并通知轴进入停轴流程。</summary>
        public void StartStopRoll()
        {
            KillTween();
            _state = RewardElementState.Stop;
            _rewardRoll.StartBackRoll(_rollIndex);

            float targetY = _nodePosList[_id];
            _root.y = targetY + 24f;
            _state = RewardElementState.BackRoll;
            _tweener = TweenUtils.DOLocalMoveY(_root, targetY, BackRollTime, EaseType.Linear, OnBackRollArrived);
        }

        /// <summary>滚动中下移，越过顶部则换图并绕回。</summary>
        public void Update(float dt)
        {
            if (_state != RewardElementState.Roll)
                return;

            float nextY = _root.y + _rollSpeed * dt;
            if (Mathf.Abs(_rollSpeed * dt) > NodeHeight)
                nextY = _root.y + NodeHeight;

            _root.y = nextY;
            if (_root.y >= _nodePosList[0])
            {
                _root.y = _nodePosList[4];
                SetRandomSprite();
            }
        }

        /// <summary>显示 bonus 或彩金；可见格用 Spine，滚动中用静态图。</summary>
        public void SetBonus(int score)
        {
            if (score <= 0)
            {
                SetBlank();
                return;
            }

            bool useSpine = _iconIndex == 0 && _state != RewardElementState.Roll;
            if (ContentModel.IsJackpotScore(score))
            {
                if (useSpine)
                    SetJackpotWithSpine(score);
                else
                    SetJackpotWithLoader(score);
                return;
            }

            if (useSpine)
                SetBonusWithSpine(score);
            else
                SetBonusWithLoader(score);
        }

        /// <summary>清空图标、分数与 Spine。</summary>
        public void SetBlank()
        {
            _jpType = -1;
            RestoreSortingOrder();
            ClearSpineEffect();
            SetMaskVisible(false);
            if (_loader != null)
            {
                _loader.visible = false;
                _loader.url = string.Empty;
            }

            if (_txtNum != null)
            {
                _txtNum.text = string.Empty;
                _txtNum.visible = false;
            }
        }

        /// <summary>收集完成后切静态图并显示遮罩。</summary>
        public void SetCollected(int score)
        {
            RestoreSortingOrder();
            ClearSpineEffect();
            if (ContentModel.IsJackpotScore(score))
                SetJackpotWithLoader(score);
            else
                SetBonusWithLoader(score);
            SetMaskUrl(score);
            SetMaskVisible(true);
        }

        /// <summary>播 collect 并闪收集光效。</summary>
        public void PlayCollect()
        {
            _animPlayer?.Play(CollectAnimName);
            PlayGlow();
        }

        /// <summary>锁定格循环播 idle。</summary>
        public void PlayIdleFromStart()
        {
            _animPlayer?.Play(IdleAnimName, true);
        }

        /// <summary>图标中心的全局坐标，供拖尾起点使用。</summary>
        public Vector2 GetCenterGlobal()
        {
            if (_root == null)
                return Vector2.zero;
            return _root.LocalToGlobal(new Vector2(_root.width * 0.5f, _root.height * 0.5f));
        }

        /// <summary>停 Tween、还层级、清 Spine。</summary>
        public void Dispose()
        {
            KillTween();
            RestoreSortingOrder();
            ClearSpineEffect();
            SetMaskVisible(false);
            _state = RewardElementState.None;
        }

        /// <summary>可见格：池化 Bonus Spine，分数挂 number 骨骼。</summary>
        private void SetBonusWithSpine(int score)
        {
            if (score <= 0)
            {
                SetBlank();
                return;
            }

            FguiPoolHelper pool = _rewardRoll.FguiPoolHelper;
            if (pool == null || _animator == null)
            {
                SetBonusWithLoader(score);
                return;
            }

            ClearSpineEffect();

            _effectPoolKey = RewardBonusPoolKey;
            string prefabPath = CustomModel.Instance.symbolRewardBonusEffect;
            _effectCom = pool.GetObject(TagPoolObject.SymbolAppear, prefabPath)?.asCom;
            if (_effectCom == null)
            {
                SetBonusWithLoader(score);
                return;
            }

            _animator.AddChild(_effectCom);
            _effectCom.SetXY(_animator.width * 0.5f, _animator.height * 0.5f);

            if (_loader != null)
            {
                _loader.visible = false;
                _loader.url = string.Empty;
            }

            if (_txtNum != null)
                _txtNum.visible = false;

            GameObject goRoot = GameCommon.FguiUtils.GetWrapperTarget(_effectCom);
            if (goRoot != null)
            {
                GameCommon.FguiUtils.RefreshWrapper(_effectCom);
                _animPlayer = new AnimPlayer(goRoot);
                PlayIdleFromStart();
                _numCom = UIPackage.CreateObject("MeiZhouHeiBao", "SmallGameNum")?.asCom;
                if (_numCom != null)
                {
                    GTextField txt = _numCom.GetChild("txtScore")?.asTextField;
                    if (txt != null)
                        txt.text = ContentModel.GetDisplayScore(score).ToString();

                    _effectCom.AddChild(_numCom);
                    _numCom.SetXY(0, 0);

                    if (!_animPlayer.Attach(
                            _numCom,
                            BonusBonePath,
                            localPos: Vector3.zero,
                            localScale: new Vector3(0.01f, 0.01f, 0.01f),
                            localRot: Quaternion.identity))
                    {
                        _numCom.Dispose();
                        _numCom = null;
                    }
                }
            }

            AttachGlow();

            if (_rewardRoll.GoExpectation != null)
                FguiSortingOrderManager.Instance.ChangeSortingOrder(_root, _rewardRoll.GoExpectation);
        }

        /// <summary>可见格：池化 Major/Minor/Mini Spine。</summary>
        private void SetJackpotWithSpine(int score)
        {
            int jpType = ContentModel.GetJackpotType(score);
            FguiPoolHelper pool = _rewardRoll.FguiPoolHelper;
            if (pool == null || _animator == null)
            {
                SetJackpotWithLoader(score);
                return;
            }

            ClearSpineEffect();
            _jpType = jpType;

            string prefabPath = CustomModel.Instance.GetJackpotPrefab(jpType);
            _effectPoolKey = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            _effectCom = pool.GetObject(TagPoolObject.SymbolAppear, prefabPath)?.asCom;
            if (_effectCom == null)
            {
                SetJackpotWithLoader(score);
                return;
            }

            _animator.AddChild(_effectCom);
            _effectCom.SetXY(_animator.width * 0.5f, _animator.height * 0.5f);

            if (_loader != null)
            {
                _loader.visible = false;
                _loader.url = string.Empty;
            }

            if (_txtNum != null)
                _txtNum.visible = false;

            GameObject goRoot = GameCommon.FguiUtils.GetWrapperTarget(_effectCom);
            if (goRoot != null)
            {
                GameCommon.FguiUtils.RefreshWrapper(_effectCom);
                _animPlayer = new AnimPlayer(goRoot);
                PlayIdleFromStart();
            }

            AttachGlow();

            if (_rewardRoll.GoExpectation != null)
                FguiSortingOrderManager.Instance.ChangeSortingOrder(_root, _rewardRoll.GoExpectation);
        }

        /// <summary>用静态图显示彩金图标。</summary>
        private void SetJackpotWithLoader(int score)
        {
            ClearSpineEffect();
            _jpType = ContentModel.GetJackpotType(score);

            if (_loader != null)
            {
                _loader.visible = true;
                _loader.url = GetJpIconUrl(_jpType);
            }

            if (_txtNum != null)
            {
                _txtNum.text = string.Empty;
                _txtNum.visible = false;
            }
        }

        /// <summary>用静态图显示 bonus 与分数。</summary>
        private void SetBonusWithLoader(int score)
        {
            ClearSpineEffect();

            if (_loader != null)
            {
                _loader.visible = true;
                _loader.url = BonusIconUrl;
            }

            if (_txtNum == null)
                return;

            int display = ContentModel.GetDisplayScore(score);
            if (display > 0)
            {
                _txtNum.text = display.ToString();
                _txtNum.visible = true;
            }
            else
            {
                _txtNum.text = string.Empty;
                _txtNum.visible = false;
            }
        }

        /// <summary>在图标上挂收集光效（默认隐藏）。</summary>
        private void AttachGlow()
        {
            ClearGlow();
            GameObject prefab = _rewardRoll.GlowPrefab;
            if (prefab == null || _animator == null)
                return;

            _glowCom = UIPackage.CreateObject("MeiZhouHeiBao", "anchorCom")?.asCom;
            if (_glowCom == null)
                return;

            _animator.AddChild(_glowCom);
            _glowCom.SetXY(_animator.width * 0.5f, _animator.height * 0.5f);
            GameCommon.FguiUtils.AddWrapper(_glowCom, UnityEngine.Object.Instantiate(prefab));
            _glowCom.visible = false;
        }

        /// <summary>显示并重播收集光效粒子。</summary>
        private void PlayGlow()
        {
            if (_glowCom == null)
                return;

            _glowCom.visible = false;
            _glowCom.visible = true;
            GameCommon.FguiUtils.RefreshWrapper(_glowCom);
            GameObject go = GameCommon.FguiUtils.GetWrapperTarget(_glowCom);
            if (go == null)
                return;

            ParticleSystem[] particles = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
                particles[i].Play(true);
        }

        /// <summary>恢复图标默认 sortingOrder。</summary>
        private void RestoreSortingOrder()
        {
            if (_root == null)
                return;
            FguiSortingOrderManager.Instance.ReturnSortingOrder(_root);
        }

        /// <summary>按分值设置收集后遮罩图。</summary>
        private void SetMaskUrl(int score)
        {
            if (_mask == null) return;
            _mask.url = ContentModel.IsJackpotScore(score)
                ? GetJpIconUrl(ContentModel.GetJackpotType(score))
                : BonusIconUrl;
        }

        /// <summary>显示或隐藏收集遮罩。</summary>
        private void SetMaskVisible(bool visible)
        {
            if (_mask != null)
                _mask.visible = visible;
        }

        /// <summary>卸掉收集光效。</summary>
        private void ClearGlow()
        {
            if (_glowCom == null)
                return;

            GameCommon.FguiUtils.DeleteWrapper(_glowCom);
            _glowCom.Dispose();
            _glowCom = null;
        }

        /// <summary>Spine 还池并清分数挂点。</summary>
        private void ClearSpineEffect()
        {
            ClearGlow();
            ClearNumBind();
            _jpType = -1;

            if (_effectCom == null)
            {
                _effectPoolKey = null;
                return;
            }

            FguiPoolHelper pool = _rewardRoll.FguiPoolHelper;
            if (pool != null && _animator != null)
            {
                string key = string.IsNullOrEmpty(_effectPoolKey) ? RewardBonusPoolKey : _effectPoolKey;
                pool.ReturnToPool(TagPoolObject.SymbolAppear, key, _animator);
            }

            _effectCom = null;
            _effectPoolKey = null;
        }

        /// <summary>卸骨骼挂点并销毁分数组件。</summary>
        private void ClearNumBind()
        {
            _animPlayer?.DetachAll();
            _animPlayer = null;

            if (_numCom != null)
            {
                _numCom.Dispose();
                _numCom = null;
            }
        }

        /// <summary>回滚，通知轴 BackRollEnd。</summary>
        private void OnBackRollArrived()
        {
            _tweener = null;
            if (_state != RewardElementState.BackRoll)
                return;

            _root.y = _nodePosList[_id];
            _state = RewardElementState.None;
            _rewardRoll.BackRollEnd(_rollIndex);
        }

        /// <summary>滚动循环换图：空白 / 随机 bonus / 假彩金。</summary>
        private void SetRandomSprite()
        {
            int ran = Random.Range(0, 100);
            if (ContentModel.Instance.isJackpotGame && ran <= 15)
            {
                int fake = ContentModel.JackpotScoreBase + Random.Range(0, 3);
                SetJackpotWithLoader(fake);
                return;
            }

            if (ran <= 40)
                SetBonusWithLoader(Random.Range(100, 1501));
            else
                SetBlank();
        }

        /// <summary>彩金类型对应静态图 URL。</summary>
        private static string GetJpIconUrl(int jpType)
        {
            if (jpType == 1) return JpMinorIconUrl;
            if (jpType == 2) return JpMiniIconUrl;
            return JpMajorIconUrl;
        }

        /// <summary>杀掉当前回滚 Tween。</summary>
        private void KillTween()
        {
            if (_tweener == null)
                return;
            _tweener.Kill();
            _tweener = null;
        }
    }
}
