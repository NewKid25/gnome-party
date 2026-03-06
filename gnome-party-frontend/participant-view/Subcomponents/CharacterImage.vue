<script setup lang="ts">
import { CharacterImageModel } from '../Models/CharacterImageModel';

const model = defineModel<CharacterImageModel>({required: true})

</script>

<template>
    <div class="character-image">
        <img :src="model.source" :alt="model.alt"/>
    </div>
</template>