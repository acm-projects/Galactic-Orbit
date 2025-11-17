using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class ShopController : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log("ShopController: OnEnable called.");

        var root = GetComponentInChildren<UIDocument>().rootVisualElement;
        if (root == null)
        {
            Debug.LogError("ShopController: No UIDocument found!");
            return;
        }

        Debug.Log("ShopController: UIDocument found, updating UI...");
        UpdateUI(root);

        // Buttons
        var eyesBtn = root.Q<Button>("EyesButton");
        var mouthBtn = root.Q<Button>("MouthButton");
        var decorBtn = root.Q<Button>("DecorButton");

        Debug.Log("ShopController: Button references - Eyes: " + (eyesBtn != null) +
                  " Mouth: " + (mouthBtn != null) +
                  " Decor: " + (decorBtn != null));

        eyesBtn?.RegisterCallback<ClickEvent>(evt => OnEyesButton());
        mouthBtn?.RegisterCallback<ClickEvent>(evt => OnMouthButton());
        decorBtn?.RegisterCallback<ClickEvent>(evt => OnDecorButton());
    }

    // === MAIN UI UPDATE WRAPPER ===
    private void UpdateUI(VisualElement root)
    {
        Debug.Log("ShopController: UpdateUI called.");
        StartCoroutine(UpdateUIRoutine(root));
    }

    // === LOAD FIREBASE DATA + UPDATE UI ===
    private IEnumerator UpdateUIRoutine(VisualElement root)
    {
        Debug.Log("ShopController: Starting UpdateUIRoutine...");

        bool eyesLoaded = false;
        bool mouthLoaded = false;
        bool decorLoaded = false;
        bool coinsLoaded = false;

        int coinAmount = 0;
        bool hasEyes = false;
        bool hasMouth = false;
        bool hasDecor = false;

        // Load items
        Debug.Log("ShopController: Requesting Firebase item checks...");

        UserProfileManager.Instance.HasItem("Eyes5", (result) =>
        {
            Debug.Log("Firebase: Eyes5 result = " + result);
            hasEyes = result;
            eyesLoaded = true;
        });

        UserProfileManager.Instance.HasItem("Mouth5", (result) =>
        {
            Debug.Log("Firebase: Mouth5 result = " + result);
            hasMouth = result;
            mouthLoaded = true;
        });

        UserProfileManager.Instance.HasItem("Decor5", (result) =>
        {
            Debug.Log("Firebase: Decor5 result = " + result);
            hasDecor = result;
            decorLoaded = true;
        });

        // Load coins
        Debug.Log("ShopController: Requesting coin count...");

        UserProfileManager.Instance.GetCoins((coins) =>
        {
            Debug.Log("Firebase: Coins = " + coins);
            coinAmount = coins;
            coinsLoaded = true;
        });

        // Wait for ALL callbacks
        Debug.Log("ShopController: Waiting for Firebase results...");

        while (!eyesLoaded || !mouthLoaded || !decorLoaded || !coinsLoaded)
        {
            yield return null;
        }

        Debug.Log("ShopController: Firebase data loaded. Applying UI updates...");

        // ------------------------------
        // APPLY RESULTS TO UI
        // ------------------------------

        var eyesItem = root.Q<VisualElement>("EyesItemContainer");
        Debug.Log("UI Element EyesItemContainer found: " + (eyesItem != null));
        if (eyesItem != null)
            eyesItem.style.opacity = hasEyes ? 0.2f : 1f;

        var mouthItem = root.Q<VisualElement>("MouthItemContainer");
        Debug.Log("UI Element MouthItemContainer found: " + (mouthItem != null));
        if (mouthItem != null)
            mouthItem.style.opacity = hasMouth ? 0.2f : 1f;

        var decorItem = root.Q<VisualElement>("DecorItemContainer");
        Debug.Log("UI Element DecorItemContainer found: " + (decorItem != null));
        if (decorItem != null)
            decorItem.style.opacity = hasDecor ? 0.2f : 1f;

        var coinLabel = root.Q<Label>("coinCount");
        Debug.Log("UI Element coinCount found: " + (coinLabel != null));
        if (coinLabel != null)
            coinLabel.text = coinAmount.ToString();

        Debug.Log($"UI Updated. Coins={coinAmount}, Eyes={hasEyes}, Mouth={hasMouth}, Decor={hasDecor}");
    }

    // === BUY FLOW ===
    private void AttemptToBuy(string itemId, int price)
    {
        Debug.Log($"ShopController: AttemptToBuy called for {itemId} price={price}");

        UserProfileManager.Instance.BuyItem(itemId, price, (success, msg) =>
        {
            Debug.Log($"BuyItem Callback → success={success}, msg={msg}");

            if (!success)
            {
                Debug.LogWarning("Purchase failed: " + msg);
                return;
            }
            AudioManager.Instance.PlaySFX(AudioManager.Instance.PurchaseSound);
            Debug.Log("Purchase successful! Refreshing UI...");
            UpdateUI(GetComponentInChildren<UIDocument>().rootVisualElement);
        });
    }

    // === BUTTON CALLBACKS ===
    private void OnEyesButton()
    {
        Debug.Log("Eyes button clicked.");
        AttemptToBuy("Eyes5", 35);
    }

    private void OnMouthButton()
    {
        Debug.Log("Mouth button clicked.");
        AttemptToBuy("Mouth5", 30);
    }

    private void OnDecorButton()
    {
        Debug.Log("Decor button clicked.");
        AttemptToBuy("Decor5", 20);
    }
}
