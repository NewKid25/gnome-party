<script setup lang="ts">
import { ActionButtonModel } from '../Models/ActionButtonModel';
import { ActionListModel } from '../Models/ActionListModel';
import ActionButton from './ActionButton.vue';

const model = defineModel<ActionListModel>({required: true});

function onActionChosen(actionButton:ActionButtonModel) {
	console.log(actionButton);
}
</script>
<template>
	<ActionButton v-for="(actionButton, i) in model.actions" v-model="model.actions[i]" @action-chosen="onActionChosen"></ActionButton>
</template>