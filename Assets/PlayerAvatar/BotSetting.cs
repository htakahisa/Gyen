using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotSetting : NetworkBehaviour, IPointerClickHandler
{

    private UIGradient image;
    public RoundManager.BotMove mode;

    private void Start()
    {
        image = GetComponent<UIGradient>();
        image.bottomColor = Color.black;
    }


    // Start is called before the first frame update
    void Update()
    {
        if (RoundManager.rm.currentBotMove == mode)
        {
            image.bottomColor = Color.gray;
        }
        else
        {
            image.bottomColor = Color.black;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ChangeBotMode();
        }
    }

  

    public void ChangeBotMode()
    {
        RoundManager.rm.currentBotMove = mode;
    }

}
