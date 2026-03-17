<script setup lang="ts">
import { TargetButtonModel } from '../Models/TargetButtonModel';
import { TargetListModel } from '../Models/TargetListModel';
import TargetButton from './TargetButton.vue';

const model = defineModel<TargetListModel>({required: true});

const emit = defineEmits<{
    targetChosen: [targetButton: TargetButtonModel]
}>();

function onTargetChosen(targetButton: TargetButtonModel) {
    model.value.targets.filter(v => v !== targetButton).forEach(v => (v.selected = false))

    emit("targetChosen", targetButton);
}

</script>
<template>
    <div class="target-list">
        <TargetButton v-for="(targetButton, i) in model.targets" :key="targetButton.targetName" v-model="model.targets[i]" @target-chosen="onTargetChosen"></TargetButton>
    </div>
</template>