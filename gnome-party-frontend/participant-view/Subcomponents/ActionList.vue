<script setup lang="ts">
import { ActionButtonModel } from '../Models/ActionButtonModel';
import { ActionListModel } from '../Models/ActionListModel';
import ActionButton from './ActionButton.vue';

const model = defineModel<ActionListModel>({required: true});

const emit = defineEmits<{
  actionChosen: [actionButton:ActionButtonModel]
}>();	

function onActionChosen(actionButton:ActionButtonModel) {
	console.log("ActionList:", actionButton);

	model.value.actions.filter((v) => v != actionButton).forEach((v) => {
		v.selected = false;
	});

	emit("actionChosen", actionButton);
}
</script>

<template>
	<h1>ACTIONS</h1>
	<ActionButton v-for="(actionButton, i) in model.actions" :key="actionButton.actionName" v-model="model.actions[i]" @action-chosen="onActionChosen"></ActionButton>
</template>