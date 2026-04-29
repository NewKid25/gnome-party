<script setup lang="ts">
import ActionList from '../Subcomponents/ActionList.vue';
import PlayerStatus from '../Subcomponents/PlayerStatus.vue';
import { ActionListModel } from '../Models/ActionListModel';
import { ActionButtonModel } from '../Models/ActionButtonModel';
import { PlayerStatusModel } from '../Models/PlayerStatusModel';

const model = defineModel<{
	playerStatusModel: PlayerStatusModel,
	actionListModel: ActionListModel,
}>({required: true});

const emit = defineEmits<{
  actionChosen: [actionButton: ActionButtonModel]
}>();

function onActionChosen(actionButton: ActionButtonModel) {
	console.log("CombatActionMenu:", actionButton);
	emit("actionChosen", actionButton);
}
</script>

<template>
	<div class="combat-menu-panel combat-action-menu">
		<PlayerStatus v-model="model.playerStatusModel"></PlayerStatus>
		<div class="combat-actions-container">
			<h1 class="actions-header">ACTIONS</h1>
			<ActionList v-model="model.actionListModel" @action-chosen="onActionChosen"></ActionList>
		</div>
	</div>
</template>