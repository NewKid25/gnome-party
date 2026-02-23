<script setup lang="ts">
import { TargetButtonModel } from '../Models/TargetButtonModel';

const model = defineModel<TargetButtonModel>({required: true});

const emit = defineEmits<{
    targetChosen: [targetButton:TargetButtonModel]
}>();

function onClick() {
    model.value.selected = true;
    emit("targetChosen", model.value);
}
</script>

<template>
    <button @click="onClick">{{ model?.targetName }}</button>
</template>