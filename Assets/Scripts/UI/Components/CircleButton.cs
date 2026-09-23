// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CircleButton : MonoBehaviour
{
    [Range(0f, 1f)]
    [FormerlySerializedAs("AlphaThreshold")]
    [SerializeField] private float _alphaThreshold = 0.5f;

    private void Start()
    {
        var img = GetComponent<Image>();

        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = _alphaThreshold;
        }
    }
}