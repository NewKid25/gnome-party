<script setup lang="ts">
import { computed, onBeforeUnmount, reactive, ref } from "vue";
import { useSocketData } from "../stores/socketData";

type Ability = {
    name: string;
    description: string;
};

type ClassOption = {
    backendName: string;
    displayName: string;
    health: number;
    imageSrc: string;
    imageAlt: string;
    abilities: Ability[];
};

const emit = defineEmits<{
    readySuccess: []
}>();

const socketStore = useSocketData();

const classOptions = reactive<ClassOption[]>([
    {
        backendName: "Warrior",
        displayName: "Warrior",
        health: 30,
        imageSrc: "/img/GnomeWarrior.svg",
        imageAlt: "Warrior gnome",
        abilities: [
            { name: "Slash", description: "Deal 10 damage to a target enemy." },
            { name: "Block", description: "Target an ally. Redirect attacks and reduce damage." },
            { name: "Parry", description: "Take no damage from a target enemy this turn." },
            { name: "Whirling Strike", description: "Deal 5 damage to all enemies." },
        ],
    },
    {
        backendName: "Bard",
        displayName: "Bard",
        health: 25,
        imageSrc: "/img/GnomeBard.svg",
        imageAlt: "Bard gnome",
        abilities: [
            { name: "Discord", description: "Deal 8 damage and reset Song." },
            { name: "Song", description: "Progresses through song effects." },
            { name: "Power Chord", description: "Apply song effect to all and stun yourself." },
            { name: "Mockery", description: "Deal 6 damage and force target to attack you." },
        ],
    },
    {
        backendName: "Mage",
        displayName: "Mage",
        health: 20,
        imageSrc: "/img/GnomeMage.svg",
        imageAlt: "Mage gnome",
        abilities: [
            { name: "Magic Missile", description: "Deal 10 unblockable damage." },
            { name: "Fireball", description: "Deal 12 damage and splash adjacent enemies." },
            { name: "Ice Ray", description: "Deal 5 damage and weaken target." },
            { name: "Mirror", description: "Repeat your next spell on this enemy." },
        ],
    },
]);

const selectedIndex = ref(0);
const isReadying = ref(false);
const errorMessage = ref("");

const selectedClass = computed(() => classOptions[selectedIndex.value]);

const heartCount = computed(() => Math.floor(selectedClass.value.health / 5));

function previousClass() {
    selectedIndex.value =
        selectedIndex.value === 0 ? classOptions.length - 1 : selectedIndex.value - 1;
}

function nextClass() {
    selectedIndex.value =
        selectedIndex.value === classOptions.length - 1 ? 0 : selectedIndex.value + 1;
}

function onSocketMessage(event: MessageEvent) {
    const data = JSON.parse(event.data);
    console.log("Class select message", data);

    if (data.Subject === "lobby-ready-success") {
        isReadying.value = false;
        emit("readySuccess");
    }

    if (data.Subject === "host-disconnected") {
        errorMessage.value = "Host disconnected.";
        isReadying.value = false;
    }
}

function onReady() {
    if (isReadying.value) return;

    errorMessage.value = "";
    isReadying.value = true;

    socketStore.socket?.removeEventListener("message", onSocketMessage);
    socketStore.socket?.addEventListener("message", onSocketMessage);

    socketStore.send({
        route: "lobby-ready",
        CharacterType: selectedClass.value.backendName,
    });
}

onBeforeUnmount(() => {
    socketStore.socket?.removeEventListener("message", onSocketMessage);
});
</script>

<template>
    <div class="class-select-menu">
        <h1 class="class-select-title">Choose a Class</h1>
        <hr>

        <div class="class-selector-row">
            <button class="class-arrow" type="button" @click="previousClass" aria-label="Previous class">
                ◀
            </button>

            <div class="class-card">
                <div class="class-card-image-wrap">
                    <img
                        class="class-card-image"
                        :src="selectedClass.imageSrc"
                        :alt="selectedClass.imageAlt"
                    >
                </div>

                <div class="class-heart-row" aria-label="Health">
                    <span
                        v-for="n in heartCount"
                        :key="n"
                        class="class-heart"
                    >
                        ❤
                    </span>
                </div>
            </div>

            <button class="class-arrow" type="button" @click="nextClass" aria-label="Next class">
                ▶
            </button>
        </div>

        <div class="class-name-line">
            {{ selectedClass.displayName }}
        </div>

        <div class="class-abilities-panel">
            <div
                v-for="ability in selectedClass.abilities"
                :key="ability.name"
                class="class-ability-row"
            >
                <span class="class-ability-name">{{ ability.name }}</span>
                <span class="class-ability-description"> – {{ ability.description }}</span>
            </div>
        </div>

        <hr>

        <button class="class-ready-button" type="button" @click="onReady" :disabled="isReadying">
            {{ isReadying ? "Readying..." : "Ready" }}
        </button>

        <p v-if="errorMessage" class="class-select-error">{{ errorMessage }}</p>
    </div>
</template>