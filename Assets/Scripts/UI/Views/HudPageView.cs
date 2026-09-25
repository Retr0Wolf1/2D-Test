// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using TMPro;

public class HudPageView : ViewBase
{
    [SerializeField] private Button _pauseButton;
    [SerializeField] private TextMeshProUGUI _foodValue;
    [SerializeField] private TextMeshProUGUI _goToExitText;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private Color _normalFoodColor = Color.white;
    [SerializeField] private Color _lowFoodColor = Color.red;
    [SerializeField] private int _lowFoodThreshold = 5;
    [SerializeField] private float _blinkSpeed = 2f;
    [SerializeField] private Color _goToExitColorA = Color.white;
    [SerializeField] private Color _goToExitColorB = Color.yellow;
    [SerializeField] private float _hintCharDelay = 0.03f;
    [SerializeField] private float _hintDuration = 5f;

    private bool _isLowFood;
    private bool _isGoToExitVisible;
    private bool _hintTyping;
    private bool _hintVisible;
    private Coroutine _hintCoroutine;

    protected override void OnShow()
    {
        if (_pauseButton != null)
        {
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        if (_goToExitText != null)
        {
            _goToExitText.gameObject.SetActive(false);
        }

        if (_hintText != null)
        {
            _hintText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_foodValue != null && _isLowFood)
        {
            var t = Mathf.PingPong(Time.unscaledTime * _blinkSpeed, 1f);
            _foodValue.color = Color.Lerp(_normalFoodColor, _lowFoodColor, t);
        }

        if (_hintVisible && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SkipHint();
        }
    }

    public void UpdateFood(int amount)
    {
        if (_foodValue == null)
        {
            return;
        }

        _foodValue.text = amount.ToString();

        if (amount <= _lowFoodThreshold)
        {
            _isLowFood = true;
        }
        else
        {
            _isLowFood = false;
            _foodValue.color = _normalFoodColor;
        }
    }

    public void ShowGoToExit()
    {
        if (_goToExitText == null)
        {
            return;
        }

        _isGoToExitVisible = true;
        _goToExitText.gameObject.SetActive(true);

        _goToExitText.color = _goToExitColorA;

        _goToExitText.DOColor(_goToExitColorB, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);

        CancelInvoke(nameof(HideGoToExit));
        Invoke(nameof(HideGoToExit), 3f);
    }

    public void ShowHint()
    {
        if (_hintText == null)
        {
            return;
        }

        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
        }

        // Сначала активируем — LocalizeStringEvent запишет текст
        _hintText.gameObject.SetActive(true);

        var hintText = _hintText.text;

        if (string.IsNullOrEmpty(hintText))
        {
            Debug.LogWarning("HintText пустой!");
            return;
        }

        // Обнуляем alpha и текст в этом же кадре
        var c = _hintText.color;
        c.a = 0f;
        _hintText.color = c;

        _hintText.text = "";
        _hintText.raycastTarget = false;

        // Принудительно перерисовываем меш — чтобы текст исчез немедленно
        _hintText.ForceMeshUpdate();

        // Плавное появление + печать
        _hintText.DOFade(1f, 0.3f)
            .OnComplete(() => _hintCoroutine = StartCoroutine(TypeHint(hintText)));
    }

    public void SkipHint()
    {
        if (!_hintVisible)
        {
            return;
        }

        if (_hintTyping)
        {
            _hintTyping = false;
        }
        else
        {
            HideHint();
        }
    }

    private System.Collections.IEnumerator TypeHint(string full)
    {
        if (_hintText == null)
        {
            yield break;
        }

        _hintText.text = "";

        _hintVisible = true;
        _hintTyping = true;

        for (var i = 0; i <= full.Length; i++)
        {
            if (_hintText == null)
            {
                yield break;
            }

            if (!_hintTyping)
            {
                _hintText.text = full;
                break;
            }

            _hintText.text = full.Substring(0, i);
            yield return new WaitForSecondsRealtime(_hintCharDelay);
        }

        _hintTyping = false;

        yield return new WaitForSecondsRealtime(_hintDuration);

        HideHint();
    }

    private void HideHint()
    {
        if (_hintText == null)
        {
            return;
        }

        _hintVisible = false;
        _hintTyping = false;

        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
            _hintCoroutine = null;
        }

        _hintText.DOFade(0f, 0.3f)
            .OnComplete(() =>
            {
                if (_hintText != null)
                {
                    _hintText.gameObject.SetActive(false);
                }
            });
    }

    private void HideGoToExit()
    {
        if (_goToExitText == null)
        {
            return;
        }

        _isGoToExitVisible = false;

        _goToExitText.DOKill();
        _goToExitText.gameObject.SetActive(false);
    }

    protected override void OnHide()
    {
        if (_pauseButton != null)
        {
            _pauseButton.onClick.RemoveAllListeners();
        }

        HideGoToExit();
        HideHint();
    }

    private void OnPauseClicked()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OpenPopup<PausePopupView>();
        }
    }
}