using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialTrigger : MonoBehaviour
{
    [Header("---------- Info Trigger ----------")]
    public Text titleTxt;
    public string title;
    public Text infoTxt;
    [TextArea(5, 10), Multiline]
    public string info;
    public GameObject panel;

    [Header("---------- Events ----------")]
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            panel.SetActive(true);
            if (titleTxt) titleTxt.text = title;
            if (infoTxt) infoTxt.text = info;

            onPlayerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            panel.SetActive(false);
            if (infoTxt) titleTxt.text = "";
            if (infoTxt) infoTxt.text = "";

            onPlayerExit.Invoke();
        }
    }
}
