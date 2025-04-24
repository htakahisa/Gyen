using UnityEngine;

public class BotSetting : MonoBehaviour
{

    public GameObject Bot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddBot()
    {
        GameObject bot = Instantiate(Bot);
        //ƒIƒ“ƒ‰ƒCƒ““¯Šú‚·‚é‚È‚çSpawn‚·‚×‚«
        SetBot(bot);
    }

    public void DeleteBot()
    {
        if (RoundManager.rm.GetBots().Count >= 1)
        {
            Destroy(RoundManager.rm.GetBots()[Random.Range(0, RoundManager.rm.GetBots().Count - 1)]);
        }
    }

    public void SetBot(GameObject bot)
    {
        bot.GetComponent<BotManager>().ResetPos();
        bot.GetComponent<HpMaster>().armer = RoundManager.rm.GetMyPlayer().GetComponent<HpMaster>().armer;
    }

    public void BotStopping()
    {
        RoundManager.rm.currentBotMove = RoundManager.BotMove.STOP;
    }

    public void BotWalking()
    {
        RoundManager.rm.currentBotMove = RoundManager.BotMove.WALK;
    }

    public void BotRunning()
    {
        RoundManager.rm.currentBotMove = RoundManager.BotMove.RUN;
    }



}
