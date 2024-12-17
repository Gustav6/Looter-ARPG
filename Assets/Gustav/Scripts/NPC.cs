using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private DialogueObject dialogue;
    [SerializeField] private bool isSpeaking;

    private bool canInteractWith = false;

    private void Start()
    {
        DialogueUI.Instance.OnDialogueFinished += Dialogue_OnDialogueFinished;
        isSpeaking = false;
    }

    private void Update()
    {
        if (canInteractWith && !isSpeaking)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                DialogueUI.Instance.ShowDialogue(dialogue);
                isSpeaking = true;
            }
        }
    }

    private void Dialogue_OnDialogueFinished(object sender, System.EventArgs e)
    {
        isSpeaking = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
        {
            return;
        }

        canInteractWith = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.transform.CompareTag("Player"))
        {
            return;
        }

        DialogueUI.Instance.CloseDialogueBox();
        canInteractWith = false;
    }
}
