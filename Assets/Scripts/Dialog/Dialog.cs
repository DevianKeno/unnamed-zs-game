using System.Collections;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using UnityEngine;
using Ink.Runtime;
using System;

namespace UZSG.DialogSystem
{
    public class Dialog : MonoBehaviour
    {
        public Story story;

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

        private bool isLoaded(){
            return story != null;
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
                return "";
            }
            return story.Continue();
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

        public virtual void OnMakeChoices(Choice choice) {}
        public virtual void onDidContinue() {}
    }
}

