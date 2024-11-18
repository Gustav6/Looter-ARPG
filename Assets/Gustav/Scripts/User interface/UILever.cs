using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILever : UIBaseScript, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClickEvent;

    [BoxGroup("Lever variables")]
    [SerializeField] private VolumeType volumeEffected;

    [BoxGroup("Lever variables")]
    [SerializeField] private GameObject objectIndicator;

    private bool leverOn;
    private Image indicatorsImage;

    private float previousValue;

    public override void Start()
    {
        previousValue = SoundManager.Instance.GetVolume(volumeEffected);

        indicatorsImage = objectIndicator.GetComponent<Image>();

        leverOn = true;
        UpdateIndicator(0);

        base.Start();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        leverOn = !leverOn;
        UpdateIndicator(0.1f);
        onClickEvent?.Invoke();
    }

    public void UpdateIndicator(float timeItTakes)
    {
        if (leverOn)
        {
            if (timeItTakes <= 0)
            {
                objectIndicator.SetActive(true);
                return;
            }

            objectIndicator.SetActive(true);
            TransitionSystem.AddTransition(new ColorTransition(indicatorsImage, timeItTakes, Color.white, TransitionType.SmoothStart2), gameObject);
        }
        else
        {
            if (timeItTakes <= 0)
            {
                objectIndicator.SetActive(false);
                return;
            }

            TransitionSystem.AddTransition(new ColorTransition(indicatorsImage, timeItTakes, Color.white * 0, TransitionType.SmoothStop2, DisableIndicator), gameObject);
        }
    }

    public void SetFullscreen()
    {
        if (leverOn)
        {
            UIManager.Instance.EnableFullscreen();
        }
        else
        {
            UIManager.Instance.DisableFullscreen();
        }

        Screen.fullScreen = UIManager.Instance.FullScreen;
    }

    public void SetSoundStatus()
    {
        if (leverOn)
        {
            SoundManager.Instance.SetVolume(volumeEffected, previousValue);
        }
        else
        {
            previousValue = SoundManager.Instance.GetVolume(volumeEffected);
            SoundManager.Instance.SetVolume(volumeEffected, 0);
        }
    }

    private void DisableIndicator()
    {
        objectIndicator.SetActive(false);
    }
}
