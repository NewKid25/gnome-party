<script setup lang="ts">
const socket = new WebSocket("wss://nl4g15d5v5.execute-api.us-east-2.amazonaws.com/Prod");

// Listen for messages
socket.addEventListener("message", (event) => {
  console.log("Message from server ", event.data);
});

</script>

<template>
  <h1>You did it!</h1>
  <p>
    Visit <a href="https://vuejs.org/" target="_blank" rel="noopener">vuejs.org</a> to read the
    documentation
  </p>
  <RouterView></RouterView>
</template>

<style scoped></style>
