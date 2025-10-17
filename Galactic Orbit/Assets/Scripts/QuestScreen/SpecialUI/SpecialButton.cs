using UnityEngine;
using TMPro;

public class SpecialButton : MonoBehaviour
{
    public TextMeshProUGUI specialTitleText;
    // public GameObject specialDetailsScreen; // Uncomment if you add a details screen

    private SpecialData mySpecial;

    // The SpecialUI script calls this to set the button's info
    public void Setup(SpecialData specialData)
    {
        mySpecial = specialData;
        specialTitleText.text = $"{mySpecial.specialTitle}\nCost: {mySpecial.cost} Coins";
    }

    public void OnButtonClick()
    {
        if (SpecialManager.Instance != null)
        {
            SpecialManager.Instance.SelectSpecial(mySpecial);
        }
        // if (specialDetailsScreen != null) { specialDetailsScreen.SetActive(true); }
    }
}