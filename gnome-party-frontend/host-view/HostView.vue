<script setup lang="ts">
import Konva from "konva";
import { onMounted, ref } from "vue";
import ViewManager from "./scripts/ViewManager";

const viewManager = ref<ViewManager | null>(null);

onMounted(() => {
  viewManager.value = new ViewManager();
});

function startCombat() {
  if (!viewManager.value) {
    console.error("ViewManager not initialized yet.");
    return;
  }

  console.log("Game Session:", viewManager.value.socketStore.gameSessionId);
  console.log("Local player:", viewManager.value.socketStore.localPlayerId);
  console.log("Encounter:", viewManager.value.socketStore.encounterId);

  viewManager.value.socket.send(
    JSON.stringify({
      route: "start-campaign",
    })
  );
}

</script>
<template>
  <div id="background"></div>
  <div id="konva-container"></div>

</template>
<style lang="css" scoped>
  #konva-container {
    width: 100vw;
    height: 100vh;
  }

  #background {
    width: 100vw;
    height: 100vh;
    position: absolute;
    background-color: #7ECE48;
    background-image: url(/img/Grass.svg);
    background-repeat: repeat;
    background-size: 75%;
  }
</style>