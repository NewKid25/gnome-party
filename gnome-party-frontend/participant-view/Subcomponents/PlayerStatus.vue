<script setup lang="ts">
import { PlayerStatusModel } from '../Models/PlayerStatusModel';
import { HealthBarModel } from '../Models/HealthBarModel';
import { PlayerImageModel } from '../Models/PlayerImageModel';
import HealthBar from './HealthBar.vue';
import PlayerImage from './PlayerImage.vue';

const model = defineModel<PlayerStatusModel>({required: true})

</script>

<template>
    <div class="player-status">
        <PlayerImage v-model="model.playerImage"></PlayerImage>
        <HealthBar v-model="model.healthBar"></HealthBar>
    </div>
</template>