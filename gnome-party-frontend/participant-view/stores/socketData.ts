import { defineStore } from "pinia";
import { ref } from "vue";

export const useSocketData = defineStore("socketData", () => {
    const socket = ref<WebSocket | null>(null);
    const isConnected = ref(false);

    // shared session/game state data
    const localPlayerId = ref("");
    const gameSessionId = ref("");
    const encounterId = ref("");
    const inviteCode = ref<number | null>(null);

    function connect(url: string) {
        if(socket.value && (socket.value.readyState === WebSocket.OPEN || socket.value.readyState === WebSocket.CONNECTING)) {
            return socket.value;
        }

        socket.value = new WebSocket(url);

        socket.value.addEventListener("open", () => {
            isConnected.value = true;
            console.log("WebSocket connected");
        });

        socket.value.addEventListener("close", () => {
            isConnected.value = false;
            console.log("WebSocket closed");
        });

        socket.value.addEventListener("error", (error) => {
            console.error("WebSocket error:", error);
        });

        return socket.value;
    }

    function send(data: object) {
        if(!socket.value || socket.value.readyState !== WebSocket.OPEN) {
            console.error("WebSocket is not open");
            return;
        }
        socket.value.send(JSON.stringify(data));
    }

    function disconnect() {
        if(socket.value) {
            socket.value.close();
            socket.value = null;
        }
        isConnected.value = false;
    }

    return {
        socket,
        isConnected,
        localPlayerId,
        gameSessionId,
        encounterId,
        inviteCode,
        connect,
        send,
        disconnect
    };
});