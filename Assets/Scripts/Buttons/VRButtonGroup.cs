using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRButtonGroup : MonoBehaviour
{
    [Header("Button Prefab and Width")]
    public GameObject textPrafab;
    public GameObject buttonPrefab;
    public GameObject planeBack;
    public float btnWidth;
    public Transform buttonParent;

    [HideInInspector]
    public string question;
    [HideInInspector]
    public int answerOptions;
    [HideInInspector]
    public float questionHeight;

    private SceneController controller;
    [HideInInspector]
    public List<VRButton> VRButtons = new();
    private string text;
    private int id = 0;

    private VRButton highlightedButton;

    public void CreateButtons(SceneController controller, int id, float width, List<string> answer, bool isEditable)
    {
        this.id = id;
        this.controller = controller;
        List<VRButton> btns = new List<VRButton>();
        btnWidth = width;
        float totalWidth = btnWidth * answerOptions;
        float startX = -totalWidth / 2f + btnWidth / 2f;

        var questionText = Instantiate(textPrafab, buttonParent);
        questionText.GetComponent<TextMeshProUGUI>().text = question;

        GameObject btnPre = null;
        GameObject btnSeq = null;
        GameObject btn = null;
        VRButton vrBtnPre = null;
        VRButton vrBtnSeq = null;
        VRButton vrBtn = null;
        int pos = 0;
        for (int i = 0; i < answerOptions; i++)
        {
            if (isEditable)
            {
                btnPre = Instantiate(buttonPrefab, buttonParent);
                btnPre.transform.localPosition = new Vector3(startX + btnWidth * pos, -questionHeight + 0.475f, 0);
                vrBtnPre = btnPre.GetComponent<VRButton>();
                vrBtnPre.SetupButton(this, "-");
                btns.Add(vrBtnPre);
                pos++;
            }

            btn = Instantiate(buttonPrefab, buttonParent);
            btn.transform.localPosition = new Vector3(startX + btnWidth * pos, -questionHeight + 0.475f, 0);
            RectTransform rt = btn.GetComponent<RectTransform>();
            Vector2 size = rt.sizeDelta;
            size.x *= width * 5f;
            rt.sizeDelta = size;
            BoxCollider btnCollider = btn.GetComponent<BoxCollider>();
            size = btnCollider.size;
            size.x *= width * 5f;
            btnCollider.size = size;
            vrBtn = btn.GetComponent<VRButton>();
            vrBtn.SetupButton(this, answer[i]);
            btns.Add(vrBtn);
            pos++;

            if (isEditable)
            {
                btnSeq = Instantiate(buttonPrefab, buttonParent);
                btnSeq.transform.localPosition = new Vector3(startX + btnWidth * pos, -questionHeight + 0.475f, 0);
                vrBtnSeq = btnSeq.GetComponent<VRButton>();
                vrBtnSeq.SetupButton(this, "+");
                btns.Add(vrBtnSeq);
                pos++;

                vrBtnPre.mainButton = vrBtn;
                vrBtnSeq.mainButton = vrBtn;
            }
        }
        VRButtons = btns;
        planeBack.GetComponent<Renderer>().material.color = Color.red;
    }

    public void SetValue(VRButton button)
    {
        highlightedButton = button;

        text = button.GetValue();

        planeBack.GetComponent<Renderer>().material.color = Color.green;

        controller.SetAnswer(id, text);

        foreach (VRButton btn in VRButtons)
        {
            Image buttonImage = btn.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.white;
            }
        }
    }
}
