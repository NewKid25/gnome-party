import { ref } from "vue";

import { ActionButtonModel } from "../Models/ActionButtonModel";
import { TargetButtonModel } from "../Models/TargetButtonModel";
import { PlayerStatusModel } from "../Models/PlayerStatusModel";
import { useEncounterData } from "../stores/encounterData";

export type CombatViewState = "actionMenu" | "targetMenu" | "waitingMenu" | "deadMenu";

export function useCombatFlow(playerStatusModel: PlayerStatusModel, socket:WebSocket) {
    const currentView = ref<CombatViewState>("actionMenu");
    const chosenAction = ref<ActionButtonModel | null>(null);
    const chosenTarget = ref<TargetButtonModel | null>(null);

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

        sendActionToBackend(chosenAction.value, target);
        currentView.value = "waitingMenu";
    }

    // Connect to backend
    function populateTargetMenu(action: ActionButtonModel) {
        console.log("Populating target menu for action:", action);
    }

    function sendActionToBackend(action: ActionButtonModel, target: TargetButtonModel) {
        console.log("Sending action and target to backend:", action, target);

        const encounterData = useEncounterData();

        socket.send(JSON.stringify({
            route: "player-action",
            EncounterId:encounterData.encounterId,
            TargetCharacterId:target.targetId, 
            SourceCharacterId:encounterData.localPlayerId, 
            Action:action.actionName, 
            GameSessionId:encounterData.gameSessionId,
        }));

    }

    function onTurnUpdate(data: any) {
        if(data.playerHealth !== undefined) {
            playerStatusModel.healthBar.value = data.playerHealth;
        }

        if(playerStatusModel.healthBar.value <= 0) {
            currentView.value = "deadMenu";
            return;
        }

        currentView.value = "actionMenu";
    }
    // End of connecting to backend

    return {
        currentView,
        chosenAction,
        chosenTarget,
        onActionChosen,
        onTargetChosen,
        onTurnUpdate,
     };
}