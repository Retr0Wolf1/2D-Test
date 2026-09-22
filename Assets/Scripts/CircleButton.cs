// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

[RequireComponent(typeof(Image))]
public class CircleButton : MonoBehaviour
{
    [Range(0f, 1f)]
    [FormerlySerializedAs("AlphaThreshold")]
    [SerializeField] private float _alphaThreshold = 0.5f;

    private void Start()
    {
        Image img = GetComponent<Image>();

        if (img != null)
        {
            img.alphaHitTestMinimumThreshold = _alphaThreshold;
        }
    }
}