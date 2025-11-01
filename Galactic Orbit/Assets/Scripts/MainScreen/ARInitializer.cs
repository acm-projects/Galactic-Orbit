using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ARInitializer : MonoBehaviour
{


    async void OnEnable()
    {
        await WaitForARControllerReady();
        await Task.Delay(200);
        SimpleARController.Instance.ToggleARMode();
    }

    async void OnDisable()
    {
        SimpleARController.Instance.ToggleARMode();
    }

    private async Task WaitForARControllerReady()
    {
        // Wait until the instance exists
        while (SimpleARController.Instance == null)
            await Task.Yield();
    }

}
