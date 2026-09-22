// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour
{
    public static ViewManager Instance { get; private set; }

    [Serializable]
    public class ViewEntry
    {
        public ViewType Type;
        public ViewBase Prefab;
    }

    [SerializeField] private ViewEntry[] _views;
    [SerializeField] private Transform _pageRoot;
    [SerializeField] private Transform _popupRoot;
    [SerializeField] private Transform _messageRoot;

    private readonly Dictionary<Type, ViewBase> _prefabs = new Dictionary<Type, ViewBase>();
    private readonly Dictionary<Type, ViewBase> _instances = new Dictionary<Type, ViewBase>();

    private readonly Stack<ViewBase> _pageStack = new Stack<ViewBase>();
    private readonly Stack<ViewBase> _popupStack = new Stack<ViewBase>();
    private readonly Stack<ViewBase> _messageStack = new Stack<ViewBase>();

    public event Action<ViewBase> OnViewShown;
    public event Action<ViewBase> OnViewHidden;

    public ViewBase CurrentPage => _pageStack.Count > 0 ? _pageStack.Peek() : null;
    public ViewBase CurrentPopup => _popupStack.Count > 0 ? _popupStack.Peek() : null;
    public ViewBase CurrentMessage => _messageStack.Count > 0 ? _messageStack.Peek() : null;

    public int PageCount => _pageStack.Count;
    public int PopupCount => _popupStack.Count;
    public int MessageCount => _messageStack.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (ViewEntry entry in _views)
        {
            if (entry.Prefab == null)
            {
                continue;
            }

            _prefabs[entry.Prefab.GetType()] = entry.Prefab;
        }
    }

    private void Start()
    {
        OpenPage<MainPageView>();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public T OpenPage<T>() where T : ViewBase
    {
        return Open<T>(ViewType.Page);
    }

    public T OpenPopup<T>() where T : ViewBase
    {
        return Open<T>(ViewType.Popup);
    }

    public T OpenMessage<T>() where T : ViewBase
    {
        return Open<T>(ViewType.Message);
    }

    public void ClosePage<T>() where T : ViewBase
    {
        Close<T>(ViewType.Page);
    }

    public void ClosePopup<T>() where T : ViewBase
    {
        Close<T>(ViewType.Popup);
    }

    public void CloseMessage<T>() where T : ViewBase
    {
        Close<T>(ViewType.Message);
    }

    public void CloseAll()
    {
        CloseAllInStack(_pageStack);
        CloseAllInStack(_popupStack);
        CloseAllInStack(_messageStack);

        UpdateTimeScale();
    }

    public void DestroyAllViews()
    {
        foreach (ViewBase view in _instances.Values)
        {
            if (view != null)
            {
                view.OnViewShown -= HandleViewShown;
                view.OnViewHidden -= HandleViewHidden;
                Destroy(view.gameObject);
            }
        }

        _instances.Clear();

        _pageStack.Clear();
        _popupStack.Clear();
        _messageStack.Clear();

        UpdateTimeScale();
    }

    public void Cleanup()
    {
        foreach (ViewBase view in _instances.Values)
        {
            if (view == null)
            {
                continue;
            }

            view.OnViewShown -= HandleViewShown;
            view.OnViewHidden -= HandleViewHidden;
        }

        _instances.Clear();
        _prefabs.Clear();

        _pageStack.Clear();
        _popupStack.Clear();
        _messageStack.Clear();

        OnViewShown = null;
        OnViewHidden = null;
    }

    private T Open<T>(ViewType type) where T : ViewBase
    {
        Type key = typeof(T);
        Stack<ViewBase> stack = GetStack(type);

        if (stack.Count > 0 && stack.Peek().GetType() == key)
        {
            return stack.Peek() as T;
        }

        if (!_instances.TryGetValue(key, out ViewBase view))
        {
            if (!_prefabs.TryGetValue(key, out ViewBase prefab))
            {
                Debug.LogError("Prefab not found for " + key.Name);
                return null;
            }

            Transform root = GetRoot(type);
            view = Instantiate(prefab, root);
            view.SetType(type);
            view.OnViewShown += HandleViewShown;
            view.OnViewHidden += HandleViewHidden;
            _instances[key] = view;
        }

        if (stack.Count > 0)
        {
            ViewBase top = stack.Peek();

            if (top == view)
            {
                return view as T;
            }

            top.Hide();
        }

        stack.Push(view);
        view.Show();

        UpdateTimeScale();

        return view as T;
    }

    private void Close<T>(ViewType type) where T : ViewBase
    {
        Type key = typeof(T);

        if (!_instances.TryGetValue(key, out ViewBase view))
        {
            return;
        }

        Stack<ViewBase> stack = GetStack(type);

        if (stack.Count == 0 || stack.Peek() != view)
        {
            return;
        }

        stack.Pop();
        view.Hide();

        if (stack.Count > 0)
        {
            stack.Peek().Show();
        }

        UpdateTimeScale();
    }

    private void CloseAllInStack(Stack<ViewBase> stack)
    {
        while (stack.Count > 0)
        {
            ViewBase view = stack.Pop();
            view.Hide();
        }
    }

    private void UpdateTimeScale()
    {
        if (_popupStack.Count == 0)
        {
            Time.timeScale = 1f;
            return;
        }

        ViewBase top = _popupStack.Peek();

        if (top is GameOverPopupView)
        {
            Time.timeScale = 1f;
            return;
        }

        Time.timeScale = 0f;
    }

    private Stack<ViewBase> GetStack(ViewType type)
    {
        switch (type)
        {
            case ViewType.Page: return _pageStack;
            case ViewType.Popup: return _popupStack;
            case ViewType.Message: return _messageStack;
        }

        return _pageStack;
    }

    private Transform GetRoot(ViewType type)
    {
        switch (type)
        {
            case ViewType.Page: return _pageRoot;
            case ViewType.Popup: return _popupRoot;
            case ViewType.Message: return _messageRoot;
        }

        return _pageRoot;
    }

    private void HandleViewShown(ViewBase view)
    {
        OnViewShown?.Invoke(view);
    }

    private void HandleViewHidden(ViewBase view)
    {
        OnViewHidden?.Invoke(view);
    }
}