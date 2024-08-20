using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    private Text text;
    
    void Start()
    {
        text = GetComponent<Text>();
        StartCoroutine(UpdateStats());
    }

    // Update is called once per frame
    IEnumerator UpdateStats()
    {
        while (true)
        {
            if (UnitManager.Instance && text && UnitManager.Instance.units != null)
            {
                text.text = "FPS: " + (1.0f / Time.deltaTime).ToString("0");
                text.text += "\n" + "Total Entities: " + UnitManager.Instance.units.Count.ToString();
            }
            
            yield return new WaitForSeconds(0.2f);

        }
    }
}
