// Copyright (c) 2003-2026 Autism Group. All Rights Reserved.

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Имитация нижней шторки оплаты Google Play для тестового магазина:
// в редакторе и в Development Build. В релизную сборку код не попадает.
public class TestBillingSheet : MonoBehaviour
{
    private const float ReferenceWidth = 1080f;
    private const float SheetHeight = 700f;
    private const float AnimationTime = 0.25f;
    private const string TestAccount = "test.user@gmail.com";
    private const string FallbackFontPath = "Fonts & Materials/LiberationSans SDF";
    private const int SpriteSize = 64;

    private static readonly Color Dim = new Color(0f, 0f, 0f, 0.5f);
    private static readonly Color SheetColor = Color.white;
    private static readonly Color TextPrimary = new Color32(0x20, 0x21, 0x24, 0xFF);
    private static readonly Color TextSecondary = new Color32(0x5F, 0x63, 0x68, 0xFF);
    private static readonly Color Divider = new Color32(0xE8, 0xEA, 0xED, 0xFF);
    private static readonly Color PlayGreen = new Color32(0x01, 0x87, 0x5F, 0xFF);
    private static readonly Color ErrorRed = new Color32(0xD9, 0x30, 0x25, 0xFF);

    private static readonly string[] PaymentMethods =
    {
        "Тестовая карта, всегда одобряет",
        "Тестовая карта, всегда отклоняет",
    };

    private Action _onApprove;
    private Action _onFail;
    private RectTransform _sheet;
    private Image _dim;
    private RectTransform _content;
    private TextMeshProUGUI _paymentText;
    private int _paymentIndex;
    private bool _isClosing;

    private static Sprite _roundSprite;
    private static TMP_FontAsset _font;

    public static void Show(string title, string appName, string price, Action onApprove, Action onFail)
    {
        if (_roundSprite == null)
        {
            _roundSprite = CreateRoundSprite();
        }

        if (_font == null)
        {
            _font = LoadFont();
        }

        // В запасном шрифте может не быть ₽.
        if (_font != null && !_font.HasCharacter('₽', tryAddCharacter: true))
        {
            price = price.Replace("₽", "руб.");
        }

        var root = new GameObject("TestBillingSheet");
        var sheet = root.AddComponent<TestBillingSheet>();

        sheet._onApprove = onApprove;
        sheet._onFail = onFail;
        sheet.Build(title, appName, price);
    }

    // Системный шрифт ближе к Google Play: Roboto на Android, Segoe UI на Windows.
    private static TMP_FontAsset LoadFont()
    {
        foreach (var family in new[] { "Roboto", "Segoe UI" })
        {
            var systemFont = TMP_FontAsset.CreateFontAsset(family, "Regular");

            if (systemFont != null)
            {
                return systemFont;
            }
        }

        return Resources.Load<TMP_FontAsset>(FallbackFontPath);
    }

