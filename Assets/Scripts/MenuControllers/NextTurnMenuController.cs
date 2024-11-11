using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnMenuController : MonoBehaviour
{
	public Image nextTurnButtonImage;
	public TMP_Text nextTurnButtonText;

	public void SetText(string text)
	{
		nextTurnButtonText.text = text;
	}

	internal void SetColor(Color color)
	{
		nextTurnButtonImage.color = color;
	}
}
