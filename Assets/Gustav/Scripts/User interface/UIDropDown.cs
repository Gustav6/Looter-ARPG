using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDropDown : UIBaseScript, IPointerClickHandler
{
    [BoxGroup("Drop down variables")]
    [SerializeField] private RectTransform rectTransform;

    [BoxGroup("Drop down variables")]
    [SerializeField] private Transform contentsParent;

    [BoxGroup("Drop down variables")]
    [SerializeField] private GameObject contentPrefab;

    private readonly List<Transform> settings = new();

    public override void Start()
    {
        for (int i = 0; i < contentsParent.childCount; i++)
        {
            settings.Add(contentsParent.GetChild(i));
        }


        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (i < 3)
            {
                settings[i].GetComponentInChildren<TextMeshProUGUI>().text = Screen.resolutions[i].width.ToString() + " x " + Screen.resolutions[i].height.ToString();
            }
            else
            {
                GameObject g = Instantiate(contentPrefab, contentsParent);
                g.GetComponentInChildren<TextMeshProUGUI>().text = Screen.resolutions[i].width.ToString() + " x " + Screen.resolutions[i].height.ToString();

                settings.Add(g.transform);
            }
        }

        base.Start();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void RunOnEnable()
    {
        TransitionSystem.AddTransition(new RectSizeTransition(rectTransform, 0.25f, new Vector2(rectTransform.sizeDelta.x, 250), TransitionType.SmoothStop2), gameObject);

        if (TransitionSystem.transitionPairs.ContainsKey(contentsParent.gameObject))
        {
            TransitionSystem.transitionPairs.Remove(contentsParent.gameObject);
        }

        base.RunOnEnable();
    }

    public override void RunOnDisable()
    {
        TransitionSystem.AddTransition(new RectSizeTransition(rectTransform, 0.25f, new Vector2(rectTransform.sizeDelta.x, 80), TransitionType.SmoothStart2), gameObject);
        TransitionSystem.AddTransition(new MoveTransition(contentsParent, 0.3f, new Vector2(0, -contentsParent.position.y), TransitionType.SmoothStart2, false), contentsParent.gameObject);

        base.RunOnDisable();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        for (int i = 0; i < settings.Count; i++)
        {
            if (settings[i].GetComponent<BoxCollider2D>().OverlapPoint(Input.mousePosition))
            {
                Debug.Log(i);
            }
        }
    }

    private void ChangeResolution(Resolution resolution)
    {
        Screen.SetResolution(resolution.width, resolution.height, true);
    }
}
