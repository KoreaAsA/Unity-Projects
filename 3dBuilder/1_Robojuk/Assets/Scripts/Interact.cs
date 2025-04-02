using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interact : MonoBehaviour
{
    private Outline outline;
    //private int activationCounter = 2;
    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = true;
    }
}

