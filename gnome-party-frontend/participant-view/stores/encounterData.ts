import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useEncounterData = defineStore('encounter', () => {
	const gameSessionId = ref("")
	const localPlayerId = ref("")
	const encounterId = ref("")

	return { gameSessionId, localPlayerId, encounterId }
})
