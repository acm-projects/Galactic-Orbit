using System.Collections.Generic;
using UnityEngine;

public class SpecialUI : MonoBehaviour
{
    public SpecialManager specialManager;
    public List<SpecialButton> specialButtons;

    void OnEnable()
    {
        PullNewSpecials();
    }

    public void PullNewSpecials()
    {
        List<SpecialData> uniqueSpecials = specialManager.GetUniqueRandomSpecials(specialButtons.Count);

        for (int i = 0; i < specialButtons.Count; i++)
        {
            if (i < uniqueSpecials.Count)
            {
                specialButtons[i].gameObject.SetActive(true);
                specialButtons[i].Setup(uniqueSpecials[i]);
            }
            else
            {
                specialButtons[i].gameObject.SetActive(false);
            }
        }
    }
}