<script setup lang="ts">
import { onBeforeUnmount, ref } from "vue";
import ParticipantLobbyFlow from "./components/ParticipantLobbyFlow.vue";
import ParticipantCombatFlow from "./components/ParticipantCombatFlow.vue";

import "./styles.css";
import { useSocketData } from "./stores/socketData";

type ParticipantPhase = "lobby" | "combat";

const participantPhase = ref<ParticipantPhase>("lobby");

const SOCKET_URL = "wss://ws.gnome-party.com";

const socketStore = useSocketData();
const socket = socketStore.socket ?? socketStore.connect(SOCKET_URL);

function onSocketMessage(event: MessageEvent) {
  const parsedJSON = JSON.parse(event.data);

  if (parsedJSON.Subject === "join-game-connection") {
    socketStore.localPlayerId = parsedJSON.Message.UserId;
  }

  if (parsedJSON.Subject === "join-game-session") {
    socketStore.gameSessionId = parsedJSON.Message.GameSessionId;
  }

  if (parsedJSON.Subject === "start-campaign") {
    participantPhase.value = "combat";
  }
}

socket.removeEventListener("message", onSocketMessage);
socket.addEventListener("message", onSocketMessage);

onBeforeUnmount(() => {
  socket.removeEventListener("message", onSocketMessage);
});
</script>

<template>
  <div class="participant-view">
    <div class="participant-container">
      <Transition name="combat-menu" mode="out-in">
        <ParticipantLobbyFlow
          v-if="participantPhase === 'lobby'"
          key="participant-lobby-flow"
        />

        <ParticipantCombatFlow
          v-else-if="participantPhase === 'combat'"
          key="participant-combat-flow"
        />
      </Transition>
    </div>
  </div>
</template>