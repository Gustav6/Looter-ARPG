using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private DialogueObject dialogue;
    [SerializeField] private bool isSpeaking;
    private DialogueUI dialogueReference;

    private bool canInteractWith = false;

    private void Start()
    {
        dialogueReference = UIManager.Instance.ObjectPairs[InstantiatedObjectType.dialogueCanvas].GetComponent<DialogueUI>();

        dialogueReference.OnDialogueFinished += Dialogue_OnDialogueFinished;
        isSpeaking = false;
    }

    private void Update()
    {
        if (canInteractWith && !isSpeaking)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                dialogueReference.ShowDialogue(dialogue);
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

        dialogueReference.CloseDialogueBox();
        canInteractWith = false;
    }
}
