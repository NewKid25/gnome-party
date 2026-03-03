<script setup lang="ts">
import Konva from "konva";
import { onMounted } from "vue";
import initializeKonva from "./scripts/initialize-konva";


onMounted(() => {
  initializeKonva();
})


</script>
<template>
  <div id="konva-container"></div>
</template>
<style lang="css" scoped>
  #konva-container {
    width: 100vw;
    height: 100vh;
  }
</style>