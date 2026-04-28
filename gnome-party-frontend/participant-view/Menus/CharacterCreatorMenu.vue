<script setup lang="ts">
import { computed, reactive, ref } from "vue";

type CustomizationOption = {
    label: string;
    values: string[];
};

const emit = defineEmits<{
    ready: []
}>();

const customizationOptions = reactive<CustomizationOption[]>([
    {
        label: "Hat Shape",
        values: ["Pointy", "Top"],
    },
    {
        label: "Hair",
        values: ["Beard", "Braids"],
    },
    {
        label: "Color",
        values: ["Yellow", "Pink", "Blue"],
    },
]);

const selectedIndexes = ref<number[]>(customizationOptions.map(() => 0));

const selectedValues = computed(() =>
    customizationOptions.map((option, index) => ({
        label: option.label,
        value: option.values[selectedIndexes.value[index]],
    }))
);

function previousOption(index: number) {
    const option = customizationOptions[index];

    selectedIndexes.value[index] =
        selectedIndexes.value[index] === 0
            ? option.values.length - 1
            : selectedIndexes.value[index] - 1;
}

function nextOption(index: number) {
    const option = customizationOptions[index];

    selectedIndexes.value[index] =
        selectedIndexes.value[index] === option.values.length - 1
            ? 0
            : selectedIndexes.value[index] + 1;
}

function onReady() {
    console.log("Selected customization:", selectedValues.value);
    emit("ready");
}
</script>

<template>
    <div class="character-creator-menu">
        <h1 class="character-creator-title">Character Creator</h1>
        <hr>

        <div class="character-options-panel">
            <div
                v-for="(option, index) in customizationOptions"
                :key="option.label"
                class="character-option-row"
            >
                <button
                    class="character-arrow"
                    type="button"
                    @click="previousOption(index)"
                    :aria-label="`Previous ${option.label}`"
                >
                    ◀
                </button>

                <div class="character-option-text">
                    {{ option.label }}
                </div>

                <button
                    class="character-arrow"
                    type="button"
                    @click="nextOption(index)"
                    :aria-label="`Next ${option.label}`"
                >
                    ▶
                </button>
            </div>
        </div>

        <div class="character-preview-card">
            <img
                class="character-preview-image"
                src=""
                alt="Customized gnome preview"
            >
        </div>

        <hr>

        <button class="character-ready-button" type="button" @click="onReady">
            Ready
        </button>
    </div>
</template>