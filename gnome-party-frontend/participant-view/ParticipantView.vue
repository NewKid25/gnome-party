<script setup lang="ts">
import CombatActionMenu from './Menus/CombatActionMenu.vue'
import { ActionButtonModel } from './Models/ActionButtonModel'
import { ActionListModel } from './Models/ActionListModel'
import ActionList from './Subcomponents/ActionList.vue'
import { Ref, ref } from 'vue'

// Eventually, this info will be pulled in via websockets. A lot of the logic will probably be moved into its own file as well

var buttonTestModel1:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 1'}
var buttonTestModel2:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 2'}
var buttonTestModel3:ActionButtonModel = {'selected': false, 'actionName': 'Hello World 3'}

var actionListTestModel:ActionListModel = new ActionListModel([buttonTestModel1, buttonTestModel2, buttonTestModel3])
var actionMenuTestModel = {actionListModel: actionListTestModel}

enum MenuScreen {
	CombatAction,
	CombatTarget
}

var currentActiveMenu:Ref<MenuScreen> = ref(MenuScreen.CombatAction);

function onActionChosen(actionButton:ActionButtonModel) {
	console.log("ParticipantView:", actionButton);
	currentActiveMenu.value = MenuScreen.CombatTarget;
}

</script>

<template>
<p>participant view</p>

<CombatActionMenu v-model="actionMenuTestModel" v-if="currentActiveMenu == MenuScreen.CombatAction" @action-chosen="onActionChosen"></CombatActionMenu>
<CombatTargetMenu v-if="currentActiveMenu == MenuScreen.CombatTarget"></CombatTargetMenu>
</template>