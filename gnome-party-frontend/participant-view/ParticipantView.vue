<script setup lang="ts">
import { reactive } from "vue";

import CombatActionMenu from "./Menus/CombatActionMenu.vue";
import CombatWaitingMenu from "./Menus/CombatWaitingMenu.vue";
import CombatDeadMenu from "./Menus/CombatDeadMenu.vue";
import CombatTargetMenu from "./Menus/CombatTargetMenu.vue";

import { ActionButtonModel } from "./Models/ActionButtonModel";
import { ActionListModel } from "./Models/ActionListModel";
import { CharacterImageModel } from "./Models/CharacterImageModel";
import { HealthBarModel } from "./Models/HealthBarModel";
import { MessageMenuModel } from "./Models/MessageMenuModel";
import { PlayerStatusModel } from "./Models/PlayerStatusModel";
import { TargetButtonModel } from "./Models/TargetButtonModel";
import { TargetListModel } from "./Models/TargetListModel";

import { useCombatFlow } from "./Composables/useCombatFlow";

import "./styles.css";
import { useEncounterData } from "./stores/encounterData";


// Test data
const actionListModel = reactive(new ActionListModel([
  // { selected: false, actionName: "Slash" } as ActionButtonModel,
  // { selected: false, actionName: "Block" } as ActionButtonModel,
  // { selected: false, actionName: "Parry" } as ActionButtonModel,
  // { selected: false, actionName: "Whirling Strike" } as ActionButtonModel,
]));

const healthBarModel: HealthBarModel = reactive({ value: 30, maxValue: 100 });
const characterImageModel: CharacterImageModel = { source: "/img/GnomeFull.svg", alt: "placeholder for player image" };

const targetAHealthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
const targetBHealthBarModel: HealthBarModel = { value: 50, maxValue: 100 };
const targetCHealthBarModel: HealthBarModel = { value: 80, maxValue: 100 };

const targetACharacterImageModel: CharacterImageModel = { source: "/img/placeholder_target_image.png", alt: "placeholder for target A image" };
const targetBCharacterImageModel: CharacterImageModel = { source: "../placeholder_target_image.png", alt: "placeholder for target B image" };
const targetCCharacterImageModel: CharacterImageModel = { source: "../placeholder_target_image.png", alt: "placeholder for target C image" };

const targetListModel = reactive(new TargetListModel([
  // { selected: false, targetName: "Skeleton A", healthbar: targetAHealthBarModel, characterImage: targetACharacterImageModel } as TargetButtonModel,
  // { selected: false, targetName: "Skeleton B", healthbar: targetBHealthBarModel, characterImage: targetBCharacterImageModel } as TargetButtonModel,
  // { selected: false, targetName: "Skeleton C", healthbar: targetCHealthBarModel, characterImage: targetCCharacterImageModel } as TargetButtonModel,
]));
// End of test data

// Define models
const playerStatusModel = new PlayerStatusModel(
  characterImageModel,
  healthBarModel
);

const combatActionMenuModel = reactive({
  playerStatusModel,
  actionListModel,
});

const combatTargetMenuModel = reactive({
  targetListModel,
});

const combatWaitingMenuModel: MessageMenuModel = reactive({
  title: "Waiting for Turn",
  message: "Please wait while the other players choose their actions.",
});

const combatDeadMenuModel: MessageMenuModel = reactive({
  title: "You Died!",
  message: "You were defeated by Skeleton A!",
});


const encounterData = useEncounterData();


const socket = new WebSocket("wss://ws.gnome-party.com");
const combatFlow = useCombatFlow(playerStatusModel, socket);


// Listen for messages
socket.addEventListener("message", (event) => {
  console.log("Message from server ", event.data);

  let parsedJSON = JSON.parse(event.data);

  if (parsedJSON.Subject == "join-game-connection") {
    encounterData.localPlayerId = parsedJSON.Message.UserId;
  }
  if (parsedJSON.Subject == "join-game-session") {
    encounterData.gameSessionId = parsedJSON.Message.GameSessionId;
  }

  if (parsedJSON.Subject == "begin-combat-encounter") {
    encounterData.encounterId = parsedJSON.Message.EncounterId;

    let player:any = parsedJSON.Message.GameState.PlayerCharacters.find((pc:any) => {return pc.Id == encounterData.localPlayerId});
    
    // Load valid actions
    let actionButtonList:ActionButtonModel[] = [];
    for (let action of player.ActionsDescriptions)
    {
      console.log("Action:", action.Name);
      actionButtonList.push({selected: false, actionName: action.Name});
    }
    actionListModel.actions = actionButtonList;

    // Load health
    healthBarModel.maxValue = player.MaxHealth;
    healthBarModel.value = player.Health;

    
    // Load enemies
    let enemyList:TargetButtonModel[] = [];
    for (let enemy of parsedJSON.Message.GameState.EnemyCharacters)
    {
      enemyList.push({
        selected: false,
        targetName: enemy.Name,
        healthbar: {value: enemy.Health, maxValue: enemy.MaxHealth},
        characterImage: { source: "/img/Skeleton.svg", alt: enemy.Name },
        targetId: enemy.Id
      });
    }
    targetListModel.targets = enemyList;

    
  }

  if (parsedJSON.Subject == "action-handler")
  {
    let playerHealth = (parsedJSON.Message as Array<any>).at(-1).GameState.PlayerCharacters.find((pc:any) => {return pc.Id == encounterData.localPlayerId})?.Health ?? 0;
    
    // Load enemies
    let enemyList:TargetButtonModel[] = [];
    for (let enemy of parsedJSON.Message.at(-1).GameState.EnemyCharacters)
    {
      enemyList.push({
        selected: false,
        targetName: enemy.Name,
        healthbar: {value: enemy.Health, maxValue: enemy.MaxHealth},
        characterImage: { source: "/img/Skeleton.svg", alt: enemy.Name },
        targetId: enemy.Id
      });
    }
    targetListModel.targets = enemyList;

    combatFlow.onTurnUpdate({playerHealth: playerHealth})
    
  }
});

socket.onopen = (ev:Event) => {
  socket.send(JSON.stringify({"route": "join-game"}))
}

window.addEventListener("beforeunload", () => {
  socket.close();
})

</script>

<template>
<div class="participant-view">
    <div class="participant-container">
      <Transition name="combat-menu" mode="out-in">
        <CombatActionMenu v-if="combatFlow.currentView.value === 'actionMenu'" key="action-menu" v-model="combatActionMenuModel" @action-chosen="combatFlow.onActionChosen"></CombatActionMenu>
        <CombatTargetMenu v-else-if="combatFlow.currentView.value === 'targetMenu'" key="target-menu" v-model="combatTargetMenuModel" @target-chosen="combatFlow.onTargetChosen"></CombatTargetMenu>
        <CombatWaitingMenu v-else-if="combatFlow.currentView.value === 'waitingMenu'" key="waiting-menu" v-model="combatWaitingMenuModel"></CombatWaitingMenu>
        <CombatDeadMenu v-else-if="combatFlow.currentView.value === 'deadMenu'" key="dead-menu" v-model="combatDeadMenuModel"></CombatDeadMenu>
      </Transition>
    </div>
</div>
</template>