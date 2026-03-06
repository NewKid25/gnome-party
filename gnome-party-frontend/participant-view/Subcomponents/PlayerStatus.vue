<script setup lang="ts">
import { PlayerStatusModel } from '../Models/PlayerStatusModel';
import HealthBar from './HealthBar.vue';
import PlayerImage from './PlayerImage.vue';

const model = defineModel<PlayerStatusModel>({required: true})

</script>

<template>
    <div class="player-status">
        <PlayerImage v-model="model.playerImage"></PlayerImage>
        <HealthBar v-model="model.healthBar"></HealthBar>
        <hr>
    </div>
</template>