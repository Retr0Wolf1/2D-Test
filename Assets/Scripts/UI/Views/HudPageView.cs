// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

using TMPro;

public class HudPageView : ViewBase
{
    [SerializeField] private Button _pauseButton;
    [SerializeField] private TextMeshProUGUI _foodLabel;
    [SerializeField] private TextMeshProUGUI _goToExitText;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private Color _normalFoodColor = Color.white;
    [SerializeField] private Color _lowFoodColor = Color.red;
    [SerializeField] private int _lowFoodThreshold = 5;
    [SerializeField] private float _blinkSpeed = 2f;
    [SerializeField] private Color _goToExitColorA = Color.white;
    [SerializeField] private Color _goToExitColorB = Color.yellow;
    [SerializeField] private float _goToExitBlinkSpeed = 2f;
    [SerializeField] private float _hintCharDelay = 0.03f;
    [SerializeField] private float _hintDuration = 5f;

    private bool _isLowFood;
    private bool _isGoToExitVisible;
    private bool _hintTyping;
    private bool _hintVisible;
    private Coroutine _hintCoroutine;

    protected override void OnShow()
    {
        _pauseButton.onClick.AddListener(OnPauseClicked);
        _goToExitText.gameObject.SetActive(false);
        _hintText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_foodLabel != null && _isLowFood)
        {
            var t = Mathf.PingPong(Time.unscaledTime * _blinkSpeed, 1f);
            _foodLabel.color = Color.Lerp(_normalFoodColor, _lowFoodColor, t);
        }

        if (_goToExitText != null && _isGoToExitVisible)
        {
            var t = Mathf.PingPong(Time.unscaledTime * _goToExitBlinkSpeed, 1f);
            _goToExitText.color = Color.Lerp(_goToExitColorA, _goToExitColorB, t);
        }

        if (_hintVisible && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SkipHint();
        }
    }

    public void UpdateFood(int amount)
    {
        if (_foodLabel == null)
        {
            return;
        }

        _foodLabel.text = LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", "food_label") + " : " + amount;

        if (amount <= _lowFoodThreshold)
        {
            _isLowFood = true;
        }
        else
        {
            _isLowFood = false;
            _foodLabel.color = _normalFoodColor;
        }
    }

    public void ShowGoToExit()
    {
        _goToExitText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", "goexit_message");
        _isGoToExitVisible = true;
        _goToExitText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideGoToExit));
        Invoke(nameof(HideGoToExit), 3f);
    }

    public void ShowHint()
    {
        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
        }

        _hintCoroutine = StartCoroutine(TypeHint());
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

    private System.Collections.IEnumerator TypeHint()
    {
        var full = LocalizationSettings.StringDatabase.GetLocalizedString("UI_Texts", "hint_message");

        _hintText.text = "";
        _hintText.gameObject.SetActive(true);
        _hintText.raycastTarget = false;

        _hintVisible = true;
        _hintTyping = true;

        for (var i = 0; i <= full.Length; i++)
        {
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
        _hintVisible = false;
        _hintTyping = false;

        if (_hintCoroutine != null)
        {
            StopCoroutine(_hintCoroutine);
            _hintCoroutine = null;
        }

        _hintText.gameObject.SetActive(false);
    }

    private void HideGoToExit()
    {
        _isGoToExitVisible = false;
        _goToExitText.gameObject.SetActive(false);
    }

    protected override void OnHide()
    {
        _pauseButton.onClick.RemoveAllListeners();
        HideGoToExit();
        HideHint();
    }

    private void OnPauseClicked()
    {
        ViewManager.Instance.OpenPopup<PausePopupView>();
    }
}