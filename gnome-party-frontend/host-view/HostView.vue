<script setup lang="ts">
import Konva from "konva";
import { onMounted } from "vue";
import ViewManager from "./scripts/ViewManager";


onMounted(() => {
  const vm = new ViewManager();
  vm.loadEncounter();

  vm.testAnimation();
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