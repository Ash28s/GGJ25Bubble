using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    public static PlaneManager Instance; // Singleton instance
    private PlaneController2D[] allPlanes;
    private int currentPlaneIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep the manager alive across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Cache all planes at the start
        allPlanes = FindObjectsOfType<PlaneController2D>();

        // Ensure planes are ordered by index
        System.Array.Sort(allPlanes, (a, b) => a.planeIndex.CompareTo(b.planeIndex));

        // Turn off all colliders initially
        foreach (var plane in allPlanes)
        {
            if (plane != null)
            {
                Collider2D collider = plane.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = false; // Disable the collider
                }
            }
        }

        // Enable the first plane
        ActivatePlane(0);
    }

    public void ActivateNextPlane()
    {
        // Disable the current plane and set its tag to default
        if (currentPlaneIndex < allPlanes.Length && allPlanes[currentPlaneIndex] != null)
        {
            allPlanes[currentPlaneIndex].EnableControl(false);
            allPlanes[currentPlaneIndex].gameObject.tag = "Untagged"; // Set the tag to default
            Collider2D collider = allPlanes[currentPlaneIndex].GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false; // Disable the collider
            }
        }

        // Move to the next plane
        currentPlaneIndex++;
        if (currentPlaneIndex < allPlanes.Length)
        {
            ActivatePlane(currentPlaneIndex);
        }
        else
        {
            Debug.LogWarning("No more planes to activate.");
        }
    }

    private void ActivatePlane(int index)
    {
        if (index < 0 || index >= allPlanes.Length || allPlanes[index] == null)
        {
            Debug.LogError("Invalid plane index.");
            return;
        }

        currentPlaneIndex = index;
        allPlanes[index].EnableControl(true);
        allPlanes[index].gameObject.tag = "Player"; // Set the active plane's tag to "Player"
        Collider2D collider = allPlanes[index].GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true; // Enable the collider
        }

        Debug.Log($"Plane {index} activated.");
    }

    public int GetCurrentPlaneIndex()
    {
        return currentPlaneIndex;
    }
}
