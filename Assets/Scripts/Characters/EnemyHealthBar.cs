// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [FormerlySerializedAs("FillImage")]
    [SerializeField] private Image _fillImage;

    [FormerlySerializedAs("HeightOffset")]
    [SerializeField] private float _heightOffset = 0.8f;

    private Transform _target;
    private Camera _camera;

    public void Setup(Transform target, Camera camera)
    {
        _target = target;
        _camera = camera;
    }

    public void SetHealth(int current, int max)
    {
        if (_fillImage == null)
        {
            return;
        }

        var ratio = max > 0 ? (float)current / max : 0f;
        _fillImage.fillAmount = Mathf.Clamp01(ratio);

        if (ratio > 0.5f)
        {
            _fillImage.color = Color.green;
        }
        else if (ratio > 0.25f)
        {
            _fillImage.color = Color.yellow;
        }
        else
        {
            _fillImage.color = Color.red;
        }
    }

    private void LateUpdate()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = _target.position + new Vector3(0, _heightOffset, 0);
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        if (_camera != null)
        {
            transform.rotation = _camera.transform.rotation;
        }
    }
}