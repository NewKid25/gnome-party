<script setup lang="ts">
import Konva from "konva";
import { onMounted } from "vue";
import ViewManager from "./scripts/ViewManager";


onMounted(() => {
  const vm = new ViewManager();

  window.ondblclick = () => {
    console.log("Game Session:", vm.socketStore.gameSessionId);
    console.log("Local player:", vm.socketStore.localPlayerId);
    console.log("Encounter:", vm.socketStore.encounterId);
    
    vm.socket.send(JSON.stringify({
    route: "start-campaign",
  }))}
  // vm.testAnimation();
})


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