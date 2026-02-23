<script setup lang="ts">
import { TargetButtonModel } from '../Models/TargetButtonModel';
import { TargetListModel } from '../Models/TargetListModel';
import TargetButton from './TargetButton.vue';

const model = defineModel<TargetListModel>({required: true});

function onTargetChosen(targetButton:TargetButtonModel) {
    console.log(targetButton);
}
</script>
<template>
    <TargetButton v-for="(targetButton, i) in model.targets" v-model="model.targets[i]" @target-chosen="onTargetChosen"></TargetButton>
</template>