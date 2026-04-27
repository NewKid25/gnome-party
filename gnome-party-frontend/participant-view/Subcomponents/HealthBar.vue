<script setup lang="ts">
import { computed } from 'vue';
import { HealthBarModel } from '../Models/HealthBarModel';

const model = defineModel<HealthBarModel>({required: true});

const healthPercent = computed(() => {
    if (model.value.maxValue <= 0) return 0;

    return Math.max(
        0,
        Math.min(100, (model.value.value / model.value.maxValue) * 100)
    );
});
</script>

<template>
    <div class="healthbar">
        <div class="healthbar-fill" :style="{ width: healthPercent + '%' }"></div>
    </div>
</template>