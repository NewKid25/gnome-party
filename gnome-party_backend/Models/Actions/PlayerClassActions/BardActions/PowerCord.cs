using System;
using System.Collections.Generic;
using System.Text;
using Models.ActionMetaData;
using Models.CharacterData;
using Models.CharacterData.PlayerCharacterClasses;
using Models.CombatData;
using Models.Status;
using static Models.CharacterData.PlayerCharacterClasses.Bard;

namespace Models.Actions.PlayerClassActions.BardActions
{
    public sealed class PowerCord : CharacterAction
    {
        public PowerCord() : base("Power Cord") // Call the base constructor with the name of the action
        {
            ActionDescription = new CharacterActionDescription("Power Cord", "Amplify your current song to all eligible targets"); // Set the action description
        }

        public override AttackResolution ResolveAttack(
            Character user, 
            Character target, 
            CombatEncounterGameState gameState, 
            bool isRedirected = false, 
            bool isUnblockable = false)
        {
            if (user == null) throw new ArgumentNullException(nameof(user)); // Validate that the user is not null
            if (target == null) throw new ArgumentNullException(nameof(target)); // Validate that the target is not null
            if (gameState == null) throw new ArgumentNullException(nameof(gameState)); // Validate that the gameState is not null

            if(user is not Bard bard) { throw new InvalidOperationException("Power Cord can only be used by a Bard"); } // Confirm if the user is a bard
            
            var currentSong = bard.GetCurrentSong(); // Get the bard's current song

            // Initialize variables to store the targets and action to use
            CharacterAction songAction;
            List<Character> powerCordTargets;

            // Choose which song to amplify based on the current bard's song
            switch(currentSong)
            {
                case BardSongs.Soothing:
                    songAction = new SoothingSong();
                    powerCordTargets = TargetingService.GetTargetsTeam(gameState, user.Id);
                    break;
                case BardSongs.Inspiring:
                    songAction = new InspiringSong();
                    powerCordTargets = TargetingService.GetTargetsTeam(gameState, user.Id);
                    break;
                case BardSongs.Frightening:
                    songAction= new FrighteningSong();
                    powerCordTargets = TargetingService.GetOpposingTeam(gameState, user.Id);
                    break;
                default:
                    throw new InvalidOperationException($"Unregistered bardic song: {currentSong}");
            }

            var resolution = new AttackResolution(); // Variable to hold the action's resolution
            var eligibleTargets = songAction.ReturnEligibleTargets(user, gameState); // Variable to hold the eligible targets

            // Apply the action of the bard's current song to each eligible target
            foreach(var currentTarget in powerCordTargets)
            {
                if (!eligibleTargets.Any(c => c.Id == currentTarget.Id)) { throw new ArgumentException("Target is nor eligible for this attack", nameof(target)); }
                var targetResolution = songAction.ResolveAttack(user, currentTarget, gameState);
                resolution.AttackInstances.AddRange(targetResolution.AttackInstances);
                resolution.HealInstances.AddRange(targetResolution.HealInstances);
                resolution.StatusEffectsToApply.AddRange(targetResolution.StatusEffectsToApply);
                resolution.Events.AddRange(targetResolution.Events);
            }

            // Stun the user for applying Power Cord
            resolution.StatusEffectsToApply.Add(new StunStatus(user));
            resolution.Events.Add(new CombatEvent("stun_status_applied", new StatusAppliedEventParams
            {
                OwnerId = user.Id
            }));

            return resolution;
        }
    }
}
