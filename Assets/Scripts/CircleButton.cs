using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleButton : MonoBehaviour
{
    [Range(0f, 1f)]
    public float AlphaThreshold = 0.5f;

    private void Start()
    {
        Image img = GetComponent<Image>();
        if (img != null)
            img.alphaHitTestMinimumThreshold = AlphaThreshold;
    }
}