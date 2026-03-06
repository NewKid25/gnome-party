<script setup lang="ts">

import { RouterView } from "vue-router";

const socket = new WebSocket("wss://nl4g15d5v5.execute-api.us-east-2.amazonaws.com/Prod");

// Listen for messages
socket.addEventListener("message", (event) => {
  console.log("Message from server ", event.data);
});

</script>

<template>
  <RouterView></RouterView>
</template>