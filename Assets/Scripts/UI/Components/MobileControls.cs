// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MobileControls : MonoBehaviour
{
    [SerializeField] private GameObject _mobileRootPrefab;
    [SerializeField] private Transform _parent;
    [SerializeField] private bool _showInEditor = false;

    private GameObject _mobileInstance;

    private IEnumerator Start()
    {
        if (_mobileRootPrefab == null) yield break;

        bool show;

#if UNITY_ANDROID && !UNITY_EDITOR
        show = true;
#elif UNITY_EDITOR
        show = _showInEditor;
#else
        show = false;
#endif

        if (!show) yield break;

        var instance = Instantiate(_mobileRootPrefab, _parent);
        instance.name = "MobileControlsRoot";
        _mobileInstance = instance;

        var joystick = instance.GetComponentInChildren<Joystick>(true);

        while (GameManager.Instance == null || GameManager.Instance.PlayerController == null)
        {
            yield return null;
        }

        var player = GameManager.Instance.PlayerController;

        if (joystick != null)
        {
            player.SetJoystick(joystick);
        }

        var attackButtonTransform = FindDeep(instance.transform, "AttackButton");
        var attackButton = attackButtonTransform != null
            ? attackButtonTransform.GetComponent<Button>()
            : null;

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(player.OnAttackButton);
        }

        while (ViewManager.Instance == null)
        {
            yield return null;
        }

        ViewManager.Instance.OnViewShown += HandleViewChanged;
        ViewManager.Instance.OnViewHidden += HandleViewChanged;

        UpdateVisibility();
    }

    private void OnDestroy()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnViewShown -= HandleViewChanged;
            ViewManager.Instance.OnViewHidden -= HandleViewChanged;
        }
    }

    private void HandleViewChanged(ViewBase view)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (_mobileInstance == null) return;
        if (ViewManager.Instance == null) return;

        bool inGame = ViewManager.Instance.CurrentPage is HudPageView;

        _mobileInstance.SetActive(inGame);
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var r = FindDeep(child, name);
            if (r != null) return r;
        }
        return null;
    }
}