<script setup lang="ts">
import { ActionButtonModel } from './Models/ActionButtonModel'
import { ActionListModel } from './Models/ActionListModel'
import ActionList from './Subcomponents/ActionList.vue'

// This stuff is in here just to test. Eventually it will all be moved to CombatActionMenu

var buttonTestModel1:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 1'}
var buttonTestModel2:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 2'}
var buttonTestModel3:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 3'}

var actionListTestModel:ActionListModel = new ActionListModel([buttonTestModel1, buttonTestModel2, buttonTestModel3])

</script>

<template>
<p>participant view</p>

<ActionList v-model="actionListTestModel"></ActionList>
</template>