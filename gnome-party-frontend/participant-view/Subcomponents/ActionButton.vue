<script setup lang="ts">
import { ActionButtonModel } from '../Models/ActionButtonModel';

const model = defineModel<ActionButtonModel>({required: true});

const emit = defineEmits<{
  actionChosen: [actionButton:ActionButtonModel]
}>();

function onClick() {
	model.value.selected = true;
	emit("actionChosen", model.value);
}
</script>

<template>
	<button @click="onClick">{{ model?.actionName }}</button>
</template>