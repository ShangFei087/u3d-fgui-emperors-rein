using FairyGUI;
using GameCommon;
using GameMaker;
using UnityEngine;

namespace MeiZhouHeiBao_3993
{
    public enum RewardElementState
    {
        None,
        Roll,
        Stop,
        BackRoll,
    }

    /// <summary>
    /// 大奖小游戏单图标。FairyGUI Y 向下，循环滚动 / 停止 / 回滚。
    /// 停稳可见格：FguiPool SymbolBonus + SmallGameNum 挂 number 骨骼（同主盘 anchorFrame 流程）。
    /// </summary>
    public class RewardElement3993
    {
        private const float NodeHeight = 300f;
        private const float MinSpeed = 2000f;
        private const float BackRollTime = 0.1f;
        private const string BonusIconUrl = "ui://MeiZhouHeiBao/ng_sym13_Bonus";
        private const string JpMiniIconUrl = "ui://MeiZhouHeiBao/ng_sym14_GoldCoin_MINI";
        private const string JpMajorIconUrl = "ui://MeiZhouHeiBao/ng_sym15_GoldCoin_MAJOR";
        private const string JpMinorIconUrl = "ui://MeiZhouHeiBao/ng_sym16_GoldCoin_MINOR";
        private const string BonusBonePath =
            "Anchor/Spine Mecanim GameObject (ng_sym14_Bonus)/SkeletonUtility-SkeletonRoot/root/All/coin/number";
        private const string CollectAnimName = "collect";
        private const string IdleAnimName = "idle";

        private static string RewardBonusPoolKey =>
            System.IO.Path.GetFileNameWithoutExtension(CustomModel.Instance.symbolRewardBonusEffect);

        private readonly GComponent _root;
        private readonly RewardRoll3993 _rewardRoll;
        private readonly GComponent _animator;
        private readonly GLoader _loader;
        private readonly GTextField _txtNum;
        private readonly GLoader _mask;
        private readonly float[] _nodePosList = new float[5];

        private int _id;
        private int _iconIndex;
        private int _rollIndex;
        private float _rollSpeed;
        private RewardElementState _state = RewardElementState.None;
        private GTweener _tweener;

        private GComponent _effectCom;
        private AnimPlayer _animPlayer;
        private GComponent _numCom;
        private GComponent _glowCom;
        private string _effectPoolKey;
        private int _jpType = -1;

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

        public void InitData()
        {
            if (_iconIndex == 0)
                return;
            SetRandomSprite();
        }

        public void StartRoll()
        {
            KillTween();
            ClearSpineEffect();
            SetMaskVisible(false);
            _state = RewardElementState.Roll;
            _rollSpeed = MinSpeed;
        }

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

        public void PlayCollect()
        {
            _animPlayer?.Play(CollectAnimName);
            PlayGlow();
        }

        public void PlayIdleFromStart()
        {
            _animPlayer?.Play(IdleAnimName, true);
        }

        public Vector2 GetCenterGlobal()
        {
            if (_root == null)
                return Vector2.zero;
            return _root.LocalToGlobal(new Vector2(_root.width * 0.5f, _root.height * 0.5f));
        }

        public void Dispose()
        {
            KillTween();
            RestoreSortingOrder();
            ClearSpineEffect();
            SetMaskVisible(false);
            _state = RewardElementState.None;
        }

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
                        txt.text = score.ToString();

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

            if (score > 0)
            {
                _txtNum.text = score.ToString();
                _txtNum.visible = true;
            }
            else
            {
                _txtNum.text = string.Empty;
                _txtNum.visible = false;
            }
        }

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

        private void RestoreSortingOrder()
        {
            if (_root == null)
                return;
            FguiSortingOrderManager.Instance.ReturnSortingOrder(_root);
        }

        private void SetMaskUrl(int score)
        {
            if (_mask == null) return;
            _mask.url = ContentModel.IsJackpotScore(score)
                ? GetJpIconUrl(ContentModel.GetJackpotType(score))
                : BonusIconUrl;
        }

        private void SetMaskVisible(bool visible)
        {
            if (_mask != null)
                _mask.visible = visible;
        }

        private void ClearGlow()
        {
            if (_glowCom == null)
                return;

            GameCommon.FguiUtils.DeleteWrapper(_glowCom);
            _glowCom.Dispose();
            _glowCom = null;
        }

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

        private void OnBackRollArrived()
        {
            _tweener = null;
            if (_state != RewardElementState.BackRoll)
                return;

            _root.y = _nodePosList[_id];
            _state = RewardElementState.None;
            _rewardRoll.BackRollEnd(_rollIndex);
        }

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

        private static string GetJpIconUrl(int jpType)
        {
            if (jpType == 1) return JpMinorIconUrl;
            if (jpType == 2) return JpMiniIconUrl;
            return JpMajorIconUrl;
        }

        private void KillTween()
        {
            if (_tweener == null)
                return;
            _tweener.Kill();
            _tweener = null;
        }
    }
}
