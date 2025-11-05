using UnityEngine;
public class ModelSizeChecker : MonoBehaviour
{
    void Start()
    {
        Vector3 size = GetObjectWorldSize(gameObject);
        Debug.Log($"{name} world size: {size}");
    }

    public static Vector3 GetObjectWorldSize(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return Vector3.zero;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds.size; // size in world units (e.g., meters if using meters)
    }
}
