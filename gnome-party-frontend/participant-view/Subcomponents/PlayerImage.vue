<script setup lang="ts">
import { PlayerImageModel } from '../Models/PlayerImageModel';

const model = defineModel<PlayerImageModel>({required: true})

</script>

<template>
    <div class="player-image">
        <img />
    </div>
</template>