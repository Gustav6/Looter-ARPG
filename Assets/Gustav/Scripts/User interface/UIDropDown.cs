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

    private readonly List<Transform> contentTransforms = new();
    public Dictionary<Transform, Resolution> ResolutionTransformPair;
    [field: SerializeField] public Transform SelectedResolutionTransform { get; private set; }

    public override void Start()
    {
        ResolutionTransformPair = new();

        for (int i = 0; i < contentsParent.childCount; i++)
        {
            contentTransforms.Add(contentsParent.GetChild(i));
        }


        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            string resolution = Screen.resolutions[Screen.resolutions.Length - 1 - i].width.ToString() + " x " + Screen.resolutions[Screen.resolutions.Length - 1 - i].height.ToString();

            if (i < 3)
            {
                contentTransforms[i].GetComponentInChildren<TextMeshProUGUI>().text = resolution;
            }
            else
            {
                GameObject g = Instantiate(contentPrefab, contentsParent);

                g.GetComponentInChildren<TextMeshProUGUI>().text = resolution;

                contentTransforms.Add(g.transform);
            }

            ResolutionTransformPair.Add(contentTransforms[i], Screen.resolutions[Screen.resolutions.Length - 1 - i]);
            contentTransforms[i].name = resolution;
        }

        ChangeResolution(contentTransforms[0]);
        HighlightResolution(contentTransforms[0]);

        base.Start();

        if (SelectableScript != null)
        {
            SelectableScript.PointerEnter += SelectableScript_PointerEnter;
            SelectableScript.PointerExit += SelectableScript_PointerExit;
        }
    }

    public override void Update()
    {
        base.Update();
    }


    private void SelectableScript_PointerEnter(object sender, PointerEventData e)
    {
        TransitionSystem.AddTransition(new RectSizeTransition(rectTransform, 0.25f, new Vector2(rectTransform.sizeDelta.x, 250), TransitionType.SmoothStop2), gameObject);

        if (TransitionSystem.transitionPairs.ContainsKey(contentsParent.gameObject))
        {
            TransitionSystem.transitionPairs.Remove(contentsParent.gameObject);
        }
    }

    private void SelectableScript_PointerExit(object sender, PointerEventData e)
    {
        TransitionSystem.AddTransition(new RectSizeTransition(rectTransform, 0.25f, new Vector2(rectTransform.sizeDelta.x, 80), TransitionType.SmoothStart2), gameObject);
        TransitionSystem.AddTransition(new MoveTransition(contentsParent, 0.3f, new Vector2(0, -contentsParent.position.y), TransitionType.SmoothStart2, false), contentsParent.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        for (int i = 0; i < contentTransforms.Count; i++)
        {
            if (contentTransforms[i].GetComponent<BoxCollider2D>().OverlapPoint(Input.mousePosition))
            {
                Debug.Log(i);
            }
        }
    }

    public void ChangeResolution(Transform transform)
    {
        if (transform == SelectedResolutionTransform)
        {
            return;
        }

        if (SelectedResolutionTransform != null)
        {
            TransitionSystem.AddTransition(new ColorTransition(SelectedResolutionTransform, 0.1f, new Color(1, 1, 1, 0.5f), TransitionType.SmoothStop2), SelectedResolutionTransform.gameObject);
        }

        Screen.SetResolution(ResolutionTransformPair[transform].width, ResolutionTransformPair[transform].height, UIManager.Instance.FullScreen);

        SelectedResolutionTransform = transform;

        Debug.Log("Resolution changed to: " + ResolutionTransformPair[transform].ToString());
    }

    public void HighlightResolution(Transform transform)
    {
        TransitionSystem.AddTransition(new ColorTransition(transform, 0.1f, new Color(1, 1, 1, 1), TransitionType.SmoothStart2), gameObject);
    }
}
