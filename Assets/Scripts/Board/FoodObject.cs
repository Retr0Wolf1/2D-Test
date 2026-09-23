// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;

public class FoodObject : CellObject
{
    [FormerlySerializedAs("AmountGranted")]
    [SerializeField] private int _amountGranted = 10;

    [FormerlySerializedAs("EatSound")]
    [SerializeField] private AudioClip _eatSound;

    public override void PlayerEntered()
    {
        if (AudioManager.Instance != null && _eatSound != null)
        {
            AudioManager.Instance.SFXSource.PlayOneShot(_eatSound, 2f);
        }

        GameManager.Instance.ChangeFood(_amountGranted);
        Destroy(gameObject);
    }
}