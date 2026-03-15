<script setup lang="ts">
import { reactive, ref } from "vue";
import CombatActionMenu from "./Menus/CombatActionMenu.vue";
import CombatTargetMenu from "./Menus/CombatTargetMenu.vue";

import { ActionButtonModel } from "./Models/ActionButtonModel";
import { ActionListModel } from "./Models/ActionListModel";
import { PlayerStatusModel } from "./Models/PlayerStatusModel";
import { HealthBarModel } from "./Models/HealthBarModel";
import { CharacterImageModel } from "./Models/CharacterImageModel";
import { TargetButtonModel } from "./Models/TargetButtonModel";
import { TargetListModel } from "./Models/TargetListModel";

import "./styles.css";

type ViewState = "actionMenu" | "targetMenu" | "waitingMenu" | "deadMenu";

const currentView = ref<ViewState>("actionMenu");

const chosenAction = ref<ActionButtonModel | null>(null);

// CombatActionMenu logic and test data
const actionListModel = new ActionListModel([
  { selected: false, actionName: "Slash" } as ActionButtonModel,
  { selected: false, actionName: "Block" } as ActionButtonModel,
  { selected: false, actionName: "Parry" } as ActionButtonModel,
  { selected: false, actionName: "Whirling Strike" } as ActionButtonModel,
]);

const healthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
const characterImageModel: CharacterImageModel = { source: "../placeholder_player_image.png", alt: "placeholder for player image" };
const playerStatusModel = new PlayerStatusModel(
  characterImageModel,
  healthBarModel
);
// End of CombatActionMenu logic and test data

const combatActionMenuModel = reactive({
  playerStatusModel,
  actionListModel,
});

function onActionChosen(action: ActionButtonModel) {
  console.log("ParticipantView:", action);

  chosenAction.value = action;

  // TODO: Connect to backend
  populateTargetMenu(action);

  currentView.value = "targetMenu";
}

function populateTargetMenu(action: ActionButtonModel) {
  console.log("Populating target menu for action:", action);
  // TODO: Replace with backend data
}

// CombatTargetMenu logic and test data
const targetAHealthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
const targetBHealthBarModel: HealthBarModel = { value: 50, maxValue: 100 };
const targetCHealthBarModel: HealthBarModel = { value: 80, maxValue: 100 };

const targetACharacterImageModel: CharacterImageModel = { source: "../placeholder_target_image.png", alt: "placeholder for target A image" };
const targetBCharacterImageModel: CharacterImageModel = { source: "../placeholder_target_image.png", alt: "placeholder for target B image" };
const targetCCharacterImageModel: CharacterImageModel = { source: "../placeholder_target_image.png", alt: "placeholder for target C image" };

const targetListModel = new TargetListModel([
  { selected: false, targetName: "Skeleton A", healthbar: targetAHealthBarModel, characterImage: targetACharacterImageModel } as TargetButtonModel,
  { selected: false, targetName: "Skeleton B", healthbar: targetBHealthBarModel, characterImage: targetBCharacterImageModel } as TargetButtonModel,
  { selected: false, targetName: "Skeleton C", healthbar: targetCHealthBarModel, characterImage: targetCCharacterImageModel } as TargetButtonModel,
]);
// End of CombatTargetMenu logic and test data

const combatTargetMenuModel = reactive({
  targetListModel,
});

function onTargetChosen(target: TargetButtonModel) {
  console.log("Chosen target:", target);

  if(!chosenAction.value) {
    console.error("No action chosen before target selection!");
    return;
  }

  sendActionToBackend(chosenAction.value, target);
  currentView.value = "waitingMenu";
}

function sendActionToBackend(action: ActionButtonModel, target: TargetButtonModel) {
  console.log("Sending action and target to backend:", action, target);
}

function checkIfDead() {
  if(playerStatusModel.healthBar.value <= 0) {
    currentView.value = "deadMenu";
  }
}

</script>

<template>
<div class="participant-view">
    <div class="participant-container">
      <CombatActionMenu v-if="currentView === 'actionMenu'" v-model="combatActionMenuModel" @action-chosen="onActionChosen"></CombatActionMenu>
      <CombatTargetMenu v-else-if="currentView === 'targetMenu'" v-model="combatTargetMenuModel" @target-chosen="onTargetChosen"></CombatTargetMenu>
      <!-- <CombatWaitingMenu v-else-if="currentView === 'waitingMenu'" v-model="combatWaitingMenuModel"></CombatWaitingMenu>
      <CombatDeadMenu v-else-if="currentView === 'deadMenu'" v-model="combatDeadMenuModel"></CombatDeadMenu> -->
    </div>
</div>
</template>