using HullRepairs;
using UnityEngine;
using PulsarModLoader.Chat.Commands.CommandRouter;
using PulsarModLoader.Utilities;

namespace HullRepairs 
{
    class RepairCommand : PublicCommand
    {
        public override string[] CommandAliases()
        {
            return new string[] { "repair" };
        }

        public override string Description()
        {
            return "Repair ship hull (Host Only)";
        }

        public override string[][] Arguments()
        {
            return new string[1][]{new string[1]{"thirty"}};
        }

        public override void Execute(string arguments, int SenderID)
        {
            PLPlayer player = PLServer.Instance.GetPlayerFromPlayerID(SenderID);

            // Checks
            if (!PhotonNetwork.isMasterClient)
            {
                Messaging.Notification("Must be host to use this command.", player);
                return;
            }

            if (PLEncounterManager.Instance.PlayerShip == null || PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList.Count < 1)
            {
                Messaging.Notification("No repair depot in this sector.", player);
                return;
            }

            if (
                PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip == null
                || !PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip.GetIsPlayerShip())
            {
                Messaging.Notification("Pull into the repair depot.", player);
                return;
            }

            if (PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip.ShieldIsActive)
            {
                Messaging.Notification("We can't repair your ship while your shields are up.", player);
                return;
            }

            string[] args = arguments.Split(new char[1]{' '});

            // Effects
            switch (args[0].ToLower())
            {
                default: // full repair

                    int amountHull = 0;

                    int amountCredits = 0;

                    PLRepairDepot.GetAutoPurchaseInfo(PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip, out amountHull, out amountCredits);

                    PLServer.Instance.photonView.RPC("ServerRepairHull", PhotonTargets.MasterClient, PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip.ShipID, amountHull, amountCredits);

                    break;

                case "thirty": // repair hull to thirty percent

                    float current = PLEncounterManager.Instance.PlayerShip.MyStats.HullCurrent;

                    float max = PLEncounterManager.Instance.PlayerShip.MyStats.HullMax;

                    // Want a little under thirty percent for BKP meta.
                    float thirty = ((max / 10f) * 3f) - 1;

                    if (current > thirty)
                    {
                        string msg = string.Format("Denied: repair would result in loss of hull: ({0}/{1})", thirty, max);

                        Messaging.Notification(msg, PLServer.Instance.GetPlayerFromPlayerID(SenderID));

                        break;
                    }

                    // Cannot underflow since current is less than or equal.
                    int purchaseAmountCredits = (int)(thirty - current) * 2;

                    purchaseAmountCredits = Mathf.Min(purchaseAmountCredits, PLServer.Instance.CurrentCrewCredits);

                    int purchaseAmountHull = purchaseAmountCredits / 2;

                    PLServer.Instance.photonView.RPC("ServerRepairHull", PhotonTargets.MasterClient, PLEncounterManager.Instance.PlayerShip.MyFlightAI.cachedRepairDepotList[0].TargetShip.ShipID, purchaseAmountHull, purchaseAmountCredits);

                    break;
            }
        }
    }
}
