using System;
using System.Linq;
using MSCLoader;
using HutongGames.PlayMaker;
using UnityEngine;

namespace SorbetTweaks {
    public class SorbetTweaks : Mod {
        public override string ID => "SorbetTweaks"; // Your (unique) mod ID 
        public override string Name => "Sorbet Tweaks"; // Your mod name
        public override string Author => "Krutonium"; // Name of the Author (your name)
        public override string Version => "1.0"; // Version
        public override string Description => "A collection of Tweaks for your Sorbet"; // Short description of your mod
        public override Game SupportedGames => Game.MyWinterCar; //Supported Games
        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.FixedUpdate, Mod_FixedUpdate);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }
        

        private static GameObject sorbet;
        private static FsmFloat charge;
        
        public SettingsSlider chargeRateSetting;
        public SettingsCheckBox showCurrentStateOfChargeInConsole;
        public SettingsCheckBox lockToMaxSetting;
        public SettingsDropDownList drivetrainSetting;
        public SettingsCheckBox autoTransmission;
        public SettingsSlider shiftUpRPMSetting;
        public SettingsSlider shiftDownRPMSetting;

        public void Mod_Settings()
        {
            Settings.AddText("These Settings can ONLY be applied by reloading your save! Please configure and Reload!");
            Settings.AddText("I know this looks small, but trust me, at 0.05 you might as well click Lock to Max");
            chargeRateSetting = Settings.AddSlider("chargeRate",
                "How fast should the battery charge back to full?", 0f, 0.1f, 0f);
            showCurrentStateOfChargeInConsole = Settings.AddCheckBox("showCurrentStateOfChargeInConsole",
                "Show current State of Charge in Console (WILL PRINT A LOT)", false);
            lockToMaxSetting =
                Settings.AddCheckBox("cheatFullPower", "Lock Battery to Full Charge (can never die)", false);
            drivetrainSetting = Settings.AddDropDownList("transmission", "Transmission Type", Enum.GetNames(typeof(Drivetrain.Transmissions)), 1);
            Settings.AddText("XWD does not work, Sadly.");
            autoTransmission = Settings.AddCheckBox("autoTransmission", "Automatic Transmission", false);
            shiftUpRPMSetting = Settings.AddSlider("shiftUpRPM", "Shift Up RPM", 1000f, 8000f, 4500f);
            shiftDownRPMSetting = Settings.AddSlider("shiftDownRPM", "Shift Down RPM", 500f, 7000f, 2000f);
        }

        private string[] EnumToStringList<T>() where T : struct
        {
            return Enum.GetNames(typeof(T));
        }

        private T StringToEnum<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
        
        

        
        private void Mod_FixedUpdate()
        {
            float TargetValue = 120f;
            //Apply Battery Tweaks - 50 times a second.
            if (charge != null)
            {
                if ((bool)lockToMaxSetting.GetValue())
                {
                    charge.Value = TargetValue;
                }
                else
                {
                    float chargeRate = (float)chargeRateSetting.GetValue();
                    if (charge.Value < TargetValue)
                    {
                        charge.Value += chargeRate;
                    }

                    if (charge.Value >= (TargetValue += 0.1f))
                    {
                        charge.Value = TargetValue;
                    }
                }

                if (showCurrentStateOfChargeInConsole.GetValue())
                {
                    ModConsole.Print(charge.Value);
                }
            }
        }

        
        private void Mod_OnLoad()
        {
            sorbet = GameObject.Find("SORBET(190-200psi)");
            if (sorbet == null)
            {
                ModConsole.Error("FAILED TO FIND SORBET!!!");
                return;
            }
            PlayMakerFSM power = sorbet.GetComponentsInChildren<PlayMakerFSM>().ToList().Find((PlayMakerFSM fsm) => fsm.FsmName == "Power");
            if (sorbet != null && power != null)
            {
                charge = power.FsmVariables.FindFsmFloat("Charge");
            }
            Drivetrain drivetrain = sorbet.GetComponent<Drivetrain>();
            
            if (drivetrain != null)
            {
                drivetrain.transmission = StringToEnum<Drivetrain.Transmissions>(drivetrainSetting.GetSelectedItemName());
                ModConsole.Log($"Made Sorbett {drivetrainSetting.GetSelectedItemName()}");
                drivetrain.automatic = autoTransmission.GetValue();
                drivetrain.shiftUpRPM = (float)shiftUpRPMSetting.GetValue();
                drivetrain.shiftDownRPM = (float)shiftDownRPMSetting.GetValue();
            }
        }
    }
}