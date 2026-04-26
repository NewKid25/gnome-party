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

function onSocketMessage(event: MessageEvent) {
  const parsedJSON = JSON.parse(event.data);

  if (parsedJSON.Subject == "action-handler") {
    const latestState = (parsedJSON.Message as Array<any>).at(-1);

    const playerHealth =
      latestState.GameState.PlayerCharacters.find(
        (pc: any) => pc.Id == socketStore.localCharacterId
      )?.Health ?? 0;

    const enemyList: TargetButtonModel[] = [];
    for (const enemy of latestState.GameState.EnemyCharacters) {
      enemyList.push({
        selected: false,
        targetName: enemy.Name,
        healthbar: { value: enemy.Health, maxValue: enemy.MaxHealth },
        characterImage: { source: "/img/Skeleton.svg", alt: enemy.Name },
        targetId: enemy.Id
      });
    }
    props.combatTargetMenuModel.targetListModel.targets = enemyList;
    combatFlow.onTurnUpdate({ playerHealth });
  }
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
      v-else-if="combatFlow.currentView.value === 'targetMenu'" key="target-menu" :model-value="props.combatTargetMenuModel" @target-chosen="combatFlow.onTargetChosen"/>

    <CombatWaitingMenu
      v-else-if="combatFlow.currentView.value === 'waitingMenu'" key="waiting-menu" v-model="combatWaitingMenuModel"/>

    <CombatDeadMenu
      v-else-if="combatFlow.currentView.value === 'deadMenu'" key="dead-menu" v-model="combatDeadMenuModel"/>
  </Transition>
</template>