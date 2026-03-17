<script setup lang="ts">
import { ActionButtonModel } from '../Models/ActionButtonModel';

const model = defineModel<ActionButtonModel>({required: true});

const emit = defineEmits<{
  actionChosen: [actionButton: ActionButtonModel]
}>();

function onClick() {
	console.log("ActionButton clicked:", model.value.actionName);
	emit("actionChosen", model.value);
}
</script>

<template>
	<button type="button"class="action-button" :class="{ 'selected': model?.selected }" @click="onClick">{{ model?.actionName }}</button>
</template>