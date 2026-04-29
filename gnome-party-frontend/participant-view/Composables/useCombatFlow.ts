import { ref } from "vue";

import { ActionButtonModel } from "../Models/ActionButtonModel";
import { TargetButtonModel } from "../Models/TargetButtonModel";
import { PlayerStatusModel } from "../Models/PlayerStatusModel";
import { useSocketData } from "../stores/socketData";
import { TargetListModel } from "../Models/TargetListModel";
import CharacterImage from "../Subcomponents/CharacterImage.vue";

export type CombatViewState = "actionMenu" | "targetMenu" | "waitingMenu" | "deadMenu";

export function useCombatFlow(playerStatusModel: PlayerStatusModel) {
    const currentView = ref<CombatViewState>("actionMenu");
    const chosenAction = ref<ActionButtonModel | null>(null);
    const chosenTarget = ref<TargetButtonModel | null>(null);
    const targetList = ref<TargetListModel>(new TargetListModel([]));
    const latestState = ref();

    const socketStore = useSocketData();

    function onActionChosen(action: ActionButtonModel) {
        chosenAction.value = action;

        if (action.targetRule == 3) {
            // No target
            sendActionToBackend(action, {selected: false, targetId: socketStore.localCharacterId, targetName: "0", characterImage: {source: "0", alt: "0"}, healthbar: {value: 0, maxValue: 0}});
            currentView.value = "waitingMenu";
            return;
        }
        
        if (populateTargetMenu(action)) {
            currentView.value = "targetMenu"
        }

    }

    function onTargetChosen(target: TargetButtonModel) {
        console.log("Chosen target:", target);

        if(!chosenAction.value) {
            console.error("No action chosen before target selection!");
            return;
        }

        chosenTarget.value = target;

        sendActionToBackend(chosenAction.value, target);
        currentView.value = "waitingMenu";
    }

    // TODO: include logic for populating target menu based on action
    function populateTargetMenu(action: ActionButtonModel) {
        console.log("Populating target menu for action:", action);
        
        // Enemy
        if (action.targetRule == 0) {
            const enemyList: TargetButtonModel[] = latestState.value.GameState.EnemyCharacters.map(
                (enemy: any) => ({
                    selected: false,
                    targetName: enemy.Name,
                    healthbar: {
                        value: enemy.Health,
                        maxValue: enemy.MaxHealth,
                    },
                    characterImage: {
                        source: "/img/Skeleton.svg",
                        alt: enemy.Name,
                    },
                    targetId: enemy.Id,
                })
            );

            if (enemyList.length > 0) {
                targetList.value.targets = enemyList;
                return true
            }
        }
        // Ally
        if (action.targetRule == 1) {
            const tList: TargetButtonModel[] = latestState.value.GameState.PlayerCharacters.filter(t => t.Id != socketStore.localCharacterId).map(
                (target: any) => ({
                    selected: false,
                    targetName: target.Name,
                    healthbar: {
                        value: target.Health,
                        maxValue: target.MaxHealth,
                    },
                    characterImage: {
                        source: "/img/GnomeFull.svg",
                        alt: target.Name,
                    },
                    targetId: target.Id,
                })
            );
            if (tList.length > 0) {
                targetList.value.targets = tList;
                return true
            }
        }
        // AllyOrSelf
        if (action.targetRule == 2) {
            const tList: TargetButtonModel[] = latestState.value.GameState.PlayerCharacters.map(
                (target: any) => ({
                    selected: false,
                    targetName: target.Name,
                    healthbar: {
                        value: target.Health,
                        maxValue: target.MaxHealth,
                    },
                    characterImage: {
                        source: "/img/GnomeFull.svg",
                        alt: target.Name,
                    },
                    targetId: target.Id,
                })
            );
            if (tList.length > 0) {
                targetList.value.targets = tList;
                return true
            }
        }
        // NoTargets
        if (action.targetRule == 3) {

        }

        return false

    }

    function sendActionToBackend(action: ActionButtonModel, target: TargetButtonModel) {
        console.log("Sending action and target to backend:", action, target);

        if(!target.targetId) {
            console.error("Target does not have a valid ID:", target);
            return;
        }

        // send action through shared socket
        socketStore.send({
            route: "player-action",
            EncounterId: socketStore.encounterId,
            TargetCharacterId: target.targetId, 
            SourceCharacterId: socketStore.localCharacterId, 
            Action: action.actionName, 
            GameSessionId: socketStore.gameSessionId,
        });

    }

    function onTurnUpdate(data: any) {
        if(data.playerHealth !== undefined) {
            playerStatusModel.healthBar.value = data.playerHealth;
        }

        if (data.playerMaxHealth !== undefined) {
            playerStatusModel.healthBar.maxValue = data.playerMaxHealth;
        }

        if(playerStatusModel.healthBar.value <= 0) {
            currentView.value = "deadMenu";
            return;
        }

        // reset selections for next turn
        chosenAction.value = null;
        chosenTarget.value = null;

        currentView.value = "actionMenu";
    }

    return {
        currentView,
        chosenAction,
        chosenTarget,
        onActionChosen,
        onTargetChosen,
        onTurnUpdate,
        targetList,
        latestState
     };
}