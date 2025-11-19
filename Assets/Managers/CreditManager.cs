using Mirror;
using UnityEngine;

public class CreditManager : NetworkBehaviour
{
    [SyncVar] public int credit = 800;

    // スロットごとの現在の支払い金額
    [SyncVar] public int currentPrimaryPaying = 0;
    [SyncVar] public int currentSidearmPaying = 0;
    [SyncVar] public int currentArmorPaying = 0;

    [SyncVar] public int rounds = 0;

    // 購入対象のスロット
    public enum PurchaseSlot
    {
        Sidearm,
        Primary,
        Armor
    }

    // クレジットを追加（サーバー専用）
    [Server]
    public void AddCredit(int value)
    {
        credit += value;
    }

    // ラウンド数追加（サーバー専用）
    [Server]
    public void GiveRound()
    {
        rounds++;
    }

    // 購入コマンド（払い戻し対応）
    [Command]
    public void CmdBuy(int cost, PurchaseSlot slot)
    {
        switch (slot)
        {
            case PurchaseSlot.Primary:
                credit += currentPrimaryPaying;
                credit -= cost;
                currentPrimaryPaying = cost;
                break;

            case PurchaseSlot.Sidearm:
                credit += currentSidearmPaying;
                credit -= cost;
                currentSidearmPaying = cost;
                break;

            case PurchaseSlot.Armor:
                credit += currentArmorPaying;
                credit -= cost;
                currentArmorPaying = cost;
                break;
        }
    }

    [Command]
    public void CmdSell(PurchaseSlot slot)
    {
        switch (slot)
        {
            case PurchaseSlot.Primary:
                credit += currentPrimaryPaying;
                currentPrimaryPaying = 0;
                break;

            case PurchaseSlot.Sidearm:
                credit += currentSidearmPaying;
                currentSidearmPaying = 0;
                break;

            case PurchaseSlot.Armor:
                credit += currentArmorPaying;
                currentArmorPaying = 0;
                break;
        }
    }

    // 所持金＋払い戻し分で購入可能かどうか
    public bool CanBuy(int cost, PurchaseSlot slot)
    {
        int refundable = 0;
        switch (slot)
        {
            case PurchaseSlot.Primary:
                refundable = currentPrimaryPaying;
                break;
            case PurchaseSlot.Sidearm:
                refundable = currentSidearmPaying;
                break;
            case PurchaseSlot.Armor:
                refundable = currentArmorPaying;
                break;
        }

        return cost <= credit + refundable;
    }

    // ラウンド開始時などに支払い履歴をリセット
    public void ResetCurrentPaying()
    {
        currentPrimaryPaying = 0;
        currentSidearmPaying = 0;
        currentArmorPaying = 0;
    }
}
