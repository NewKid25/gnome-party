<script setup lang="ts">
import { onBeforeUnmount, reactive } from "vue";

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
import { useSocketData } from "./stores/socketData";

// Define models
const actionListModel = reactive(new ActionListModel([ ]));
const healthBarModel: HealthBarModel = reactive({ value: 100, maxValue: 100 });
const characterImageModel: CharacterImageModel = { source: "/img/GnomeFull.svg", alt: "placeholder for player image" };
const targetListModel = reactive(new TargetListModel([ ]));

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

const SOCKET_URL = "wss://ws.gnome-party.com";

const encounterData = useEncounterData();
const socketStore = useSocketData();

// reuse existing socket or connect if needed
const socket = socketStore.socket ?? socketStore.connect(SOCKET_URL);

// handle combat state logic
const combatFlow = useCombatFlow(playerStatusModel);

// Listen for combat-related backend messages
function onSocketMessage(event: MessageEvent) {
  console.log("Message from server ", event.data);

  const parsedJSON = JSON.parse(event.data);

  if (parsedJSON.Subject == "join-game-connection") {
    encounterData.localPlayerId = parsedJSON.Message.UserId;
    socketStore.localPlayerId = parsedJSON.Message.UserId;
  }
  if (parsedJSON.Subject == "join-game-session") {
    encounterData.gameSessionId = parsedJSON.Message.GameSessionId;
    socketStore.gameSessionId = parsedJSON.Message.GameSessionId;
  }

  if (parsedJSON.Subject == "begin-combat-encounter") {
    encounterData.encounterId = parsedJSON.Message.EncounterId;
    socketStore.encounterId = parsedJSON.Message.EncounterId;

    // Load player data
    const player:any = parsedJSON.Message.GameState.PlayerCharacters.find((pc:any) => {return pc.Id == encounterData.localPlayerId});
    
    if(!player) {
      console.error("Player data not found in game state.");
      return;
    }

    // Load valid actions
    const actionButtonList:ActionButtonModel[] = [];
    for (const action of player.ActionsDescriptions)
    {
      console.log("Action:", action.Name);
      actionButtonList.push({selected: false, actionName: action.Name});
    }
    actionListModel.actions = actionButtonList;

    // Load health
    healthBarModel.maxValue = player.MaxHealth;
    healthBarModel.value = player.Health;

    // Load enemies
    const enemyList:TargetButtonModel[] = [];
    for (const enemy of parsedJSON.Message.GameState.EnemyCharacters)
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
    const playerHealth = (parsedJSON.Message as Array<any>).at(-1).GameState.PlayerCharacters.find((pc:any) => {return pc.Id == encounterData.localPlayerId})?.Health ?? 0;
    
    // Load enemies
    const enemyList:TargetButtonModel[] = [];
    for (const enemy of parsedJSON.Message.at(-1).GameState.EnemyCharacters)
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

    combatFlow.onTurnUpdate({playerHealth: playerHealth});
    
  }
}

// prevent multiple listeners
socket.removeEventListener("message", onSocketMessage);
socket.addEventListener("message", onSocketMessage);

// clean up listener on unmount but keep socket open
onBeforeUnmount(() => {
    socket.removeEventListener("message", onSocketMessage);
});

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