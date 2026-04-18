import { ref } from "vue";

import { ActionButtonModel } from "../Models/ActionButtonModel";
import { TargetButtonModel } from "../Models/TargetButtonModel";
import { PlayerStatusModel } from "../Models/PlayerStatusModel";
import { useSocketData } from "../stores/socketData";

export type CombatViewState = "actionMenu" | "targetMenu" | "waitingMenu" | "deadMenu";

export function useCombatFlow(playerStatusModel: PlayerStatusModel) {
    const currentView = ref<CombatViewState>("actionMenu");
    const chosenAction = ref<ActionButtonModel | null>(null);
    const chosenTarget = ref<TargetButtonModel | null>(null);

    const socketStore = useSocketData();

    function onActionChosen(action: ActionButtonModel) {
        chosenAction.value = action;

        populateTargetMenu(action);

        currentView.value = "targetMenu";
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
     };
}