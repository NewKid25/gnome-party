<script setup lang="ts">
import { TargetButtonModel } from '../Models/TargetButtonModel';
import CharacterImage from './CharacterImage.vue';
import HealthBar from './HealthBar.vue';

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
    <button 
        class="target-button" :class="{ 'selected': model?.selected }" @click="onClick">
        <HealthBar v-model="model.healthbar"></HealthBar>
        <CharacterImage v-model="model.characterImage"></CharacterImage>
        <div class="target-name">{{ model?.targetName }}</div>
    </button>
</template>