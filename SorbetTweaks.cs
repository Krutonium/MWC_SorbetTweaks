using System;
using System.Collections.Generic;
using System.Linq;
using MSCLoader;
using HutongGames.PlayMaker;
using UnityEngine;

namespace SorbetTweaks
{
    public class SorbetTweaks : Mod
    {
        public override string ID => "SorbetTweaks"; // Your (unique) mod ID 
        public override string Name => "Sorbet Tweaks"; // Your mod name
        public override string Author => "Krutonium"; // Name of the Author (your name)
        public override string Version => "1.2"; // Version
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
        private static Drivetrain drivetrain;

        public SettingsSlider chargeRateSetting;
        public SettingsCheckBox showCurrentStateOfChargeInConsole;
        public SettingsCheckBox lockToMaxSetting;
        public SettingsDropDownList drivetrainSetting;
        public SettingsCheckBox autoTransmission;
        public SettingsSlider shiftUpRPMSetting;
        public SettingsSlider shiftDownRPMSetting;
        public SettingsSlider horsePower;
        public SettingsSlider carWeight;
        public SettingsSlider maxTorque;
        public SettingsSlider maxTorqueRPM;
        public SettingsSlider minimumRPM;
        public SettingsSlider maximumRPM;
        public SettingsCheckBox enableABS;
        public SettingsCheckBox enableTCS;
        public SettingsSlider minimumTCS;
        public SettingsSlider minimumABS;

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
            drivetrainSetting = Settings.AddDropDownList("transmission", "Transmission Type",
                Enum.GetNames(typeof(Drivetrain.Transmissions)), 1);
            Settings.AddText("XWD does not work, Sadly.");
            autoTransmission = Settings.AddCheckBox("autoTransmission", "Automatic Transmission", false);
            shiftUpRPMSetting = Settings.AddSlider("shiftUpRPM", "Shift Up RPM", 1000f, 8000f, 4500f);
            shiftDownRPMSetting = Settings.AddSlider("shiftDownRPM", "Shift Down RPM", 500f, 7000f, 2000f);
            horsePower = Settings.AddSlider("horsePower", "Engine Horsepower", 0f, 300f, 72f);
            carWeight = Settings.AddSlider("carWeight", "Car Weight", 0f, 2000f, 995f);
            maxTorque = Settings.AddSlider("maxTorque", "Maximum Torque", 100f, 500, 130f);
            maxTorqueRPM = Settings.AddSlider("maxTorqueRPM", "RPM at which you reach max Torque", 500f, 10000f, 3000f);
            maximumRPM = Settings.AddSlider("maxRPM", "Maxmimum Engine RPM", 5000f, 15000f, 7000f);
            minimumRPM = Settings.AddSlider("minRPM",
                "Minimum Engine RPM (Higher can avoid stalls, but uses more fuel)", 500f, 1500f, 650f);
            enableABS = Settings.AddCheckBox("abs", "Enable ABS (Antilock Breaks)", false);
            minimumABS = Settings.AddSlider("minABS", "Minimum Speed at which ABS works", 0f, 40f, 5f);
            enableTCS = Settings.AddCheckBox("tcs", "Enable TCS (Traction Control)", false);
            minimumTCS = Settings.AddSlider("minTCS", "Minimum Speed at which TCS works", 0f, 20f, 10f);
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

            if (drivetrain.rpm > (maximumRPM.GetValue() + 100))
            {
                drivetrain.rpm = maximumRPM.GetValue();
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

            PlayMakerFSM power = sorbet.GetComponentsInChildren<PlayMakerFSM>().ToList()
                .Find((PlayMakerFSM fsm) => fsm.FsmName == "Power");
            if (sorbet != null && power != null)
            {
                charge = power.FsmVariables.FindFsmFloat("Charge");
            }

            drivetrain = sorbet.GetComponent<Drivetrain>();
            Rigidbody rigidbody = sorbet.GetComponent<Rigidbody>();
            AxisCarController axisCarController = sorbet.GetComponent<AxisCarController>();
            if (drivetrain != null && rigidbody != null && axisCarController != null)
            {
                drivetrain.transmission =
                    StringToEnum<Drivetrain.Transmissions>(drivetrainSetting.GetSelectedItemName());
                ModConsole.Log($"Made Sorbett {drivetrainSetting.GetSelectedItemName()}");
                drivetrain.automatic = autoTransmission.GetValue();
                drivetrain.shiftUpRPM = (float)shiftUpRPMSetting.GetValue();
                drivetrain.shiftDownRPM = (float)shiftDownRPMSetting.GetValue();
                drivetrain.maxPower = horsePower.GetValue();
                drivetrain.maxTorque = maxTorque.GetValue();
                drivetrain.maxTorqueRPM = maxTorqueRPM.GetValue();
                drivetrain.minRPM = minimumRPM.GetValue();
                drivetrain.maxRPM = maximumRPM.GetValue();
                rigidbody.mass = carWeight.GetValue();
                axisCarController.ABS =  enableABS.GetValue();
                axisCarController.ABSMinVelocity = minimumABS.GetValue();
                axisCarController.TCS = enableTCS.GetValue();
                axisCarController.TCSMinVelocity = minimumTCS.GetValue();
            }
        }
    }
}