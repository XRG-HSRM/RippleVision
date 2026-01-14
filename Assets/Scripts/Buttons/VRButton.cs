using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    public TextMeshProUGUI tmp;

    private string text;
    [HideInInspector]
    public VRButtonGroup buttonGroup;

    [HideInInspector]
    public bool isEnabled;

    // button that gets edited by this one
    [HideInInspector]
    public VRButton mainButton;

    private Image image;
    private bool isHighlighted;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void Hover(bool hover)
    {
        if (isHighlighted || !isEnabled) return;
        image.color = hover ? new Color(0.9f, 0.95f, 1f, 1f) : Color.white;
    }

    public void SetupButton(VRButtonGroup bg, string text)
    {
        isEnabled = true;
        this.text = text;
        buttonGroup = bg;
        tmp.text = text;
    }

    public virtual void PressButton()
    {
        if (!isEnabled) return;
        if (mainButton != null)
        {
            EditButton();
        }
        else
        {
            buttonGroup.SetValue(this);
            HighlightButton(true);
        }
    }

    public virtual void EditButton()
    {
        string newValue = "";
        if (text == "+")
        {
            newValue = (int.Parse(mainButton.text) + 1).ToString();
        }
        if (text == "-")
        {
            newValue = (int.Parse(mainButton.text) - 1).ToString();
        }
        mainButton.text = newValue;
        mainButton.tmp.text = newValue;
        mainButton.PressButton();
    }

    public VRButtonGroup GetButtonGroup()
    {
        return buttonGroup;
    }

    private void HighlightButton(bool toHighlight)
    {
        if (!toHighlight)
        {
            isHighlighted = false;
            image.color = Color.white;
            return;
        }
        foreach (VRButton btn in buttonGroup.VRButtons)
        {
            if (btn != this && btn.isHighlighted)
            {
                btn.HighlightButton(false);
            }
        }
        if (image != null)
        {
            image.color = new Color(0.7f, 0.85f, 1f, 1f);
            isHighlighted = true;
        }
    }

    public string GetValue()
    {
        return text;
    }
}
