using UnityEngine;
using UnityEngine.EventSystems;

public class SelectableCharacter : MonoBehaviour, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Player clicked: " + gameObject.name);
        PauseUIManager.Instance.ShowCharacterUI(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        PauseUIManager.Instance.ShowCharacterUI(gameObject);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        PauseUIManager.Instance.HideCharacterUI();
    }
}