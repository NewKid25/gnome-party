<script setup lang="ts">
import Konva from "konva";
import { onMounted, ref } from "vue";
import ViewManager from "./scripts/ViewManager";

const viewManager = ref<ViewManager | null>(null);

onMounted(() => {
  const vm = new ViewManager();
  viewManager.value = vm;

  window.ondblclick = () => {
    console.log("Game Session:", vm.socketStore.gameSessionId);
    console.log("Local player:", vm.socketStore.localPlayerId);
    console.log("Encounter:", vm.socketStore.encounterId);
    
    vm.socket.send(JSON.stringify({
    route: "start-campaign",
  }))}
  // vm.testAnimation();
})

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
  <button @click="startCombat" style="position: absolute; top: 10px; left: 10px; z-index: 1;">
    Start Combat
  </button>
  
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