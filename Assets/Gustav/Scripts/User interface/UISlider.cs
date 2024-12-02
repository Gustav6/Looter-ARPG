using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISlider : UIBaseScript, IPointerDownHandler, IPointerUpHandler
{
    [BoxGroup("Slider variables")]
    [SerializeField] private VolumeType volumeEffected;

    [BoxGroup("Slider variables")]
    [SerializeField] private GameObject slidingPart;

    private float maxMoveValue;

    private bool canMove = false;

    public override void Start()
    {
        maxMoveValue = Mathf.Abs(slidingPart.transform.localPosition.x);
        slidingPart.transform.localPosition = PercentageToPosition(SoundManager.Instance.GetVolume(volumeEffected));

        base.Start();

        if (SelectableScript != null)
        {
            SelectableScript.PointerExit += SelectableScript_PointerExit;
        }
    }

    private void SelectableScript_PointerExit(object sender, PointerEventData e)
    {
        SoundManager.Instance.SetVolume(volumeEffected, TotalSlidingPercentage());
    }

    public override void Update()
    {
        if (canMove)
        {
            MoveSliderTowardsMouse(Input.mousePosition.x);
        }

        base.Update();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        canMove = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        canMove = false;

        SoundManager.Instance.SetVolume(volumeEffected, TotalSlidingPercentage());
    }

    public float TotalSlidingPercentage()
    {
        return ((slidingPart.transform.localPosition.x + maxMoveValue) / (maxMoveValue * 2)) * UIManager.Instance.ResolutionScaling;
    }

    public Vector3 PercentageToPosition(float value)
    {
        return new Vector3((value * maxMoveValue * 2) - maxMoveValue, slidingPart.transform.localPosition.y);
    }

    private void MoveSliderTowardsMouse(float mouseX)
    {
        if (mouseX > transform.position.x + (maxMoveValue * UIManager.Instance.ResolutionScaling) * transform.localScale.x)
        {
            slidingPart.transform.localPosition = new(maxMoveValue, 0);
        }
        else if (mouseX < transform.position.x + (-maxMoveValue * UIManager.Instance.ResolutionScaling) * transform.localScale.x)
        {
            slidingPart.transform.localPosition = new(-maxMoveValue, 0);
        }
        else
        {
            slidingPart.transform.position = new(mouseX, transform.position.y);
        }
    }
}
