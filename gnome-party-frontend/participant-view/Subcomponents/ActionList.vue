<script setup lang="ts">
import { ActionButtonModel } from '../Models/ActionButtonModel';
import { ActionListModel } from '../Models/ActionListModel';
import ActionButton from './ActionButton.vue';

const model = defineModel<ActionListModel>({required: true});

const emit = defineEmits<{
  actionChosen: [actionButton: ActionButtonModel]
}>();	

function onActionChosen(actionButton: ActionButtonModel) {
	model.value.actions.forEach((v) => {
		v.selected = (v === actionButton);
	});

	console.log("ActionList selected:", model.value.selected);
	emit("actionChosen", actionButton);
}
</script>

<template>
	<div class="action-list">
		<ActionButton v-for="(actionButton, i) in model.actions" :key="actionButton.actionName" v-model="model.actions[i]" @action-chosen="onActionChosen"></ActionButton>
	</div>
</template>