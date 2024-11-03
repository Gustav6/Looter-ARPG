using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class ColorTransition : Transition
{
    private Image image;
    private TextMeshProUGUI text;

    private Color startingColor;
    private Color targetColor;

    private float t;

    public ColorTransition(Image i, float time, Color target, TransitionType transition, ExecuteAfterTransition execute = null)
    {
        image = i;

        timerMax = time;
        this.transition = transition;

        startingColor = i.color;
        targetColor = target;

        this.execute += execute;
    }

    public ColorTransition(TextMeshProUGUI t, float time, Color target, TransitionType transition, ExecuteAfterTransition execute = null)
    {
        text = t;

        this.transition = transition;
        timerMax = time;

        startingColor = t.color;
        targetColor = target;


        this.execute += execute;
    }
    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        if (image == null && text == null)
        {
            isRemoved = true;
            return;
        }

        t = transition switch
        {
            TransitionType.SmoothStart2 => TransitionSystem.SmoothStart2(timer / timerMax),
            TransitionType.SmoothStart3 => TransitionSystem.SmoothStart3(timer / timerMax),
            TransitionType.SmoothStart4 => TransitionSystem.SmoothStart4(timer / timerMax),
            TransitionType.SmoothStop2 => TransitionSystem.SmoothStop2(timer / timerMax),
            TransitionType.SmoothStop3 => TransitionSystem.SmoothStop3(timer / timerMax),
            TransitionType.SmoothStop4 => TransitionSystem.SmoothStop4(timer / timerMax),
            _ => 0,
        };

        if (image != null)
        {
            image.color = Color.Lerp(startingColor, targetColor, t);
        }
        else if (text != null)
        {
            text.color = Color.Lerp(startingColor, targetColor, t);
        }

        base.Update();
    }

    public override void RunAfterTransition()
    {
        if (image != null)
        {
            image.color = targetColor;
        }
        else if (text != null)
        {
            text.color = targetColor;
        }
        else
        {
            isRemoved = true;
            return;
        }

        base.RunAfterTransition();
    }
}
