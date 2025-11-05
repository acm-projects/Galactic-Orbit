using UnityEngine;
using UnityEngine.UI;

public class BackgroundSpriteAnimator : MonoBehaviour
{
    [Header("Sprite Sheet (Resources Folder)")]
    public string spriteSheetName;   // just the name, no file extension

    [Header("Animation Settings")]
    public float frameRate = 12f;
    public float lastFrameHold = 10f;
    public bool loop = true;

    private Image image;
    private Sprite[] frames;
    private int currentFrame;
    private float timer;

    void Awake()
    {
        image = GetComponent<Image>();

        // Load all sliced sprites from the Resources folder
        frames = Resources.LoadAll<Sprite>(spriteSheetName);

        if (frames == null || frames.Length == 0)
        {
            Debug.LogError("No frames found for: " + spriteSheetName);
        }
        else
        {
            image.sprite = frames[0];
        }
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float waitTime = (currentFrame == frames.Length - 1) ? lastFrameHold : (1f / frameRate);

        if (timer >= waitTime)
        {
            if (currentFrame < frames.Length - 1)
            {
                currentFrame++;
            }
            else if (loop)
            {
                currentFrame = 0;
            }
            else
            {
                return; // stop on last frame if not looping
            }

            image.sprite = frames[currentFrame];
            timer = 0f;
        }
    }
}
