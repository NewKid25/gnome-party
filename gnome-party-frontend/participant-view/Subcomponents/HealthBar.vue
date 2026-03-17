<script setup lang="ts">
import { HealthBarModel } from '../Models/HealthBarModel';

const model = defineModel<HealthBarModel>({required: true});

</script>

<template>
    <div class="healthbar">
        <div class="healthbar-fill" :style="{width: (model.value / model.maxValue * 100) + '%'}"></div>
    </div>
</template>