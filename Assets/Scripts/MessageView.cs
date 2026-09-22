// Copyright (c) 2012-2021 FuryLion Group. All Rights Reserved.

using UnityEngine;
using TMPro;

public class MessageView : ViewBase
{
    [SerializeField] private TextMeshProUGUI _messageText;

    public void SetMessage(string message)
    {
        _messageText.text = message;
    }

    protected override void OnShow()
    {
        Invoke(nameof(AutoClose), 2f);
    }

    private void AutoClose() => ViewManager.Instance.CloseMessage<MessageView>();
}