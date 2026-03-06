<script setup lang="ts">
import { reactive } from "vue";
import CombatActionMenu from "./Menus/CombatActionMenu.vue";

import { ActionButtonModel } from "./Models/ActionButtonModel";
import { ActionListModel } from "./Models/ActionListModel";
import { PlayerStatusModel } from "./Models/PlayerStatusModel";
import { HealthBarModel } from "./Models/HealthBarModel";
import { CharacterImageModel } from "./Models/CharacterImageModel";

import "./styles.css";

const actionListModel = new ActionListModel([
  { selected: false, actionName: "Slash" } as ActionButtonModel,
  { selected: false, actionName: "Block" } as ActionButtonModel,
  { selected: false, actionName: "Parry" } as ActionButtonModel,
  { selected: false, actionName: "Whirling Strike" } as ActionButtonModel,
]);
// CombatActionMenu logic and test data
// const actionListModel = new ActionListModel([
//   { selected: false, actionName: "Slash" } as ActionButtonModel,
//   { selected: false, actionName: "Block" } as ActionButtonModel,
//   { selected: false, actionName: "Parry" } as ActionButtonModel,
//   { selected: false, actionName: "Whirling Strike" } as ActionButtonModel,
// ]);

// const healthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
// const characterImageModel: CharacterImageModel = { source: "../placeholder_player_image.png", alt: "placeholder for player image" };
// const playerStatusModel = new PlayerStatusModel(
//   characterImageModel,
//   healthBarModel
// );

// const combatActionMenuModel = reactive({
//   playerStatusModel,
//   actionListModel,
// });

// function onActionChosen(action: ActionButtonModel) {
//   console.log("ParticipantView:", action);
// }

const healthBarModel: HealthBarModel = { value: 30, maxValue: 100 };
const playerImageModel: PlayerImageModel = { source: "../placeholder_player_image.png", alt: "placeholder for player image" };
const playerStatusModel = new PlayerStatusModel(
  playerImageModel,
  healthBarModel
);

const combatActionMenuModel = reactive({
  playerStatusModel,
  actionListModel,
});

function onActionChosen(action: ActionButtonModel) {
  console.log("App.vue:", action);
}


</script>

<template>
  <CombatActionMenu v-model="combatActionMenuModel" @action-chosen="onActionChosen"></CombatActionMenu>
</template>