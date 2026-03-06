<script setup lang="ts">
import { reactive } from "vue";
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

const combatActionMenuModel = reactive({
  playerStatusModel,
  actionListModel,
});

function onActionChosen(action: ActionButtonModel) {
  console.log("ParticipantView:", action);
}

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

const combatTargetMenuModel = reactive({
  targetListModel,
});

function onTargetChosen(target: TargetButtonModel) {
  console.log("Chosen target:", target);
  console.log("Partiipant view", target);
}

</script>

<template>
  <CombatActionMenu v-model="combatActionMenuModel" @action-chosen="onActionChosen"></CombatActionMenu>
  <CombatTargetMenu v-model="combatTargetMenuModel" @target-chosen="onTargetChosen"></CombatTargetMenu>
</template>