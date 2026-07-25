using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSpriteTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("Target Graphic (Choose One)")]
    [SerializeField] private SpriteRenderer targetWorldSprite;
    [SerializeField] private Image targetUIImage;

    [Header("Sprite States")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite clickSprite;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateSprite(hoverSprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateSprite(defaultSprite);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSprite(clickSprite);
    }

    private void UpdateSprite(Sprite newSprite)
    {
        if (newSprite == null) return;

        if (targetWorldSprite != null)
        {
            targetWorldSprite.sprite = newSprite;
        }

        if (targetUIImage != null)
        {
            targetUIImage.sprite = newSprite;
        }
    }
}