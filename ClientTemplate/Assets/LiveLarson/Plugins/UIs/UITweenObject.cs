using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine.EventSystems;

namespace LiveLarson.Plugins.UIs
{
    public class UITweenObject : UIBehaviour
    {
        public List<DOTweenAnimation> animations;
        public bool playOnEnable = true;

        public bool collectInChildRecurcive;
        public bool includeInactive;
        public string testID;

        public bool toggleAutoPlay;
        public bool ignoreTimeScale;
        private bool _isFirstInit;
        private readonly OnDisableBehaviour _disableSetting = OnDisableBehaviour.None;

        private readonly OnEnableBehaviour _enableSetting = OnEnableBehaviour.Restart;
        private readonly bool playOnEnableLeavedCildren = false;

        protected override void Awake()
        {
            base.Awake();
            if (animations == null || animations.Count == 0)
                Collect();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (playOnEnable)
            {
                if (!_isFirstInit)
                {
                    toggleAutoPlay = false;
                    SwitchTweensAutoPlay(toggleAutoPlay);
                    _isFirstInit = true;
                }

                EnableAction();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            DisableAction();
        }

        public void Test()
        {
            Play(testID);
        }

        private void EnableAction()
        {
            if (playOnEnableLeavedCildren)
            {
                EnableTweenAction();
                return;
            }

            switch (_enableSetting)
            {
                case OnEnableBehaviour.None:
                    break;
                case OnEnableBehaviour.Play:
                    Play();
                    break;
                case OnEnableBehaviour.Restart:
                    Restart();
                    break;
                case OnEnableBehaviour.RestartFromSpawnPoint:
                    RestartFromSpawnPoint();
                    break;
            }
        }

        private void DisableAction()
        {
            if (playOnEnableLeavedCildren)
            {
                DisableTweenAction();
                return;
            }

            switch (_disableSetting)
            {
                case OnDisableBehaviour.None:
                    break;
                case OnDisableBehaviour.Pause:
                    Pause();
                    break;
                case OnDisableBehaviour.Rewind:
                    Rewind();
                    break;
                case OnDisableBehaviour.Kill:
                    DoKill();
                    break;
                case OnDisableBehaviour.KillAndComplete:
                    DoComplete();
                    DoKill();
                    break;
                case OnDisableBehaviour.DestroyGameObject:
                    DestroyObject();
                    break;
            }
        }

        public void Collect()
        {
            if (collectInChildRecurcive)
                animations = GetComponentsInChildren<DOTweenAnimation>(includeInactive)?.ToList();
            else
                animations = GetComponents<DOTweenAnimation>()?.ToList();
        }

        public void Play()
        {
            if (animations != null)
                foreach (var tween in animations)
                    tween.DOPlay();
        }

        public void EnableTweenAction()
        {
            animations.ForEach(p => p.enabled = true);
        }

        public void DisableTweenAction()
        {
            animations.ForEach(p => p.enabled = false);
        }

        public void Restart()
        {
            animations.ForEach(p => p.DORestart());
        }

        public void RestartFromSpawnPoint()
        {
            animations.ForEach(p => p.DORestart(true));
        }

        public void Rewind()
        {
            animations.ForEach(p => p.DORewind());
        }

        public void Pause()
        {
            animations.ForEach(p => p.tween?.Pause());
        }

        public void DoKill()
        {
            animations.ForEach(p => p.DOKill());
        }

        public void DoComplete()
        {
            animations.ForEach(p => p.DOComplete());
        }

        public void DestroyObject()
        {
            for (var i = animations.Count - 1; i >= 0; i--)
                if (animations[i].tween != null)
                    if (animations[i].tween.IsComplete())
                    {
                        Destroy(animations[i].gameObject);
                        animations.RemoveAt(i);
                    }
        }

        public void SwitchTweensAutoPlay(bool enable = true)
        {
            foreach (var tween in animations) tween.autoPlay = enable;
        }

        public void SwitchTweensIgnoreTimeScale(bool enable = true)
        {
            foreach (var tween in animations) tween.isIndependentUpdate = enable;
        }

        public void ToggleAutoPlay()
        {
            toggleAutoPlay = !toggleAutoPlay;
            SwitchTweensAutoPlay(toggleAutoPlay);
        }

        public void ToggleIgnoreTimeScale()
        {
            ignoreTimeScale = !ignoreTimeScale;
            SwitchTweensIgnoreTimeScale(ignoreTimeScale);
        }


        public void Play(string id)
        {
            // animations?.Where(p => p.id == id).ForEach(p => p.tween.Restart());

            if (animations != null)
                foreach (var item in animations)
                    if (item.id == id)
                        item.DORestartById(id);
        }

        public IEnumerable<DOTweenAnimation> GetTweenToPreview()
        {
            return animations?.Where(p => p.id == testID);
        }
    }
}