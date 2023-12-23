using UnityEngine;

namespace UZSG.UI
{
    public class FrameController : MonoBehaviour
    {
        public float AnimationFactor = 1f;
        [field: SerializeField] public Frame CurrentFrame { get; set; }
        public Frame[] Frames = new Frame[3];
        private bool IsTransitioning = false;

        [SerializeField] GameObject inactive;

        void Start()
        {
            if (CurrentFrame == null)
            CurrentFrame = GetComponentInChildren<Frame>();
        }

        public void SwitchFrame(string name)
        {
            Frame frame = GetFrame(name);
            if (frame == null) return;

            if (CurrentFrame != null)
            {
                if (CurrentFrame.Name == name) return;
                if (IsTransitioning) return;
                IsTransitioning = true;

                LeanTween.move(CurrentFrame.Rect, new Vector3(-Screen.width, 0f, 0f), AnimationFactor)
                .setEaseOutExpo()
                .setOnComplete( () =>
                {
                    CurrentFrame.transform.SetParent(inactive.transform);
                    CurrentFrame.Rect.anchoredPosition = new Vector3(0f, 0f, 0f);
                    CurrentFrame.gameObject.SetActive(false);
                });

                frame.gameObject.SetActive(true);
                frame.gameObject.transform.SetParent(transform);
                LeanTween.move(frame.Rect, new Vector3(0f, 0f, 0f), AnimationFactor)
                .setEaseOutExpo()
                .setOnComplete( () =>
                {
                    CurrentFrame = frame;
                    IsTransitioning = false;
                });
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
    }
}

