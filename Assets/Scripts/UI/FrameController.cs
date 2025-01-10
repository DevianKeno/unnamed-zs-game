using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UZSG.Systems;

namespace UZSG.UI
{
    public class FrameController : MonoBehaviour
    {
        public enum SwitchStatus {
            Started, Finished
        }

        public struct SwitchFrameContext
        {
            public SwitchStatus Status { get; set; }
            /// <summary>
            /// The frame to switch to.
            /// </summary>
            public Frame Frame { get; set; }
            public string Previous { get; set; }
            public string Next { get; set; }
        }

        public Vector2 FrameSize;
        public Vector2 InactiveFramePosition = new(Screen.width, 0f); /// one screen right
        public float AnimationFactor = 0.5f;
        public LeanTweenType Ease = LeanTweenType.easeOutExpo;
        public bool IsTransitioning { get; private set; }
        [SerializeField] Frame currentFrame;
        public Frame CurrentFrame => currentFrame;

        string _previousFrame;
        [SerializeField] List<Frame> frames = new();

        /// <summary>
        /// Called whenever frames are switched.
        /// </summary>
        public event Action<SwitchFrameContext> OnSwitchFrame;


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

        public void SwitchToFrame(int index)
        {
            if (!frames.IsValidIndex(index)) return;

            SwitchToFrame(frames[index].Name, instant: false, force: false);
        }

        public void SwitchToFrame(string name)
        {
            SwitchToFrame(name, instant: false, force: false);
        }

        public void SwitchToFrame(string name, bool instant = false)
        {
            SwitchToFrame(name, instant, force: false);
        }

        public void SwitchToFrame(string name, bool instant = false, bool force = false)
        {
            Frame frame = GetFrame(name);
            if (frame == null) return;

            if (IsTransitioning) return;
            IsTransitioning = true;

            _previousFrame = currentFrame.Name;
            var context = new SwitchFrameContext()
            {
                Status = SwitchStatus.Started,
                Frame = frame,
                Previous = _previousFrame,
                Next = name,
            };
            OnSwitchFrame?.Invoke(context);

            if (Game.UI.EnableScreenAnimations && !instant)
            {
                if (currentFrame != null && currentFrame.Name != frame.Name)
                {
                    /// Move current frame out of the way
                    LeanTween.move(currentFrame.Rect, new Vector2(-1920, 0f), AnimationFactor)
                    .setEase(Ease)
                    .setOnComplete(() =>
                    {
                        currentFrame.Rect.anchoredPosition = InactiveFramePosition;
                    });
                }
                else if (!force)
                {
                    IsTransitioning = false;
                    return;
                }

                /// Move new frame into view
                LeanTween.move(frame.Rect, Vector2.zero, AnimationFactor)
                .setEase(Ease)
                .setOnComplete((() =>
                {
                    currentFrame = frame;
                    currentFrame.transform.SetAsLastSibling();
                    IsTransitioning = false;

                    context.Status = SwitchStatus.Finished;
                    OnSwitchFrame?.Invoke(context);
                }));
                LayoutRebuilder.ForceRebuildLayoutImmediate(frame.transform as RectTransform);
            }
            else
            {
                IsTransitioning = true;
                currentFrame.Rect.anchoredPosition = new Vector2(-1920, 0f); /// Hide current frame
                frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
                currentFrame = frame;
                currentFrame.transform.SetAsLastSibling();
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
                IsTransitioning = false;
                context.Status = SwitchStatus.Finished;
                OnSwitchFrame?.Invoke(context);
            }
        }

        public void AppendFrame(Frame frame, bool display = true)
        {
            frame.transform.SetParent(transform);
            frame.Rect.sizeDelta = Vector2.zero;
            frames.Add(frame);
            if (display)
            {
                SwitchToFrame(frame.Name, instant: true);
            }
            else
            {
                frame.Rect.anchoredPosition = InactiveFramePosition;
            }
        }

        public void RemoveFrame(Frame frame)
        {
            if (frames.Contains(frame))
            {
                frames.Remove(frame);
            }
            SwitchToFrame(_previousFrame, instant: true);
        }

        public Frame GetFrame(string name)
        {
            foreach (Frame f in frames)
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
                if (index >= 0 && index < frames.Count)
                {
                    frame = frames[index];
                }
            }
            else
            {
                frame = GetFrame(switchTo);
            }
            if (frame == null) return;
            
            currentFrame.Rect.anchoredPosition = InactiveFramePosition; /// Hide current frame
            frame.Rect.anchoredPosition = Vector2.zero; /// Show new frame
            frame.transform.SetAsLastSibling();
            currentFrame = frame;
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        #endregion
    }
}

