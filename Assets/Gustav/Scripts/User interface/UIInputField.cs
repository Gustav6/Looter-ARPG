using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIInputField : UIBaseScript, IPointerClickHandler
{
    [BoxGroup("Input field variables")]
    [SerializeField] private bool canWrite;

    [BoxGroup("Input field variables")]
    [SerializeField] private string standardText;

    [BoxGroup("Input field variables")]
    [SerializeField] private int maxAmountOfLetters;

    [BoxGroup("Input field variables")]
    [SerializeField] private TextMeshProUGUI textReference;

    public override void Start()
    {
        UpdateText();
        canWrite = false;
        textReference.text = standardText;

        base.Start();
    }

    public override void Update()
    {
        if (canWrite)
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // has backspace/delete been pressed?
                {
                    if (textReference.text.Length != 0)
                    {
                        textReference.text = textReference.text[..^1];
                    }
                }
                else if ((c == '\n') || (c == '\r')) // enter/return
                {
                    if (textReference.text != "")
                    {
                        Debug.Log("User entered: " + textReference.text);
                    }

                    canWrite = false;
                    UpdateText();

                    break;
                }
                else if (textReference.text.Length < maxAmountOfLetters)
                {
                    textReference.text += c;
                }
            }
        }

        base.Update();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        canWrite = !canWrite;

        if (textReference.text == standardText)
        {
            textReference.text = "";
            textReference.color = Color.green;
            return;
        }
        else if (textReference.text == "")
        {
            textReference.text = standardText;
            textReference.color = Color.black;
            return;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (canWrite)
        {
            TransitionSystem.AddTransition(new ColorTransition(textReference, 0.1f, Color.green, TransitionType.SmoothStop2), gameObject);
        }
        else
        {
            TransitionSystem.AddTransition(new ColorTransition(textReference, 0.1f, Color.black, TransitionType.SmoothStop2), gameObject);
        }
    }
}
