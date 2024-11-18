using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorTransition : Transition
{
    private readonly Image image;
    private readonly TextMeshProUGUI text;

    private readonly Color startingColor;
    private readonly Color targetColor;

    public ColorTransition(Component c, float time, Color target, TransitionType type, ExecuteAfterTransition execute = null)
    {
        if (c.GetComponent<Image>() != null)
        {
            image = c.GetComponent<Image>();
            startingColor = image.color;
        }
        else if (c.GetComponent<TextMeshProUGUI>() != null)
        {
            text = c.GetComponent<TextMeshProUGUI>();
            startingColor = text.color;
        }
        else
        {
            Debug.Log("Invalid Component provided");
            return;
        }

        transitionType = type;
        timerMax = time;

        targetColor = target;

        this.execute += execute;
    }

    //public ColorTransition(Component c, float time, CurveType type, float interval, float amplitude, Vector2 offset, ExecuteAfterTransition execute = null)
    //{
    //    if (c.GetComponent<Image>() != null)
    //    {
    //        image = c.GetComponent<Image>();
    //    }
    //    else if (c.GetComponent<TextMeshProUGUI>() != null)
    //    {
    //        text = c.GetComponent<TextMeshProUGUI>();
    //    }
    //    else
    //    {
    //        Debug.Log("Invalid Component provided");
    //        return;
    //    }

    //    curveType = type;
    //    timerMax = time;

    //    curveInterval = interval;
    //    curveAmplitude = amplitude;
    //    curveOffset = offset;

    //    this.execute += execute;
    //}

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();

        if (image == null && text == null)
        {
            isRemoved = true;
            return;
        }

        if (image != null)
        {
            image.color = Color.Lerp(startingColor, targetColor, t);
        }
        else if (text != null)
        {
            text.color = Color.Lerp(startingColor, targetColor, t);
        }
    }

    public override void RunAfterTransition()
    {
        SetColorToTarget();
    }

    public override void OnInstantTransition()
    {
        SetColorToTarget();

        isRemoved = true;
    }

    private void SetColorToTarget()
    {
        if (image != null)
        {
            image.color = targetColor;
        }
        else if (text != null)
        {
            text.color = targetColor;
        }
    }
}
