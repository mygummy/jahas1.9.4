using EasyTransition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
 

    [SerializeField]
    private GameObject PlayUI;

    public void OnClickPlayButton() {
        PlayUI.SetActive(true);

    }
    public void OnClickQuitButton()
    {
        PlayUI.SetActive(false);
    }

}
