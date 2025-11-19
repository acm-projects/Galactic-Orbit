using System;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsScript : MonoBehaviour
{
    private VisualElement musicFill;
    private VisualElement sfxFill;

    private Slider musicSlider;
    private Slider sfxSlider;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        //var closeButton = root.Q<Button>("closeButton");
        //closeButton.clicked += CloseUI;

        musicSlider = root.Q<Slider>("musicSlider");
        if (musicSlider == null) musicSlider = root.Q<Slider>("SFXSlider");
        
        sfxSlider = root.Q<Slider>("SFXSlider");
        if (sfxSlider == null) sfxSlider = root.Q<Slider>("musicSlider");

        float savedMusicVolume = 1-AudioManager.Instance.musicSource.volume;//PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float savedSFXVolume = 1-AudioManager.Instance.sfxSource.volume;//PlayerPrefs.GetFloat("SFXVolume", 0.7f);

        if (musicSlider != null) musicSlider.value = savedMusicVolume * 100f;
        if (sfxSlider != null) sfxSlider.value = savedSFXVolume * 100f;

        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null && AudioManager.Instance.sfxSource != null)
        {
            //AudioManager.Instance.musicSource.volume = 0.1f;
            //AudioManager.Instance.musicSource.volume = 0.1f;
        }

        if (musicSlider != null) InjectFill(musicSlider, out musicFill);
        if (sfxSlider != null) InjectFill(sfxSlider, out sfxFill);

        if (musicSlider != null) musicSlider.schedule.Execute(() => UpdateFill(musicSlider, musicFill)).ExecuteLater(100);
        if (sfxSlider != null) sfxSlider.schedule.Execute(() => UpdateFill(sfxSlider, sfxFill)).ExecuteLater(100);

        if (musicSlider != null)
        {
            musicSlider.RegisterValueChangedCallback(evt => 
            {
                UpdateFill(musicSlider, musicFill);
                
                float volume = evt.newValue / 100f;
                volume = 1 - volume;
                
                if (AudioManager.Instance != null)
                {
                    Debug.Log(volume);
                    
                    AudioManager.Instance.SetMusicVolume(volume);
                    
                    if (AudioManager.Instance.musicSource != null)
                    {
                        AudioManager.Instance.musicSource.volume = volume;
                    }
                }
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.RegisterValueChangedCallback(evt => 
            {
                UpdateFill(sfxSlider, sfxFill);
                
                float volume = evt.newValue / 100f;
                volume = 1 - volume;
                if (AudioManager.Instance != null)
                {
                    Debug.Log(volume);
                    AudioManager.Instance.SetSFXVolume(volume);
                    if (AudioManager.Instance.sfxSource != null)
                    {
                        AudioManager.Instance.sfxSource.volume = volume;
                    }
                }
            });
        }

        SetupAppleStyleToggle(root, "eventToggle");
        SetupAppleStyleToggle(root, "questToggle");
    }

    private void SetupAppleStyleToggle(VisualElement root, string toggleName)
    {
        var toggle = root.Q<Toggle>(toggleName);
        
        if (toggle == null)
        {
            Debug.LogError($"Toggle '{toggleName}' not found!");
            return;
        }

        var defaultCheckmark = toggle.Q("unity-checkmark");
        if (defaultCheckmark != null)
            defaultCheckmark.RemoveFromHierarchy();

        var track = new VisualElement();
        track.name = "toggle-track";
        track.style.width = 51;
        track.style.height = 31;
        track.style.backgroundColor = new StyleColor(new Color(0.78f, 0.78f, 0.8f)); // Off state gray
        
        track.style.borderTopLeftRadius = 15;
        track.style.borderTopRightRadius = 15;
        track.style.borderBottomLeftRadius = 15;
        track.style.borderBottomRightRadius = 15;
        
        track.style.position = Position.Absolute;
        track.style.right = 0;
        track.style.top = 0;

        var thumb = new VisualElement();
        thumb.name = "toggle-thumb";
        thumb.style.width = 27;
        thumb.style.height = 27;
        thumb.style.backgroundColor = Color.white;
        
        thumb.style.borderTopLeftRadius = Length.Percent(50);
        thumb.style.borderTopRightRadius = Length.Percent(50);
        thumb.style.borderBottomLeftRadius = Length.Percent(50);
        thumb.style.borderBottomRightRadius = Length.Percent(50);
        
        thumb.style.position = Position.Absolute;
        thumb.style.left = 2; 
        thumb.style.top = 2;

        track.Add(thumb);
        toggle.Add(track);

        UpdateToggleVisual(toggle, track, thumb);

        toggle.RegisterValueChangedCallback(evt => UpdateToggleVisual(toggle, track, thumb));
    }

    private void UpdateToggleVisual(Toggle toggle, VisualElement track, VisualElement thumb)
    {
        if (toggle.value) 
        {
            track.style.backgroundColor = new StyleColor(new Color(0.447f, 0.427f, 0.659f)); // #726DA8
            thumb.style.left = 22; 
        }
        else 
        {
            track.style.backgroundColor = new StyleColor(new Color(0.78f, 0.78f, 0.8f)); // Gray
            thumb.style.left = 2; 
        }
    }

    private void InjectFill(Slider slider, out VisualElement fill)
    {
        var tracker = slider.Q("unity-tracker");

        fill = new VisualElement();
        fill.name = "custom-fill";
        
        Color customColor = new Color(0.447f, 0.427f, 0.659f);
        fill.style.backgroundColor = new StyleColor(customColor);
        
        fill.style.position = Position.Absolute;
        fill.style.right = 0; 
        fill.style.bottom = 0;
        fill.style.height = Length.Percent(100); 
        fill.style.width = 0; 

        fill.style.borderTopRightRadius = 4;
        fill.style.borderBottomRightRadius = 4;

        tracker.Insert(0, fill);
    }

    private void UpdateFill(Slider slider, VisualElement fill)
    {
        float percent = Mathf.InverseLerp(slider.lowValue, slider.highValue, slider.value);
        float reversedPercent = 1f - percent;
        
        var tracker = slider.Q("unity-tracker");
        float trackerWidth = tracker.resolvedStyle.width;
        
        if (trackerWidth > 0)
        {
            fill.style.width = trackerWidth * reversedPercent;
        }
        else
        {
            slider.schedule.Execute(() => UpdateFill(slider, fill)).ExecuteLater(1);
        }
    }

    private void CloseUI()
    {
        gameObject.SetActive(false);
    }
}