using System.Collections.Generic;
using UnityEngine;

public class SpecialManager : MonoBehaviour
{
    public static SpecialManager Instance;

    // Drag all of your SpecialData assets here in the Inspector.
    public List<SpecialData> allSpecials;

    public SpecialData selectedSpecial { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
    }

    // Call this to remember which special was clicked
    public void SelectSpecial(SpecialData specialToSelect)
    {
        selectedSpecial = specialToSelect;
    }

    // Gets a list of unique, random specials.
    public List<SpecialData> GetUniqueRandomSpecials(int count)
    {
        List<SpecialData> availableSpecials = new List<SpecialData>(allSpecials);
        List<SpecialData> chosenSpecials = new List<SpecialData>();

        if (count >= availableSpecials.Count)
        {
            return availableSpecials;
        }

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, availableSpecials.Count);
            chosenSpecials.Add(availableSpecials[randomIndex]);
            availableSpecials.RemoveAt(randomIndex);
        }
        return chosenSpecials;
    }
}