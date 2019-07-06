﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneControl : MonoBehaviour {
    private bool inTitle, inCredit;
    public GameObject titleMenu, creditObj, backObj;
    public string sceneName;
    void Start () {
        inTitle = true;
        inCredit = false;
    }

    // Update is called once per frame
    void Update () {
        quitCredit ();
    }

    private void quitCredit () {
        if (inCredit && Input.GetKeyDown (KeyCode.Escape)) {
            inCredit = false;
            inTitle = true;
            creditObj.SetActive (false);
            titleMenu.SetActive (true);
        }
    }

    public void clickStart () {
        SceneManager.LoadScene ("GameScene");
    }

    public void clickCredit () {
        inCredit = true;
        inTitle = false;
        creditObj.SetActive (true);
        titleMenu.SetActive (false);
        backObj.SetActive (true);
    }
    public void clickExit () {
        Application.Quit ();
    }
    public void clickBack () {
        inCredit = false;
        inTitle = true;
        backObj.SetActive (false);
        creditObj.SetActive (false);
        titleMenu.SetActive (true);
    }
}