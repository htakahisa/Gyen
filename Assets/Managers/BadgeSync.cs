using Mirror;
using System.Collections.Generic;

public class BadgeSync : NetworkBehaviour
{
    public readonly SyncList<BadgeManager.Badges> syncedBadges = new SyncList<BadgeManager.Badges>();

    private void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (isLocalPlayer)
        {
            // 自分用パネルに表示
            BadgeUIManager.Instance.UpdateLocalUI(syncedBadges);
        }
        else
        {
            // 相手用パネルに表示
            BadgeUIManager.Instance.UpdateRemoteUI(syncedBadges);
        }
    }

    // バッジ追加をサーバーに依頼
    [Command]
    public void CmdAddBadge(BadgeManager.Badges badge)
    {
        if (!syncedBadges.Contains(badge))
            syncedBadges.Add(badge);
    }
}
