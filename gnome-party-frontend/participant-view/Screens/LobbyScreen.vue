<script setup lang="ts">
import { reactive, ref, onBeforeUnmount } from 'vue';
import { useEncounterData } from '../stores/encounterData';
import { useSocketData } from '../stores/socketData';

const encounterData = useEncounterData();
const socketStore = useSocketData();

const model = reactive({
    roomCode: "",
    playerName: "",
});

const errorMessage = ref("");
const isJoining = ref(false);
const hasJoined = ref(false);

const SOCKET_URL = "wss://ws.gnome-party.com";

// handles backend messages related to lobby
function onSocketMessage(event: MessageEvent) {
    const data = JSON.parse(event.data);
    console.log("Lobby message ", data);

    if (data.Subject === "join-game-connection") {
        encounterData.localPlayerId = data.Message.UserId;
        socketStore.localPlayerId = data.Message.UserId;
    } 
    else if (data.Subject === "join-game-session") {
        encounterData.gameSessionId = data.Message.GameSessionId;
        socketStore.gameSessionId = data.Message.GameSessionId;

        if (data.Message.InviteCode !== undefined) {
            socketStore.inviteCode = data.Message.InviteCode;
        }

        isJoining.value = false;
        hasJoined.value = true;

        console.log("Joined game session:", encounterData.gameSessionId);

        // temp auto-ready message, remove after adding character customization and waiting room
        socketStore.send({
            route: "lobby-ready",
            CharacterType: "Mage"
        });

        // TODO: navigate to character customization screen then waiting room
    }
    else if (data.Subject === "lobby-ready-success") {
        console.log("Participant successfully readied:", data.Message);
    }
    else if (data.Subject === "begin-combat-encounter") {
        encounterData.encounterId = data.Message.EncounterId;
        socketStore.encounterId = data.Message.EncounterId;
    } 
    else if (data.Subject === "join-game-session-failed") {
        errorMessage.value = data.Message ?? "Failed to join game session.";
        isJoining.value = false;
    } 
    else if (data.Subject === "host-disconnected") {
        errorMessage.value = data.Message ?? "Host disconnected.";
        isJoining.value = false;
        hasJoined.value = false;
    }
}

function onPlay() {
    // block duplicate joins
    if (isJoining.value || hasJoined.value || socketStore.gameSessionId) {
        return;
    }

    errorMessage.value = "";

    const inviteCode = Number(model.roomCode.trim());

    if (!Number.isInteger(inviteCode) || model.roomCode.trim().length !== 6) {
        errorMessage.value = "Please enter a valid 6-digit room code.";
        return;
    }

    if (!model.playerName.trim()) {
        errorMessage.value = "Please enter a player name.";
        return;
    }

    isJoining.value = true;

    const socket = socketStore.connect(SOCKET_URL);

    // prevent duplicate listeners
    socket.removeEventListener("message", onSocketMessage);
    socket.addEventListener("message", onSocketMessage);

    socket.addEventListener("error", onSocketError, { once: true });
    socket.addEventListener("close", onSocketClose, { once: true });

    if (socket.readyState === WebSocket.OPEN) {
        socketStore.send({
            route: "join-game",
            InviteCode: inviteCode,
        });
    } else {
        socket.addEventListener("open", () => {
            socketStore.send({
                route: "join-game",
                InviteCode: inviteCode,
            });
        }, { once: true });
    }
}

function onSocketError() {
    errorMessage.value = "Failed to connect to the server.";
    isJoining.value = false;
}

function onSocketClose() {
    if (isJoining.value) {
        errorMessage.value = "Connection closed before join completed.";
        isJoining.value = false;
    }
}

// remove listener but keep socket open
onBeforeUnmount(() => {
    socketStore.socket?.removeEventListener("message", onSocketMessage);
});
</script>

<template>
    <div class="lobby-view">
        <div class="lobby-container">
            <div class="lobby-menu-panel">
                <div class="lobby-title-panel">
                    <h1 class="lobby-title">Gnome Party</h1>
                </div>

                <label class="lobby-label" for="room-code">ROOM CODE</label>
                <input id="room-code" type="text" v-model="model.roomCode" placeholder="ENTER 6-DIGIT CODE" maxlength="6">

                <label class="lobby-label" for="player-name">NAME</label>
                <input id="player-name" type="text" v-model="model.playerName" placeholder="ENTER YOUR NAME">

                <button @click="onPlay" :disabled="isJoining || hasJoined">
                    {{ isJoining ? 'JOINING...' : hasJoined ? 'JOINED' : 'PLAY' }}
                </button>

                <p class="error-message" v-if="errorMessage">{{ errorMessage }}</p>
            </div>
        </div>
    </div>
</template>