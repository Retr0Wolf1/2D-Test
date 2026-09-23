// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public abstract class ViewBase : MonoBehaviour
{
    public event Action<ViewBase> OnViewShown;
    public event Action<ViewBase> OnViewHidden;

    public ViewType Type { get; private set; }
    public bool IsOpen { get; private set; }

    public void SetType(ViewType type)
    {
        Type = type;
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
        IsOpen = true;

        OnShow();
        OnViewShown?.Invoke(this);
    }

    public virtual void Hide()
    {
        OnHide();

        gameObject.SetActive(false);
        IsOpen = false;

        OnViewHidden?.Invoke(this);
    }

    protected virtual void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    protected virtual void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale locale)
    {
        if (IsOpen)
        {
            OnHide();
            OnShow();
        }
    }

    protected virtual void OnShow() { }
    protected virtual void OnHide() { }

    protected virtual void OnDestroy()
    {
        OnViewShown = null;
        OnViewHidden = null;
    }
}