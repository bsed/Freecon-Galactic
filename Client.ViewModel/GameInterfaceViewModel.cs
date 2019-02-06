using System;
using System.Collections.Generic;
using Freecon.Client.Objects;
using Freecon.Client.Objects.Weapons;
using Freecon.Client.Core.Interfaces;
using Freecon.Client.ViewModel.GameInterfaceComponents;
using Core.Interfaces;
using Freecon.Client.Core.States;
using Freecon.Client.View.CefSharp.States;

namespace Freecon.Client.ViewModel
{
    public class GameInterfaceViewModel : IViewModel
    {        
        //@Free
        //It seems like this class would lend itself to specialization for different game states.
        //Seems odd to cram all of this functionality (update weapon stats, update ship stats) into the same class for all game states


        public GlobalGameInterfaceState CurrentGlobalGameInterfaceState { get; set; }

        UIStateManagerContainer StateContainer;

        public GameInterfaceViewModel()
        {
            
        }

        public void Update(IGameTimeService gameTime)
        {
            if (StateContainer == null)//Currently should only be possible if first update is called after initialization before GameStateManager sets the state container  
            {
                return;
            }

            switch (StateContainer.CurrentAreaType)
            {
                case GameStateType.Space:
                case GameStateType.Planet:
                    {
                        var stateContainer = StateContainer as PlayableUIStateManagerContainer;

                        // Don't think this should be possible, but adding it just in case
                        if (stateContainer.PlayerShipManager.PlayerShip == null)
                        {
                            throw new Exception("Inconsistent UI State. Bailing out!");
                        }

                        CurrentGlobalGameInterfaceState = new GlobalGameInterfaceState(
                        new StatBarDisplayState(
                            UpdateShipStatDisplayState(stateContainer.PlayerShipManager.PlayerShip),
                            UpdateWeaponsStatDisplayState(stateContainer.PlayerShipManager.PlayerShip)
                            )
                        );


                        break;
                    }
                default:
                    {
                        //???Not sure what the plan is from here
                        CurrentGlobalGameInterfaceState = null;
                        break;
                    }


            }

            
        }

        public void SetUIStateManagerContainer(UIStateManagerContainer container)
        {
            StateContainer = container;
        }

        public string GetCooldownText(float current, float max)
        {
            var cooldownMs = max - current;

            if (cooldownMs >= 1000)
            {
                return Math.Round(cooldownMs / 1000f, 2) + "s";
            }

            return Math.Round(cooldownMs) + "ms";
        }

        public StatBarGroupState UpdateShipStatDisplayState(Ship playerShip)
        {
            var shipStats = playerShip.ShipStats;

            var hullPercentage = playerShip.CurrentHealth / (float)shipStats.MaxHealth;
            var shieldPercentage = playerShip.Shields.CurrentShields / (float)shipStats.MaxShields;

            var currentEnergy = playerShip.GetCurrentEnergy();
            var energyPercentage = currentEnergy / (float)shipStats.Energy;

            var currentWeapons = new List<StatBarState>()
            {
                new StatBarState("Hull", "red", hullPercentage, playerShip.CurrentHealth.ToString()),
                new StatBarState("Shields", "blue", shieldPercentage, playerShip.Shields.CurrentShields.ToString()),
                new StatBarState("Energy", "orange", energyPercentage, currentEnergy.ToString())
            };

            return new StatBarGroupState("Ship Systems", currentWeapons);
        }

        public StatBarGroupState UpdateWeaponsStatDisplayState(Ship playerShip)
        {

            var currentWeapons = new List<StatBarState>()
            {
                GetWeaponDisplay(playerShip.GetWeapon(0), "red"),
                GetWeaponDisplay(playerShip.GetWeapon(1), "yellow")
            };
            
            return new StatBarGroupState("Weapons", currentWeapons);
        }

        public StatBarState GetWeaponDisplay(Weapon weapon, string color)
        {
            if (weapon == null)
            {
                return null;
            }

            var name = weapon.Stats.ProjectileType.ToString();

            var timeSinceLastShot = weapon.timeSinceLastShot;

            if (timeSinceLastShot == float.MaxValue)
            {
                timeSinceLastShot = 0;
            }

            if (timeSinceLastShot > weapon.Stats.FirePeriod)
            {
                timeSinceLastShot = weapon.Stats.FirePeriod;
            }

            var cooldownPercentage = timeSinceLastShot / weapon.Stats.FirePeriod;

            var cooldownText = GetCooldownText(timeSinceLastShot, weapon.Stats.FirePeriod);

            return new StatBarState(name, color, cooldownPercentage, cooldownText);
        }
    }
}
