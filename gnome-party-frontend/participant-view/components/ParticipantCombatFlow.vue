<script setup lang="ts">
import { onBeforeUnmount, reactive } from "vue";

import CombatActionMenu from "../Menus/CombatActionMenu.vue";
import CombatWaitingMenu from "../Menus/CombatWaitingMenu.vue";
import CombatDeadMenu from "../Menus/CombatDeadMenu.vue";
import CombatTargetMenu from "../Menus/CombatTargetMenu.vue";

import { ActionListModel } from "../Models/ActionListModel";
import { MessageMenuModel } from "../Models/MessageMenuModel";
import { PlayerStatusModel } from "../Models/PlayerStatusModel";
import { TargetButtonModel } from "../Models/TargetButtonModel";
import { TargetListModel } from "../Models/TargetListModel";

import { useCombatFlow } from "../Composables/useCombatFlow";
import { useSocketData } from "../stores/socketData";
import { getEnemyImage } from "../Composables/getEnemyImage";

const props = defineProps<{
  combatActionMenuModel: {
    playerStatusModel: PlayerStatusModel;
    actionListModel: ActionListModel;
  };
  combatTargetMenuModel: {
    targetListModel: TargetListModel;
  };
}>();

const combatWaitingMenuModel: MessageMenuModel = reactive({
  title: "Waiting for Turn",
  message: "Please wait while the other players choose their actions.",
});

const combatDeadMenuModel: MessageMenuModel = reactive({
  title: "You Died!",
  message: "You were defeated by Skeleton A. Better luck next time!",
});

const SOCKET_URL = "wss://ws.gnome-party.com";

const socketStore = useSocketData();

// Reuse existing socket or connect if needed
const socket = socketStore.socket ?? socketStore.connect(SOCKET_URL);

const combatFlow = useCombatFlow(props.combatActionMenuModel.playerStatusModel);

enum TargetRule {
  Enemy,
  Ally,
  AllyOrSelf,
  NoTargets
}

function getLatestStateFromActionHandler(message: unknown) {
  if (!Array.isArray(message) || message.length === 0) {
    return null;
  }

  for (let i = message.length - 1; i >= 0; i--) {
    const step = message[i];

    const localPlayer = step.GameState?.PlayerCharacters?.find(
      (pc: any) => pc.Id === socketStore.localCharacterId
    );

    if (localPlayer) {
      return step;
    }
  }

  return null;
}

function updateLocalPlayerHealth(latestState: any) {
  const localPlayer = latestState.GameState.PlayerCharacters.find(
    (pc: any) => pc.Id === socketStore.localCharacterId
  );

  if (!localPlayer) {
    console.error("Local character not found in latest game state.");
    console.log("localCharacterId:", socketStore.localCharacterId);
    console.log("PlayerCharacters:", latestState.GameState.PlayerCharacters);
    return null;
  }

  props.combatActionMenuModel.playerStatusModel.healthBar.value = localPlayer.Health;
  props.combatActionMenuModel.playerStatusModel.healthBar.maxValue = localPlayer.MaxHealth;

  return localPlayer;
}

function updateActionTargets(latestState: any) {
  const enemyList: TargetButtonModel[] = latestState.GameState.EnemyCharacters.map(
    (enemy: any) => ({
      selected: false,
      targetName: enemy.Name,
      healthbar: {
        value: enemy.Health,
        maxValue: enemy.MaxHealth,
      },
      characterImage: {
        source: getEnemyImage(enemy.Name),
        alt: enemy.Name,
      },
      targetId: enemy.Id,
    })
  );

  props.combatTargetMenuModel.targetListModel.targets = enemyList;
}

function onSocketMessage(event: MessageEvent) {
  const parsedJSON = JSON.parse(event.data);
  console.log("ParticipantView message:", parsedJSON.Subject);

  if (parsedJSON.Subject === "begin-combat-encounter") {
    console.log("Updating game state:", parsedJSON.Message);
    combatFlow.latestState.value = parsedJSON.Message;
  }
  if (parsedJSON.Subject !== "action-handler") {
    return;
  }

  const latestState = getLatestStateFromActionHandler(parsedJSON.Message);

  if (!latestState?.GameState) {
    console.error("No action-handler step contained the local player:", parsedJSON);
    return;
  }

  console.log("Updating game state:", latestState);
  combatFlow.latestState.value = latestState;

  const localPlayer = updateLocalPlayerHealth(latestState);
  if (!localPlayer) return;

  updateActionTargets(latestState);

  console.log(
    "Participant health updated:",
    localPlayer.Health,
    "/",
    localPlayer.MaxHealth
  );

  combatFlow.onTurnUpdate({
    playerHealth: localPlayer.Health,
    playerMaxHealth: localPlayer.MaxHealth,
  });
}

socket.removeEventListener("message", onSocketMessage);
socket.addEventListener("message", onSocketMessage);

onBeforeUnmount(() => {
  socket.removeEventListener("message", onSocketMessage);
});
</script>

<template>
  <Transition name="combat-menu" mode="out-in">
    <CombatActionMenu
      v-if="combatFlow.currentView.value === 'actionMenu'" key="action-menu" :model-value="props.combatActionMenuModel" @action-chosen="combatFlow.onActionChosen"/>

    <CombatTargetMenu 
      v-else-if="combatFlow.currentView.value === 'targetMenu'" key="target-menu" :model-value="{targetListModel: combatFlow.targetList.value}" @target-chosen="combatFlow.onTargetChosen"/>

    <CombatWaitingMenu
      v-else-if="combatFlow.currentView.value === 'waitingMenu'" key="waiting-menu" v-model="combatWaitingMenuModel"/>

    <CombatDeadMenu
      v-else-if="combatFlow.currentView.value === 'deadMenu'" key="dead-menu" v-model="combatDeadMenuModel"/>
  </Transition>
</template>