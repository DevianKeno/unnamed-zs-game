using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    public class FrameController : MonoBehaviour
    {
        [SerializeField] bool _isTransitioning;
        public bool IsTransitioning => _isTransitioning;
        [SerializeField] Frame currentFrame;
        public Frame CurrentFrame => currentFrame;
        public float AnimationFactor = 0.5f;
        public LeanTweenType TweenType = LeanTweenType.easeOutExpo;
        public List<Frame> Frames = new();

        public event Action<string> OnSwitchFrame;

#region Editor
        [SerializeField] string switchTo;
#endregion

        void Start()
        {
            if (currentFrame == null && transform.childCount > 0)
            {
                currentFrame = transform.GetChild(0).GetComponent<Frame>();
            }
        }

        public void SwitchToFrame(string name)
        {
            SwitchToFrame(name, force: false);
        }

        public void SwitchToFrame(string name, bool force = false)
        {
            Frame frame = GetFrame(name);
            if (frame == null) return;

            if (_isTransitioning) return;
            _isTransitioning = true;

            if (Game.UI.EnableScreenAnimations)
            {
                if (currentFrame != null && currentFrame.Name != frame.Name)
                {
                    /// Move current frame out of the way
                    LeanTween.move(currentFrame.Rect, new Vector2(-currentFrame.Rect.rect.width, 0f), AnimationFactor)
                    .setEase(TweenType)
                    .setOnComplete(() =>
                    {
                        currentFrame.Rect.anchoredPosition = new Vector2(currentFrame.Rect.rect.width, 0f);
                    });
                }
                else if (!force)
                {
                    _isTransitioning = false;
                    return;
                }

                /// Move new frame into view
                LeanTween.move(frame.Rect, Vector2.zero, AnimationFactor)
                .setEase(TweenType)
                .setOnComplete(() =>
                {
                    currentFrame = frame;
                    currentFrame.transform.SetAsLastSibling();
                    _isTransitioning = false;
                });
                LayoutRebuilder.ForceRebuildLayoutImmediate(frame.transform as RectTransform);
            }
            else
            {
                currentFrame.Rect.anchoredPosition = new Vector2(currentFrame.Rect.rect.width, 0f); /// Hide current frame
                frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
                currentFrame = frame;
                currentFrame.transform.SetAsLastSibling();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
            
            OnSwitchFrame?.Invoke(currentFrame.Name);
        }

        public Frame GetFrame(string name)
        {
            foreach (Frame f in Frames)
            {
                if (f.Name != name) continue;
                return f;
            }
            return null;
        }


        #region Editor

        public void SwitchFrameEditor(string frameId)
        {
            switchTo = frameId;
            SwitchFrameEditor();
        }

        public void SwitchFrameEditor()
        {
            Frame frame = null;
            if (int.TryParse(switchTo, out int index))
            {
                if (index >= 0 && index < Frames.Count)
                {
                    frame = Frames[index];
                }
            }
            else
            {
                frame = GetFrame(switchTo);
            }
            if (frame == null) return;
            
            currentFrame.Rect.anchoredPosition = new Vector2(currentFrame.Rect.rect.width, 0f); /// Hide current frame
            frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
            currentFrame = frame;
            currentFrame.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        #endregion
    }
}

