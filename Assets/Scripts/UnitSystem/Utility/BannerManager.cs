using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class BannerManager : MonoBehaviour
{
    GameObject GroupIdDisplay;
    GameObject HealthBar;
    
    
    void Awake()
    {
        GroupIdDisplay = transform.GetChild(0).gameObject;
        HealthBar = transform.GetChild(1).gameObject;
    }

    void OnMouseEnter()
    {
        Outline script = transform.parent.gameObject.GetComponent<Outline>();
        script.OutlineWidth = script.OutlineWidth + 1f;
    }

    void OnMouseExit()
    {
        Outline script = transform.parent.gameObject.GetComponent<Outline>();
        script.OutlineWidth = script.OutlineWidth - 1f;
    }

    public void SetGroupIdDisplay(char id)
    {
        if (id != '#')
        {
            Debug.Log(GroupIdDisplay.name);
            GroupIdDisplay.GetComponent<TextMeshPro>().text = id.ToString();
        }
        else
        {
            GroupIdDisplay.GetComponent<TextMeshPro>().text = "";
        }
    }
    public void SetHealthBar(float[] healthArray)
    {
        
        int i = 0;
        foreach(Transform trans in HealthBar.transform)
        {
            if (trans.gameObject.name.Contains("Cell"))
            {
                Renderer renderer = trans.gameObject.GetComponent<Renderer>();
                Material newMaterial = renderer.material;

                if (healthArray[i] > 100)
                {
                    newMaterial.color = Color.blue;
                }
                else if (healthArray[i] > 75)
                {
                    newMaterial.color = Color.green;
                }
                else if(healthArray[i] > 50)
                {
                    newMaterial.color = Color.yellow;
                }
                else if (healthArray[i] > 25)
                {
                    newMaterial.color = new Color(1.0f, 0.5f, 0.0f); ;
                }
                else if (healthArray[i] > 0)
                {
                    newMaterial.color = Color.red;
                }
                else if (healthArray[i] <= 0)
                {
                    newMaterial.color = Color.black;
                }
                i++;
            }
        }
    }


}
