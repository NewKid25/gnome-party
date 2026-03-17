<script setup lang="ts">
import TargetList from '../Subcomponents/TargetList.vue';
import { TargetListModel } from '../Models/TargetListModel';
import { TargetButtonModel } from '../Models/TargetButtonModel';

const model = defineModel<{targetListModel: TargetListModel}>({required: true});

const emit = defineEmits<{
    targetChosen: [targetButton: TargetButtonModel]
}>();

function onTargetChosen(targetButton: TargetButtonModel) {
    console.log("CombatTargetMenu:", targetButton);
    emit("targetChosen", targetButton);
}

</script>

<template>
    <div class="combat-menu-panel">
        <h1 class="target-menu-title">Pick Target</h1>
        <hr></hr>
        <TargetList v-model="model.targetListModel" @target-chosen="onTargetChosen"></TargetList>
    </div>
</template>
