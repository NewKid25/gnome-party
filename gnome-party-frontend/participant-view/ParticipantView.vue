<script setup lang="ts">
import { ActionButtonModel } from './Models/ActionButtonModel'
import { ActionListModel } from './Models/ActionListModel'
import ActionList from './Subcomponents/ActionList.vue'

import { TargetButtonModel } from './Models/TargetButtonModel'
import { TargetListModel } from './Models/TargetListModel'
import TargetList from './Subcomponents/TargetList.vue'

// This stuff is in here just to test. Eventually it will all be moved to CombatActionMenu

var buttonTestModel1:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 1'}
var buttonTestModel2:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 2'}
var buttonTestModel3:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 3'}

var actionListTestModel:ActionListModel = new ActionListModel([buttonTestModel1, buttonTestModel2, buttonTestModel3])

var buttonTestModelA:TargetButtonModel = {'selected': false, 'targetName': 'Hello World A'}
var buttonTestModelB:TargetButtonModel = {'selected': false, 'targetName': 'Hello World B'}
var buttonTestModelC:TargetButtonModel = {'selected': false, 'targetName': 'Hello World C'}

var targetListTestModel:TargetListModel = new TargetListModel([buttonTestModelA, buttonTestModelB, buttonTestModelC])

</script>

<template>
<p>participant view</p>

<ActionList v-model="actionListTestModel"></ActionList>

<TargetList v-model="targetListTestModel"></TargetList>
</template>