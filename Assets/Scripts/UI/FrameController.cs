using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UZSG.Systems;

namespace UZSG.UI
{
    public class FrameController : MonoBehaviour
    {
        [SerializeField] bool isTransitioning;
        public bool IsTransitioning => isTransitioning;
        [SerializeField] Frame currentFrame;
        public Frame CurrentFrame => currentFrame;
        public float AnimationFactor = 0.5f;
        public LeanTweenType TweenType = LeanTweenType.easeOutExpo;
        public List<Frame> Frames = new();

        // [SerializeField] GameObject inactive;

#region Editor
        [SerializeField] string switchTo;
#endregion

        void Start()
        {
            // (inactive.transform as RectTransform).anchoredPosition = new(0, 1080f);
            currentFrame ??= GetComponentInChildren<Frame>();

            foreach (Frame frame in Frames)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(frame.rect);
            }
        }

        public void SwitchToFrame(string name)
        {
            Frame frame = GetFrame(name);
            if (frame == null) return;

            if (currentFrame != null)
            {
                if (currentFrame.Name == name) return; /// Same frame to switch to
                if (isTransitioning) return;
                isTransitioning = true;

                // if (Game.Main.Settings.EnableAnimations)
                // {
                    /// Move old frame out of the way
                    /// The value -1920 moves the frame one screen wide to the left
                    LeanTween.move(currentFrame.rect, new Vector2(-currentFrame.rect.rect.width, 0f), AnimationFactor)
                    .setEase(TweenType)
                    .setOnComplete(() =>
                    {
                        currentFrame.rect.anchoredPosition = new Vector2(currentFrame.rect.rect.width, 0f);
                    });

                    /// Move new frame into view
                    LeanTween.move(frame.rect, Vector2.zero, AnimationFactor)
                    .setEase(TweenType)
                    .setOnComplete(() =>
                    {
                        currentFrame = frame;
                        isTransitioning = false;
                    });
                    LayoutRebuilder.ForceRebuildLayoutImmediate(frame.transform as RectTransform);
                // }
                // else
                // {
                //     CurrentFrame.transform.SetParent(inactive.transform);
                //     CurrentFrame.rect.anchoredPosition = Vector2.zero;

                //     frame.transform.SetParent(transform);
                //     frame.gameObject.SetActive(true);
                //     frame.rect.anchoredPosition = Vector2.zero;
                //     LayoutRebuilder.ForceRebuildLayoutImmediate(frame.transform as RectTransform);
                    
                //     CurrentFrame = frame;
                // }
            }
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
            } else
            {
                frame = GetFrame(switchTo);
            }
            if (frame == null) return;
            if (currentFrame.Name == name) return;

            /// Hide current frame
            currentFrame.rect.anchoredPosition = new Vector2(currentFrame.rect.rect.width, 0f);

            /// Show new frame
            frame.rect.anchoredPosition = Vector2.zero;

            currentFrame = frame;
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        #endregion
    }
}

