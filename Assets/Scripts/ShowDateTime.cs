using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ShowDateTime : MonoBehaviour
{
    public TextMeshProUGUI time;
    public TextMeshProUGUI date;
    //public DateTimeManager dtManager;

    private void Update()
    {
        time.text = DateTime.Now.ToLongTimeString();
        date.text = DateTime.Now.ToLongDateString();
    }
}
