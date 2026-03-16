<script setup lang="ts">
import { MessageMenuModel } from "../Models/MessageMenuModel";

const model = defineModel<MessageMenuModel>({required: true});

</script>

<template>
    <div class="combat-menu-panel combat-dead-menu">
        <h1 class="combat-dead-menu-title">{{model.title}}</h1>
        <hr></hr>
        <p class="combat-dead-menu-message">{{model.message}}</p>
    </div>
</template>