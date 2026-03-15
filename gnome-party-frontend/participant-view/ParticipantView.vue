<script setup lang="ts">
import { reactive, ref } from "vue";

import CombatActionMenu from "./Menus/CombatActionMenu.vue";
import CombatTargetMenu from "./Menus/CombatTargetMenu.vue";

import { ActionButtonModel } from "./Models/ActionButtonModel";
import { ActionListModel } from "./Models/ActionListModel";
import { CharacterImageModel } from "./Models/CharacterImageModel";
import { HealthBarModel } from "./Models/HealthBarModel";
import { PlayerStatusModel } from "./Models/PlayerStatusModel";
import { TargetButtonModel } from "./Models/TargetButtonModel";
import { TargetListModel } from "./Models/TargetListModel";

import { useCombatFlow } from "./Composables/useCombatFlow";

import "./styles.css";

// Test data
const actionListModel = new ActionListModel([
  { selected: false, actionName: "Slash" } as ActionButtonModel,
  { selected: false, actionName: "Block" } as ActionButtonModel,
  { selected: false, actionName: "Parry" } as ActionButtonModel,
  { selected: false, actionName: "Whirling Strike" } as ActionButtonModel,
]);

const healthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
const characterImageModel: CharacterImageModel = { source: "../placeholder_player_image.png", alt: "placeholder for player image" };

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



const combatFlow = useCombatFlow(playerStatusModel);

</script>

<template>
<div class="participant-view">
    <div class="participant-container">
      <!-- <CombatWaitingMenu v-else-if="currentView === 'waitingMenu'" v-model="combatWaitingMenuModel"></CombatWaitingMenu>
      <CombatDeadMenu v-else-if="currentView === 'deadMenu'" v-model="combatDeadMenuModel"></CombatDeadMenu> -->
      <CombatActionMenu v-if="combatFlow.currentView.value === 'actionMenu'" v-model="combatActionMenuModel" @action-chosen="combatFlow.onActionChosen"></CombatActionMenu>
      <CombatTargetMenu v-else-if="combatFlow.currentView.value === 'targetMenu'" v-model="combatTargetMenuModel" @target-chosen="combatFlow.onTargetChosen"></CombatTargetMenu>
    </div>
</div>
</template>