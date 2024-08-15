using System;
using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Mono.Cecil;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UZSG.DialogSystem;
using UZSG.Systems;

namespace UZSG.DialogSystem
{
    public class DialogUI : MonoBehaviour
    {
        public Dialog dialogInstance = new();
        public TextMeshProUGUI UIText;
        public GameObject ChoicePanelObject;

        private GameObject choicePanelInstance;
        public GameObject ChoiceButton;
        private ChoicePanel _choicePanel;

        
        // Start is called before the first frame update
        void Start()
        {
            TextAsset asset = Resources.Load<TextAsset>("Dialogue/crime_story");
            dialogInstance.LoadStory(asset);
            dialogInstance.OnTypeEffectUpdate += OnTypeEffectUpdate;
            dialogInstance.OnChoiceEncounter += OnChoiceEncounter;
            // dialogInstance.story.onMakeChoice += OnMakeChoices;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return)){
                TypeEffectInvokeNextLine();
            }
        }

        public void TypeEffectInvokeNextLine()
        {
            if(dialogInstance.isTyping)
            {
                dialogInstance.StopTypeInvoke();
                return;
            }

            if (choicePanelInstance != null) return;
            
            dialogInstance.NextLine();
            StartCoroutine(dialogInstance.TypeEffect());

        }

        void OnTypeEffectUpdate(Char x)
        {
            UIText.text += x;
        }
        
        // public void OnMakeChoices(Choice choice)
        // {
        //     var _tmpChoicePanelInstance = choicePanelInstance;
        //     choicePanelInstance = null;
        //     Destroy(_tmpChoicePanelInstance.gameObject);
        //     TypeEffectInvokeNextLine();
        // }

    

        void OnChoiceEncounter(List<Choice> choices){
            choicePanelInstance = Instantiate(ChoicePanelObject, transform.parent);
            _choicePanel = choicePanelInstance.GetComponent<ChoicePanel>();

            foreach(Choice c in choices)
            {
                GameObject choiceButtonInstance = Instantiate(ChoiceButton);
                choiceButtonInstance.transform.SetParent(choicePanelInstance.transform);

                Button choiceButtonInstanceButtonComponent = choiceButtonInstance.GetComponent<Button>();
                TextMeshProUGUI choiceButtonInstanceTextComponent = choiceButtonInstanceButtonComponent.GetComponentInChildren<TextMeshProUGUI>();
                
                choiceButtonInstanceButtonComponent.onClick.AddListener(() =>
                {
                    dialogInstance.MakeChoice(c);
                    DestroyChoicePanel();
                    TypeEffectInvokeNextLine();
                });
                choiceButtonInstanceTextComponent.text = c.text;
            }
            
        }

        void DestroyChoicePanel()
        {
            var _tmpChoicePanelInstance = choicePanelInstance;
            choicePanelInstance = null;
            Destroy(_tmpChoicePanelInstance.gameObject);
        }
    }
}
