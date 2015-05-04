﻿using System;
using Rocket.RocketAPI;
using Rocket.Logging;
using unturned.ROCKS.Uconomy;
using SDG;
using Steamworks;

namespace Uconomy_Essentials
{
    public class CommandExchange : IRocketCommand
    {
        public bool RunFromConsole
        {
            get
            {
                return false;
            }
        }
        public string Name
        {
            get
            {
                return "exchange";
            }
        }
        public string Help
        {
            get
            {
                return "Exchanges experience for economy currency.";
            }
        }
        public void Execute(RocketPlayer playerid, string amt)
        {
            string message;
            if (string.IsNullOrEmpty(amt))
            {
                message = Uconomy_Essentials.Instance.Translate("exchange_usage_msg", new object[] { });
                RocketChatManager.Say(playerid, message);
                return;
            }
            string[] components0 = Parser.getComponentsFromSerial(amt, '/');
            if (components0.Length > 2)
            {
                message = Uconomy_Essentials.Instance.Translate("exchange_usage_msg", new object[] { });
                RocketChatManager.Say(playerid, message);
                return;
            }
            if (!Uconomy_Essentials.Instance.Configuration.ExpExchange && components0.Length == 1)
            {
                message = Uconomy_Essentials.Instance.Translate("experience_exchange_not_available", new object[] { });
                RocketChatManager.Say(playerid, message);
                return;
            }
            if (!Uconomy_Essentials.Instance.Configuration.MoneyExchange && components0.Length == 2)
            {
                message = Uconomy_Essentials.Instance.Translate("money_exchange_not_available", new object[] { });
                RocketChatManager.Say(playerid, message);
                return;
            }
            // Get expereience balance first
            uint exp = playerid.Experience;
            uint examt = 0;
            UInt32.TryParse(components0[0], out examt);
            if (examt <= 0)
            {
                message = Uconomy_Essentials.Instance.Translate("exchange_zero_amount_error", new object[] { });
                RocketChatManager.Say(playerid, message);
                return;
            }
            if (exp < examt && components0.Length == 1)
            {
                message = Uconomy_Essentials.Instance.Translate("exchange_insufficient_experience", new object[] { examt.ToString() });
                RocketChatManager.Say(playerid, message);
                return;
            }
            // Get balance first
            decimal bal = Uconomy.Instance.Database.GetBalance(playerid.CSteamID);
            if (components0.Length > 1 && bal < examt)
            {
                message = Uconomy_Essentials.Instance.Translate("exchange_insufficient_money", new object[] { examt.ToString(), Uconomy.Instance.Configuration.MoneyName });
                RocketChatManager.Say(playerid, message);
                return;
            }
            switch (components0.Length)
            {
                case 1:
                    decimal gain = (decimal)((float)examt * Uconomy_Essentials.Instance.Configuration.ExpExchangerate);
                    // Just to make sure to avoid any errors
                    gain = Decimal.Round(gain, 2);
                    decimal newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, gain);
                    message = Uconomy_Essentials.Instance.Translate("new_balance_msg", new object[] { newbal.ToString(), Uconomy.Instance.Configuration.MoneyName });
                    RocketChatManager.Say(playerid, message);
                    playerid.Experience -= examt;
                    Uconomy_Essentials.HandleEvent(playerid, gain, "exchange", examt);
                    break;
                case 2:
                    uint gainm = (uint)((float)examt * Uconomy_Essentials.Instance.Configuration.MoneyExchangerate);
                    // Just to make sure to avoid any errors
                    newbal = Uconomy.Instance.Database.IncreaseBalance(playerid.CSteamID, (examt * -1.0m));
                    message = Uconomy_Essentials.Instance.Translate("new_balance_msg", new object[] { newbal.ToString(), Uconomy.Instance.Configuration.MoneyName });
                    RocketChatManager.Say(playerid, message);
                    playerid.Experience += gainm;
                    Uconomy_Essentials.HandleEvent(playerid, gainm, "exchange", examt, "money");
                    break;
            }
            playerid.Player.SteamChannel.send("tellExperience", ESteamCall.OWNER, ESteamPacket.UPDATE_TCP_BUFFER, new object[]
		    {
			    playerid.Experience
		    });
        }
    }
}
