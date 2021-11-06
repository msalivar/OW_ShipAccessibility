using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ModTemplate
{
    public class MainClass : ModBehaviour
    {
        private static IModHelper modHelper;
        private ShipCockpitController shipController;
        private bool isControllingShip;

        // Settings
        private static bool usingGamepad;
        private static bool matchVelocityToggleEnabled;
        private static bool shipMovementToggleEnabled;
        private static bool shipUpDownToggleEnabled;

        // Control States
        private static bool matchVelocityToggled;
        private static bool keyWToggled;
        private static bool keyAToggled;
        private static bool keySToggled;
        private static bool keyDToggled;
        private static bool upThrustToggled;
        private static bool downThrustToggled;

        public override void Configure(IModConfig config)
        {
            usingGamepad = config.GetSettingsValue<bool>("usingGamepad");
            matchVelocityToggleEnabled = config.GetSettingsValue<bool>("matchVelocityToggleEnabled");
            shipMovementToggleEnabled = config.GetSettingsValue<bool>("shipMovementToggleEnabled");
            shipUpDownToggleEnabled = config.GetSettingsValue<bool>("shipUpDownToggleEnabled");
            ModHelper.Console.WriteLine($"matchVelocityToggleEnabled: {matchVelocityToggleEnabled}", MessageType.Info);
            ModHelper.Console.WriteLine($"shipMovementToggleEnabled: {shipMovementToggleEnabled}", MessageType.Info);
            ModHelper.Console.WriteLine($"shipUpDownToggleEnabled: {shipUpDownToggleEnabled}", MessageType.Info);
        }

        private void Start()
        {
            modHelper = ModHelper;
            ModHelper.Console.WriteLine($"My mod {nameof(MainClass)} is loaded!", MessageType.Success);

            ModHelper.HarmonyHelper.AddPostfix<OWInput>("IsNewlyReleased", typeof(MainClass), "IsNewlyReleased");
            ModHelper.HarmonyHelper.AddPostfix<OWInput>("GetValue", typeof(MainClass), "GetValue");
            ModHelper.HarmonyHelper.AddPostfix<Autopilot>("StopMatchVelocity", typeof(MainClass), "StopMatchVelocity");

            ModHelper.Events.Subscribe<ShipCockpitController>(Events.AfterAwake);
            ModHelper.Events.Event += OnEvent;
        }

        private void Update()
        {
            if(shipController == null || !shipController.IsPlayerAtFlightConsole())
            {
                if(isControllingShip)
                {
                    matchVelocityToggled = false;
                    keyWToggled = false;
                    keyAToggled = false;
                    keySToggled = false;
                    keyDToggled = false;
                    upThrustToggled = false;
                    downThrustToggled = false;

                    isControllingShip = false;
                }

                return;
            }

            isControllingShip = true;
            if (usingGamepad)
            {
                CheckGamepadState();
            }
            else
            {
                CheckKeyboardState();
            }
        }

        private void OnEvent(MonoBehaviour behaviour, Events ev)
        {
            if (behaviour is ShipCockpitController cockpitController && ev == Events.AfterAwake)
            {
                ModHelper.Console.WriteLine("ShipCockpitController loaded!");
                shipController = cockpitController;
            }
        }

        #region Helpers

        private void CheckKeyboardState()
        {
            if (Keyboard.current[Key.Z].wasPressedThisFrame)
            {
                matchVelocityToggled = false;
                keyWToggled = false;
                keyAToggled = false;
                keySToggled = false;
                keyDToggled = false;
                upThrustToggled = false;
                downThrustToggled = false;
            }
            if (Keyboard.current[Key.Space].wasPressedThisFrame)
            {
                matchVelocityToggled = !matchVelocityToggled;
                if(matchVelocityToggled)
                {
                    keyWToggled = false;
                    keyAToggled = false;
                    keySToggled = false;
                    keyDToggled = false;
                    upThrustToggled = false;
                    downThrustToggled = false;
                }
            }
            if (Keyboard.current[Key.W].wasPressedThisFrame)
            {
                keyWToggled = !keyWToggled;
                keySToggled = keyWToggled ? false : keySToggled;
            }
            if (Keyboard.current[Key.A].wasPressedThisFrame)
            {
                keyAToggled = !keyAToggled;
                keyDToggled = keyAToggled ? false : keyDToggled;
            }
            if (Keyboard.current[Key.S].wasPressedThisFrame)
            {
                keySToggled = !keySToggled;
                keyWToggled = keySToggled ? false : keyWToggled;
            }
            if (Keyboard.current[Key.D].wasPressedThisFrame)
            {
                keyDToggled = !keyDToggled;
                keyAToggled = keyDToggled ? false : keyAToggled;
            }
            if (Keyboard.current[Key.LeftShift].wasPressedThisFrame)
            {
                upThrustToggled = !upThrustToggled;
                downThrustToggled = upThrustToggled ? false : downThrustToggled;
            }
            if (Keyboard.current[Key.LeftCtrl].wasPressedThisFrame)
            {
                downThrustToggled = !downThrustToggled;
                upThrustToggled = downThrustToggled ? false : upThrustToggled;
            }
        }

        private void CheckGamepadState()
        {

        }

        #endregion

        #region Patches

        private static void IsNewlyReleased(ref bool __result, IInputCommands command)
        {
            if (matchVelocityToggleEnabled && __result && command == InputLibrary.matchVelocity)
            {
                __result = !matchVelocityToggled;
            }
        }

        private static void GetValue(ref float __result, IInputCommands command)
        {
            if (shipUpDownToggleEnabled && command == InputLibrary.thrustUp)
            {
                __result = upThrustToggled ? 1 : 0;
            }
            else if (shipUpDownToggleEnabled && command == InputLibrary.thrustDown)
            {
                __result = downThrustToggled ? 1 : 0;
            }
            else if (shipMovementToggleEnabled && command == InputLibrary.thrustX)
            {
                if(keyAToggled)
                {
                    __result = -1;
                }
                else if (keyDToggled)
                {
                    __result = 1;
                }
                else
                {
                    __result = 0;
                }
            }
            else if (shipMovementToggleEnabled && command == InputLibrary.thrustZ)
            {
                if (keySToggled)
                {
                    __result = -1;
                }
                else if (keyWToggled)
                {
                    __result = 1;
                }
                else
                {
                    __result = 0;
                }
            }
        }

        private static void StopMatchVelocity()
        {
            matchVelocityToggled = false;
        }

        #endregion

    }
}