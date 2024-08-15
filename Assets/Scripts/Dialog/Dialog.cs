using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using Ink.Runtime;
using System;
using System.Linq;

namespace UZSG.DialogSystem
{
    public class Dialog
    {
        public Story story;
        public event Action<Char> OnTypeEffectUpdate;
        
        public event Action<List<Choice>> OnChoiceEncounter;
        public float TypeEffectSpeed = 0.025f;

        public bool isTypingStopInvoke = false;
        public bool isTyping = false;

        public void LoadStory(TextAsset asset)
        {
            story = new Story(asset.text);

            story.onError += (msg, type) => {
                if( type == Ink.ErrorType.Warning ) Debug.LogWarning(msg);
                else Debug.LogError(msg);
            };

            story.onMakeChoice += OnMakeChoices;
            story.onDidContinue += onDidContinue;
        }

        private bool isLoaded()
        {
            return story != null;
        }


        private void CheckpointCheck()
        {
            if (story.currentChoices.Count > 0)
            {
                OnChoiceEncounter?.Invoke(story.currentChoices);
                return;
            }
            Debug.LogWarning("You have reached the end of the stoyr");
        }

        public string NextLine()
        {
            if(!isLoaded())
            {
                Debug.LogError("Load your story first before proceeding with next line");
                return "";
            } 
            if (!story.canContinue)
            {
                Debug.LogWarning("You either reached a choice or end of story");
                CheckpointCheck();
                return "";
            } 
            return story.Continue();
        }

        public void StopTypeInvoke(){
            isTypingStopInvoke = true;
        }

        public string CurrentLine()
        {
            if(!isLoaded())
            {
                Debug.LogError("Load your story first before proceeding with next line");
                return "";
            } 
            return story.currentText;
        }


        public List<Choice> GetChoices(){
            if(story.canContinue)
            {
                Debug.LogError("You can't retrieve choices while the story is in progress");
            }

            return story.currentChoices;
        }

        public void ResetStory()
        {
            if(!isLoaded())
            {
                Debug.LogError("You didn't even load the story in the first place");
                return;
            }
            story.ResetState();
        }

        public void MakeChoice(int choiceIndex)
        {
            if(story.canContinue)
            {
                Debug.LogError("You can't choose while the story is in progress");
                return;
            }

            story.ChooseChoiceIndex(choiceIndex);
        }

        public IEnumerator TypeEffect() 
        {
            isTyping = true;
            foreach(Char x in story.currentText)
            {
                if (isTypingStopInvoke)
                {
                    OnTypeEffectUpdate?.Invoke(x);
                    continue;
                }

                OnTypeEffectUpdate?.Invoke(x);
                if (isTypingStopInvoke) break;
                yield return new WaitForSeconds(TypeEffectSpeed);

            }

            isTypingStopInvoke = false;
            OnTypeEffectUpdate?.Invoke('\n');
            isTyping = false;
            CheckpointCheck();
        }

        public virtual void OnMakeChoices(Choice choice) {}
        public virtual void onDidContinue() {}
    }
}

