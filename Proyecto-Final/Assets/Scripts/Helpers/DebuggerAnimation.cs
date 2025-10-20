// This tool is intended for in-editor use only. Attach to GameObjects in the Editor to preview and trigger animations.

using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Helpers
{
    public class DebuggerAnimation : MonoBehaviour
    {
        [HideInInspector] public List<AnimationClip> AnimationClips = new();

        [TableList(ShowIndexLabels = true)] [ShowInInspector] private List<AnimClipInfo> animationList = new();

        [ReadOnly, Tooltip("Animator to fetch AnimationClips from. If left empty, will auto-get from this GameObject.")]
        private Animator animator;
        private PlayableGraph playableGraph;
        private AnimationClipPlayable currentPlayable;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animator == null)
                animator = gameObject.AddComponent<Animator>();
            FetchClipsFromAnimator();
            PopulateAnimationList();
        }

        private void OnDestroy()
        {
            if (playableGraph.IsValid())
                playableGraph.Destroy();
        }

        private void OnValidate()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            FetchClipsFromAnimator();
            PopulateAnimationList();
        }

        private void FetchClipsFromAnimator()
        {
            AnimationClips.Clear();
            if (animator == null || animator.runtimeAnimatorController == null) return;
            var clips = animator.runtimeAnimatorController.animationClips;
            var added = new HashSet<string>();
            foreach (var clip in clips)
            {
                if (clip == null || added.Contains(clip.name)) continue;
                AnimationClips.Add(clip);
                added.Add(clip.name);
            }
        }

        [System.Serializable]
        public class AnimClipInfo
        {
            [ReadOnly]
            [TableColumnWidth(200)]
            public string ClipName;
            [ReadOnly]
            [TableColumnWidth(100)]
            public bool IsLooping;
            [HideInInspector]
            public AnimationClip Clip;

            private readonly DebuggerAnimation parent;

            [Button("Is Looping?", ButtonSizes.Large)]
            [TableColumnWidth(120)]
            private void ShowLooping()
            {
                var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(Clip);
                Debug.Log($"'{ClipName}' looping: {settings.loopTime}");
            }

            [Button("Play", ButtonSizes.Large)]
            [TableColumnWidth(100)]
            private void Play()
            {
                if (parent != null)
                    parent.PlayClip(Clip);
                else
                    Debug.LogWarning("No parent DebuggerAnimation assigned.");
            }

            public AnimClipInfo(AnimationClip clip, DebuggerAnimation parent = null)
            {
                Clip = clip;
                ClipName = clip.name;
                var settings = UnityEditor.AnimationUtility.GetAnimationClipSettings(clip);
                IsLooping = settings.loopTime;
                this.parent = parent;
            }
        }

        private void PlayClip(AnimationClip clip)
        {
            if (clip == null) return;
            if (playableGraph.IsValid())
                playableGraph.Destroy();
            playableGraph = PlayableGraph.Create("DebuggerPlayerGraph");
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
            currentPlayable = AnimationClipPlayable.Create(playableGraph, clip);
            playableOutput.SetSourcePlayable(currentPlayable);
            playableGraph.Play();
        }

        private void PopulateAnimationList()
        {
            animationList.Clear();
            HashSet<string> added = new HashSet<string>();
            foreach (var clip in AnimationClips)
            {
                if (clip != null && !added.Contains(clip.name))
                {
                    animationList.Add(new AnimClipInfo(clip, this));
                    added.Add(clip.name);
                }
            }
        }

        // Called by Animation Event on 'Revive' animation
        public void OnReviveAnimationEnd()
        {
            Debug.Log("OnReviveAnimationEnd event received.");
            // TODO: Add your logic here
        }

        // Called by Animation Event on 'Death' animation
        public void OnDeathAnimationEnd()
        {
            Debug.Log("OnDeathAnimationEnd event received.");
            // TODO: Add your logic here
        }

        // Called by Animation Event on 'Walk_Down' animation
        public void PlayFootstep()
        {
            Debug.Log("PlayFootstep event received.");
            // TODO: Add your logic here
        }

        // Called by Animation Event on 'HandAttack' animation
        public void CallShoot()
        {
            Debug.Log("CallShoot event received.");
            // TODO: Add your logic here
        }

        // Called by Animation Event on 'HandAttack' animation
        public void OnAttackAnimationEnd()
        {
            Debug.Log("OnAttackAnimationEnd event received.");
            // TODO: Add your logic here
        }
    }
}