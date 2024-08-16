using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UZSG.DialogSystem;

public class ChoicePanel : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject ChoiceButton;

    public Dialog dialogInstance;

    void Start()
    {

    }

    public void InstantiateChoices(List<Choice> choices)
    {
        // foreach(Choice c in choices)
        // {
        //     GameObject choiceButtonInstance = Instantiate(ChoiceButton);
        //     choiceButtonInstance.transform.SetParent(transform);
        //     Button choiceButtonInstanceButtonComponent = choiceButtonInstance.GetComponent<Button>();
        //     TextMeshProUGUI choiceButtonInstanceTextComponent = choiceButtonInstanceButtonComponent.GetComponentInChildren<TextMeshProUGUI>();
            
        //     choiceButtonInstanceButtonComponent.onClick.AddListener(delegate{dialogInstance.MakeChoice();});
        //     choiceButtonInstanceTextComponent.text = c.text;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
