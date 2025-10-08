using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BotAdd : MonoBehaviour, IPointerClickHandler
{
    UIGradient image;
    BotBuy botBuy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<UIGradient>();
        image.bottomColor = Color.black;
        botBuy = GetComponentInParent<BotBuy>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            StartCoroutine(PressedColor());
            botBuy.AddBot();
        }
    }

    public IEnumerator PressedColor()
    {
        image.bottomColor = Color.gray;
        yield return new WaitForSeconds(0.1f);
        image.bottomColor = Color.black;
    }
}
