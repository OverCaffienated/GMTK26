using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SwordHoverSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image swordImage;
    [SerializeField] private Sprite sheathedSprite;
    [SerializeField] private Sprite unsheathedSprite;

    private void Start()
    {
        swordImage.sprite = sheathedSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        swordImage.sprite = unsheathedSprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        swordImage.sprite = sheathedSprite;
    }
}