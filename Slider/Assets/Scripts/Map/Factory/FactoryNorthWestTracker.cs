using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// lazy state machine
public class FactoryNorthWestTracker : MonoBehaviour
{
    public PoweredDoor LeftDoor;
    public PoweredDoor RightDoor;

    public enum NorthWestPosition
    {
        Airlock,
        Electric,
        Closet,
    }

    private NorthWestPosition playerPosition;

    public void SetPlayerAirlock()  => SetPlayerPosition(NorthWestPosition.Airlock);
    public void SetPlayerElectric() => SetPlayerPosition(NorthWestPosition.Electric);
    public void SetPlayerCloset()   => SetPlayerPosition(NorthWestPosition.Closet);

    public void SetPlayerPosition(NorthWestPosition position)
    {
        switch (position)
        {
            case NorthWestPosition.Airlock:
                // Electric -> Airlock
                if (playerPosition == NorthWestPosition.Electric)
                {
                    // if Player is holding bob and door is not powered -> cheating
                    // if Player is holding bob and door is powered -> hasbob
                    if (IsPlayerHoldingBob())
                    {
                        if (IsRightDoorPowered())
                        {
                            SaveSystem.Current.SetBool("factoryAcquiredBob", true);
                        }
                        else
                        {
                            SaveSystem.Current.SetBool("factoryBobCheated", true);
                        }
                    }

                    if(SaveSystem.Current.GetBool("factoryClosetSoftlock"))
                    {
                        //was softlocked and escaped
                        SaveSystem.Current.SetBool("factoryClosetSoftlock", false);
                        SaveSystem.Current.SetBool("factoryEscapedClosetSoftlock", true);
                    }
                }
                break;

            case NorthWestPosition.Electric:
                break;

            case NorthWestPosition.Closet:
                // if left door is closed -> softlock
                if (!IsLeftDoorPowered())
                {
                    SaveSystem.Current.SetBool("factoryClosetSoftlock", true);
                    SaveSystem.Current.SetBool("factoryEscapedClosetSoftlock", false);

                }
                break;
        }
        playerPosition = position;
    }

    private bool IsPlayerHoldingBob()   => PlayerInventory.GetCurrentItem() != null && PlayerInventory.GetCurrentItem().itemName == "Conductive Bob";
    private bool IsLeftDoorPowered()    => LeftDoor.Powered;
    private bool IsRightDoorPowered()   => RightDoor.Powered;
}
