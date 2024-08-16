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

        private GameObject _choicePanelInstance;
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
            
            if (_choicePanelInstance != null) return;

            if (dialogInstance.isStoryEnd())
            {
                return;
            };
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

        int selectedChoice;

        void OnChoiceEncounter(List<Choice> choices){
            _choicePanelInstance = Instantiate(ChoicePanelObject, transform.parent);
            _choicePanel = _choicePanelInstance.GetComponent<ChoicePanel>();

            foreach(Choice c in choices)
            {
                GameObject choiceButtonInstance = Instantiate(ChoiceButton);
                choiceButtonInstance.transform.SetParent(_choicePanelInstance.transform);

                Button choiceButtonInstanceButtonComponent = choiceButtonInstance.GetComponent<Button>();
                TextMeshProUGUI choiceButtonInstanceTextComponent = choiceButtonInstanceButtonComponent.GetComponentInChildren<TextMeshProUGUI>();
                ChoiceContainer choiceContainer = choiceButtonInstance.GetComponent<ChoiceContainer>();
                choiceContainer.ChoiceIndex = c.index;
               
                choiceButtonInstanceButtonComponent.onClick.AddListener(() =>
                {
                    dialogInstance.MakeChoice(choiceContainer.ChoiceIndex);
                    DestroyChoicePanel();
                    TypeEffectInvokeNextLine();
                });
                choiceButtonInstanceTextComponent.text = c.text;
            }
            
        }



        void DestroyChoicePanel()
        {
            var _tmpChoicePanelInstance = _choicePanelInstance;
            _choicePanelInstance = null;
            Destroy(_tmpChoicePanelInstance.gameObject);
        }
    }
}
