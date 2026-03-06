<script setup lang="ts">
import { PlayerStatusModel } from '../Models/PlayerStatusModel';
import HealthBar from './HealthBar.vue';
import CharacterImage from './CharacterImage.vue';

const model = defineModel<PlayerStatusModel>({required: true})

</script>

<template>
    <div class="player-status">
        <CharacterImage v-model="model.characterImage"></CharacterImage>
        <HealthBar v-model="model.healthBar"></HealthBar>
        <hr>
    </div>
</template>