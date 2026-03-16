<script setup lang="ts">
import { MessageMenuModel } from "../Models/MessageMenuModel";

const model = defineModel<MessageMenuModel>({required: true});

</script>

<template>
    <div class="combat-menu-panel combat-waiting-menu">
        <h1 class="combat-waiting-menu-title waiting-dots">{{model.title}}</h1>
        <hr></hr>
        <p class="combat-waiting-menu-message">{{model.message}}</p>
    </div>
</template>