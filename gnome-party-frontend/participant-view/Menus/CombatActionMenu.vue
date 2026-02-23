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
  actionChosen: [actionButton:ActionButtonModel]
}>();

function onActionChosen(actionButton:ActionButtonModel) {
	console.log("CombatActionMenu:", actionButton);
	emit("actionChosen", actionButton);
}
</script>
<template>
<PlayerStatus v-model="model.playerStatusModel"></PlayerStatus>
<ActionList v-model="model.actionListModel" @action-chosen="onActionChosen"></ActionList>	
</template>