    // Белый квадрат со скруглёнными углами для 9-slice. Радиус задаётся через pixelsPerUnitMultiplier.
    private static Sprite CreateRoundSprite()
    {
        const float radius = SpriteSize / 2f;

        var texture = new Texture2D(SpriteSize, SpriteSize, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };

        var pixels = new Color32[SpriteSize * SpriteSize];

        for (int y = 0; y < SpriteSize; y++)
        {
            for (int x = 0; x < SpriteSize; x++)
            {
                // Круг со сглаженным краем: при нарезке 9-slice углы становятся четвертями круга.
                float dx = x + 0.5f - radius;
                float dy = y + 0.5f - radius;
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy));

                pixels[y * SpriteSize + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        var border = new Vector4(radius - 1f, radius - 1f, radius - 1f, radius - 1f);

        return Sprite.Create(texture, new Rect(0, 0, SpriteSize, SpriteSize), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
    }

    private void Build(string title, string appName, string price)
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, 1920f);
        scaler.matchWidthOrHeight = 0f;

        gameObject.AddComponent<GraphicRaycaster>();

        // Затемнение: тап мимо шторки отменяет покупку, как в Google Play.
        _dim = CreateImage("Dim", transform, Dim, 0f);
        Stretch(_dim.rectTransform);
        _dim.gameObject.AddComponent<Button>().onClick.AddListener(Cancel);

        // Шторка со скруглёнными верхними углами: нижние уходят за экран.
        var sheetImage = CreateImage("Sheet", transform, SheetColor, 40f);
        _sheet = sheetImage.rectTransform;
        _sheet.anchorMin = new Vector2(0f, 0f);
        _sheet.anchorMax = new Vector2(1f, 0f);
        _sheet.pivot = new Vector2(0.5f, 0f);
        _sheet.sizeDelta = new Vector2(0f, SheetHeight + 60f);

        _content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
        _content.SetParent(_sheet, false);
        _content.anchorMin = new Vector2(0f, 1f);
        _content.anchorMax = new Vector2(1f, 1f);
        _content.pivot = new Vector2(0.5f, 1f);
        _content.sizeDelta = new Vector2(0f, SheetHeight);
        _content.anchoredPosition = Vector2.zero;

        BuildPurchaseState(title, appName, price);

        // Выезд снизу.
        _sheet.anchoredPosition = new Vector2(0f, -SheetHeight - 60f);
        _sheet.DOAnchorPosY(-60f, AnimationTime).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject);
        _dim.color = Color.clear;
        _dim.DOColor(Dim, AnimationTime).SetUpdate(true).SetLink(gameObject);
    }

    private void BuildPurchaseState(string title, string appName, string price)
    {
        // Ручка шторки.
        var handle = CreateImage("Handle", _content, Divider, 4f);
        Place(handle.rectTransform, 0f, -28f, 72f, 8f, center: true);

        CreateText("Google Play", _content, 34, TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left, 48f, -70f, 400f);
        CreateText(TestAccount, _content, 30, TextSecondary, FontStyles.Normal, TextAlignmentOptions.Right, -48f, -70f, 500f, fromRight: true);

        // Товар: иконка, название, приложение, цена.
        var icon = CreateImage("Icon", _content, PlayGreen, 24f);
        Place(icon.rectTransform, 48f, -140f, 112f, 112f);
        var iconLetter = CreateText(appName.Substring(0, 1), icon.transform, 60, Color.white, FontStyles.Bold, TextAlignmentOptions.Center, 0f, 0f, 112f);
        Stretch(iconLetter.rectTransform);

        CreateText(title, _content, 40, TextPrimary, FontStyles.Normal, TextAlignmentOptions.Left, 190f, -150f, 560f);
        CreateText(appName, _content, 32, TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left, 190f, -205f, 560f);
        CreateText(price, _content, 40, TextPrimary, FontStyles.Normal, TextAlignmentOptions.Right, -48f, -150f, 300f, fromRight: true);

        var line = CreateImage("Divider", _content, Divider, 0f);
        Place(line.rectTransform, 48f, -290f, ReferenceWidth - 96f, 2f);

        // Способ оплаты: тап переключает тестовые карты, как выбор карты в Google Play.
        var paymentRow = CreateImage("Payment", _content, Color.clear, 0f);
        Place(paymentRow.rectTransform, 0f, -300f, ReferenceWidth, 120f);
        paymentRow.gameObject.AddComponent<Button>().onClick.AddListener(SwitchPaymentMethod);

        var card = CreateImage("Card", paymentRow.transform, TextSecondary, 6f);
        Place(card.rectTransform, 48f, -40f, 64f, 42f);
        _paymentText = CreateText(PaymentMethods[_paymentIndex], paymentRow.transform, 34, TextPrimary, FontStyles.Normal, TextAlignmentOptions.Left, 140f, -30f, 760f);
        CreateText(">", paymentRow.transform, 40, TextSecondary, FontStyles.Normal, TextAlignmentOptions.Right, -48f, -28f, 60f, fromRight: true);

        CreateText("Тестовая покупка: деньги не будут списаны.", _content, 28, TextSecondary, FontStyles.Normal, TextAlignmentOptions.Left, 48f, -440f, 900f);

        var buy = CreateButton("Купить", PlayGreen, OnBuyClicked);
        Place(buy, 48f, -540f, ReferenceWidth - 96f, 110f);
    }

    private void SwitchPaymentMethod()
    {
        _paymentIndex = (_paymentIndex + 1) % PaymentMethods.Length;
        _paymentText.text = PaymentMethods[_paymentIndex];
    }

    private void OnBuyClicked()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        ClearContent();

        bool isApproved = _paymentIndex == 0;

        if (isApproved)
        {
            ShowResult(null, PlayGreen, "Оплата прошла успешно");
            DOVirtual.DelayedCall(1.2f, () => Close(_onApprove), ignoreTimeScale: true).SetLink(gameObject);
        }
        else
        {
            ShowResult("!", ErrorRed, "Платёж отклонён. Выберите другой способ оплаты.");
            var ok = CreateButton("OK", PlayGreen, () => Close(_onFail));
            Place(ok, 48f, -540f, ReferenceWidth - 96f, 110f);
        }
    }

    // symbol == null — галочка из двух полосок: в шрифте может не быть символа ✓.
    private void ShowResult(string symbol, Color color, string message)
    {
        var circle = CreateImage("ResultIcon", _content, color, 80f);
        Place(circle.rectTransform, 0f, -120f, 160f, 160f, center: true);

        if (symbol == null)
        {
            CreateBar(circle.transform, new Vector2(-32f, -14f), 50f, -45f);
            CreateBar(circle.transform, new Vector2(17.5f, 0f), 90f, 45f);
        }
        else
        {
            var mark = CreateText(symbol, circle.transform, 90, Color.white, FontStyles.Bold, TextAlignmentOptions.Center, 0f, 0f, 160f);
            Stretch(mark.rectTransform);
        }

        CreateText(message, _content, 38, TextPrimary, FontStyles.Normal, TextAlignmentOptions.Center, 0f, -330f, ReferenceWidth - 96f, center: true);

        circle.transform.localScale = Vector3.zero;
        circle.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true).SetLink(gameObject);
    }

    private static void CreateBar(Transform parent, Vector2 position, float length, float angle)
    {
        var bar = CreateImage("Bar", parent, Color.white, 0f).rectTransform;
        bar.anchorMin = bar.anchorMax = bar.pivot = new Vector2(0.5f, 0.5f);
        bar.sizeDelta = new Vector2(length, 16f);
        bar.anchoredPosition = position;
        bar.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Cancel()
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        Close(_onFail);
    }

    private void Close(Action callback)
    {
        _sheet.DOAnchorPosY(-SheetHeight - 60f, AnimationTime).SetEase(Ease.InCubic).SetUpdate(true).SetLink(gameObject);
        _dim.DOColor(Color.clear, AnimationTime).SetUpdate(true).SetLink(gameObject)
            .OnComplete(() =>
            {
                Destroy(gameObject);
                callback?.Invoke();
            });
    }

    private void ClearContent()
    {
        for (int i = _content.childCount - 1; i >= 0; i--)
        {
            Destroy(_content.GetChild(i).gameObject);
        }
    }

    private RectTransform CreateButton(string label, Color color, Action onClick)
    {
        var image = CreateImage("Button", _content, color, 55f);
        image.gameObject.AddComponent<Button>().onClick.AddListener(() => onClick());

        var text = CreateText(label, image.transform, 40, Color.white, FontStyles.Bold, TextAlignmentOptions.Center, 0f, 0f, 100f);
        Stretch(text.rectTransform);

        return image.rectTransform;
    }

    // cornerRadius == 0 — прямоугольник без спрайта.
    private static Image CreateImage(string name, Transform parent, Color color, float cornerRadius)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false);
        image.color = color;

        if (cornerRadius > 0f)
        {
            image.sprite = _roundSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = SpriteSize / 2f / cornerRadius;
        }

        return image;
    }

    private static TextMeshProUGUI CreateText(string value, Transform parent, float size, Color color, FontStyles style,
        TextAlignmentOptions alignment, float x, float y, float width, bool fromRight = false, bool center = false)
    {
        var text = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(parent, false);
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;

        if (_font != null)
        {
            text.font = _font;
        }

        Place(text.rectTransform, x, y, width, size * 1.4f, fromRight, center);

        return text;
    }

    private static void Place(RectTransform rect, float x, float y, float width, float height, bool fromRight = false, bool center = false)
    {
        float anchorX = center ? 0.5f : fromRight ? 1f : 0f;

        rect.anchorMin = new Vector2(anchorX, 1f);
        rect.anchorMax = new Vector2(anchorX, 1f);
        rect.pivot = new Vector2(anchorX, 1f);
        rect.sizeDelta = new Vector2(width, height);
        rect.anchoredPosition = new Vector2(x, y);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
#endif
