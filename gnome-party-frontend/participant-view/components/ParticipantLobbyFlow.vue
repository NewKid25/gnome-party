<script setup lang="ts">
import { ref } from "vue";
import LobbyScreen from "../Menus/LobbyJoinMenu.vue";
import ClassSelectScreen from "../Menus/ClassSelectMenu.vue";

type LobbyStep = "join" | "classSelect" | "waiting";

const currentStep = ref<LobbyStep>("join");

function onJoined() {
    currentStep.value = "classSelect";
}

function onReadySuccess() {
    currentStep.value = "waiting";
}
</script>

<template>
    <LobbyScreen v-if="currentStep === 'join'" @joined="onJoined"/>

    <ClassSelectScreen v-else-if="currentStep === 'classSelect'" @ready-success="onReadySuccess"/>

    <!-- Replace with waiting room -->
    <div v-else> Waiting for host to start...</div>
</template>