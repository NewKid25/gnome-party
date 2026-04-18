<script setup lang="ts">
import { onBeforeUnmount, reactive, ref } from "vue";
import ParticipantLobbyFlow from "./components/ParticipantLobbyFlow.vue";
import ParticipantCombatFlow from "./components/ParticipantCombatFlow.vue";

import "./styles.css";
import { useSocketData } from "./stores/socketData";

import { ActionButtonModel } from "./Models/ActionButtonModel";
import { ActionListModel } from "./Models/ActionListModel";
import { CharacterImageModel } from "./Models/CharacterImageModel";
import { HealthBarModel } from "./Models/HealthBarModel";
import { PlayerStatusModel } from "./Models/PlayerStatusModel";
import { TargetButtonModel } from "./Models/TargetButtonModel";
import { TargetListModel } from "./Models/TargetListModel";

type ParticipantPhase = "lobby" | "combat";

const participantPhase = ref<ParticipantPhase>("lobby");

const SOCKET_URL = "wss://ws.gnome-party.com";

const socketStore = useSocketData();
const socket = socketStore.socket ?? socketStore.connect(SOCKET_URL);

const actionListModel = reactive(new ActionListModel([]));
const healthBarModel: HealthBarModel = reactive({ value: 100, maxValue: 100 });
const characterImageModel: CharacterImageModel = {
  source: "/img/GnomeFull.svg",
  alt: "placeholder for player image"
};
const targetListModel = reactive(new TargetListModel([]));

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

function onSocketMessage(event: MessageEvent) {
  const parsedJSON = JSON.parse(event.data);

  if (parsedJSON.Subject === "join-game-connection") {
    socketStore.localPlayerId = parsedJSON.Message.UserId;
  }

  if (parsedJSON.Subject === "join-game-session") {
    socketStore.gameSessionId = parsedJSON.Message.GameSessionId;
  }

  if (parsedJSON.Subject === "start-campaign") {
    participantPhase.value = "combat";
  }
  
  if (parsedJSON.Subject === "begin-combat-encounter") {
    socketStore.encounterId = parsedJSON.Message.EncounterId;

    const player: any = parsedJSON.Message.GameState.PlayerCharacters.find(
      (pc: any) => pc.Id === socketStore.localCharacterId
    );

    if (!player) {
      console.error("Player data not found in game state.");
      console.log("localCharacterId:", socketStore.localCharacterId);
      console.log("PlayerCharacters:", parsedJSON.Message.GameState.PlayerCharacters);
      return;
    }

    // Populate available actions for the local player
    const actionButtonList: ActionButtonModel[] = [];
    for (const action of player.ActionsDescriptions) {
      actionButtonList.push({
        selected: false,
        actionName: action.Name
      });
    }
    actionListModel.actions = actionButtonList;

    // Populate player health
    healthBarModel.maxValue = player.MaxHealth;
    healthBarModel.value = player.Health;

    // Populate enemies
    const enemyList: TargetButtonModel[] = [];
    for (const enemy of parsedJSON.Message.GameState.EnemyCharacters) {
      enemyList.push({
        selected: false,
        targetName: enemy.Name,
        healthbar: { value: enemy.Health, maxValue: enemy.MaxHealth },
        characterImage: { source: "/img/Skeleton.svg", alt: enemy.Name },
        targetId: enemy.Id
      });
    }
    targetListModel.targets = enemyList;
  }
}

socket.removeEventListener("message", onSocketMessage);
socket.addEventListener("message", onSocketMessage);

onBeforeUnmount(() => {
  socket.removeEventListener("message", onSocketMessage);
});
</script>

<template>
  <div class="participant-view">
    <div class="participant-container">
      <ParticipantLobbyFlow
        v-if="participantPhase === 'lobby'"
      />

      <ParticipantCombatFlow
        v-else-if="participantPhase === 'combat'"
        :combat-action-menu-model="combatActionMenuModel"
        :combat-target-menu-model="combatTargetMenuModel"
      />
    </div>
  </div>
</template>