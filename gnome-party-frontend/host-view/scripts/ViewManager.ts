import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";
import AnimationPause from "./AnimationPause";
import GnomePuppet from "./puppets/GnomePuppet";
import HealthBar from "./HealthBar";
import FunctionStep from "./FunctionStep";
import AnimationStep from "./interfaces/AnimationStep";
import { Character, GameState, TurnStep } from "./interfaces/TurnStep";
import Puppet from "./interfaces/Puppet";
import SlashAnimation from "./action-animations/SlashAnimation";
import BoneSlashAnimation from "./action-animations/BoneSlashAnimation";
import { useSocketData } from "../../participant-view/stores/socketData";
import SkeletonPuppet from "./puppets/SkeletonPuppet";
import GoblinArcherPuppet from "./puppets/GoblinArcherPuppet";
import { C } from "vue-router/dist/options-CjwwR_07.cjs";
import ForestSpritePuppet from "./puppets/ForestSpritePuppet";
import CaveBatPuppet from "./puppets/CaveBatPuppet";
import GnombieBrutePuppet from "./puppets/GnombieBrutePuppet";
import GnomeEaterPuppet from "./puppets/GnomeEaterPuppet";
import NecrognomancerPuppet from "./puppets/NecrognomancerPuppet";
import TextPopupAnimation from "./animations/TextPopupAnimation";
import HeavySlamAnimation from "./action-animations/HeavySlamAnimation";
import CrushingSwipeAnimation from "./action-animations/CrushingSwipeAnimation";
import BlockAnimation from "./action-animations/BlockAnimation";
import ParryAnimation from "./action-animations/ParryAnimation";
import RattleGuardAnimation from "./action-animations/RattleGuardAnimation";
import WhirlingStrikeAnimation from "./action-animations/WhirlingStrikeAnimation";
import SongAnimation from "./action-animations/SongAnimation";
import DiscordAnimation from "./action-animations/DiscordAnimation";
import MockeryAnimation from "./action-animations/MockeryAnimation";
import IceRayAnimation from "./action-animations/IceRayAnimation";
import FireballAnimation from "./action-animations/FireballAnimation";
import MagicMissileAnimation from "./action-animations/MagicMissileAnimation";
import PiercingArrowAnimation from "./action-animations/PiercingArrowAnimation";
import CripplingShotAnimation from "./action-animations/CripplingShotAnimation";
import RottingAuraAnimation from "./action-animations/RottingAuraAnimation";
import DarkBoltAnimation from "./action-animations/DarkBoltAnimation";
import LeafDartAnimation from "./action-animations/LeafDartAnimation";
import DevourEssenceAnimation from "./action-animations/DevourEssenceAnimation";
import SonicSquealAnimation from "./action-animations/SonicSquealAnimation";
import BloodPeckAnimation from "./action-animations/BloodPeckAnimation";
import PrimalRoarAnimation from "./action-animations/PrimalRoarAnimation";
import SoulDrainAnimation from "./action-animations/SoulDrainAnimation";
import MirrorAnimation from "./action-animations/MirrorAnimation";
import RavenousGrowthAnimation from "./action-animations/RavenousGrowthAnimation";
import EntangleAnimation from "./action-animations/EntangleAnimation";
import PowerChordAnimation from "./action-animations/PowerChordAnimation";
import SummonAnimation from "./action-animations/SummonAnimation";

export default
class ViewManager {

	HEALTHBAR_HEIGHT = 120;


	socket:WebSocket

	stage:Konva.Stage

	backgroundLayer:Konva.Layer
	decorLayer:Konva.Layer
	lobbyLayer:Konva.Layer
	combatLayer:Konva.Layer
	uiLayer:Konva.Layer
	
	playerVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()
	enemyVisualComponents:Map<string, CharacterVisualComponents> = new Map<string, CharacterVisualComponents>()

	socketStore = useSocketData();

	// track participants in lobby on host side
	readyPlayers:any[] = [];

	inviteCode: string = "";
	isCombatStarted: boolean = false;
	hasStartButtonBeenPressed: boolean = false;

	lobbyPlayerPuppets:Map<string, GnomePuppet> = new Map();
	lobbyPlayerNames:Map<string, Konva.Text> = new Map();
	lobbyPlayerShadows:Map<string, Konva.Ellipse> = new Map();

	lobbyRoomCodeGroup:Konva.Group | null = null;
	lobbyPlayerCountGroup:Konva.Group | null = null;
	lobbyBannerGroup:Konva.Group | null = null;
	lobbyStartButtonGroup:Konva.Group | null = null;

	cloudGroups:Konva.Group[] = [];
	cloudAnimation:Konva.Animation | null = null;

	combatEndOverlayGroup:Konva.Group | null = null;


	constructor() {
		this.socket = new WebSocket("wss://ws.gnome-party.com");

		this.socket.addEventListener("message", (ev) => {
			console.log("Message from server ", ev.data);
  			let parsedJSON = JSON.parse(ev.data);
			
			this.handleMessage(parsedJSON);
		});

		const container:HTMLDivElement = document.getElementById("konva-container") as HTMLDivElement;
	
		// first we need to create a stage
		this.stage = new Konva.Stage({
			container: 'konva-container', // id of container <div>
			width: container.clientWidth,
			height: container.clientHeight
		});
	
		// then create layers
		this.backgroundLayer = new Konva.Layer();
		this.decorLayer = new Konva.Layer();
		this.lobbyLayer = new Konva.Layer();
		this.combatLayer = new Konva.Layer();
		this.uiLayer = new Konva.Layer();
	
		// add layers to the stage
		this.stage.add(this.backgroundLayer);
		this.stage.add(this.decorLayer);
		this.stage.add(this.lobbyLayer);
		this.stage.add(this.combatLayer);
		this.stage.add(this.uiLayer);

		this.socket.onopen = (ev:Event) => {
			this.socket.send(JSON.stringify({route: "host-game"}))
		};

		window.addEventListener("resize", this.handleResize);
	}

	handleResize = () => {
		const container = document.getElementById("konva-container") as HTMLDivElement;
		if (!container) return;

		this.stage.width(container.clientWidth);
		this.stage.height(container.clientHeight);

		if (this.isCombatStarted) {
			// Re-render combat scene on resize if needed later.
			// For now, just redraw existing layers.
			this.backgroundLayer.draw();
			this.decorLayer.draw();
			this.combatLayer.draw();
			this.uiLayer.draw();
		} else {
			this.renderHostLobby();
		}
	};

	w() {
		return this.stage.width();
	}

	h() {
		return this.stage.height();
	}
	
loadEncounter(gameState:any)
	{
		this.clearCombatScene();

		var playerCharacters = gameState.PlayerCharacters;
		var enemyCharacters = gameState.EnemyCharacters;

		for (let i = 0; i < playerCharacters.length; i++) {
			this.addPlayerToCombat(playerCharacters[i], i, playerCharacters.length);
		}

		for (let i = 0; i < enemyCharacters.length; i++) {
			this.addEnemyToCombat(enemyCharacters[i], i, enemyCharacters.length);
		}

		this.combatLayer.draw();
		this.uiLayer.draw();
	}

	getCombatPuppetScale() {
		return Math.min(this.w(), this.h()) * 0.00026;
	}

	addPlayerToCombat(playerCharacter: any, index: number, totalPlayers: number) {
		const pos = this.getPlayerCombatPosition(index, totalPlayers);
		const combatScale = this.getCombatPuppetScale();

		const puppet = new GnomePuppet(playerCharacter.CharacterType);
		puppet.scale({ x: combatScale, y: combatScale });
		puppet.position({ x: pos.x, y: pos.y });

		this.combatLayer.add(puppet);

		let healthbar = new HealthBar(playerCharacter.MaxHealth, {
			x: this.w() * 0.075,
			y: this.h() * 0.025,
		});

		healthbar.x(pos.x - this.w() * 0.0375);
		healthbar.y(pos.y - 620 * combatScale);

		this.uiLayer.add(healthbar);
		this.playerVisualComponents.set(playerCharacter.Id, { puppet, healthbar });
	}

	addEnemyToCombat(enemyCharacter: any, index: number, totalEnemies: number) {
		const pos = this.getEnemyCombatPosition(index, totalEnemies);
		const combatScale = this.getCombatPuppetScale();

		// get appropriate puppet later
		const puppet = this.createEnemyPuppet(enemyCharacter.CharacterType);

		puppet.scale({ x: combatScale, y: combatScale });
		puppet.position({ x: pos.x, y: pos.y });

		this.combatLayer.add(puppet);

		let healthbar = new HealthBar(enemyCharacter.MaxHealth, {
			x: this.w() * 0.075,
			y: this.h() * 0.025,
		});

		healthbar.x(pos.x - this.w() * 0.0375);
		healthbar.y(pos.y - 600 * combatScale);

		this.uiLayer.add(healthbar);
		this.enemyVisualComponents.set(enemyCharacter.Id, { puppet, healthbar });
	}

	handleMessage(msg:any) {
		if (msg.Subject == "host-game") {
			this.socketStore.gameSessionId = msg.Message.GameSessionId;
			this.socketStore.localPlayerId = msg.Message.Host.UserId;
			this.inviteCode = msg.Message.InviteCode;

			// for testing
			console.log("Host game created");
			console.log("Invite code:", msg.Message.InviteCode);
			console.log("Game session:", msg.Message.GameSessionId);

			this.renderHostLobby();
		}
		if (msg.Subject === "lobby-ready") {
			const existing = this.readyPlayers.find((p) => p.Id === msg.Message.Id);
			if (!existing) {
				this.readyPlayers.push(msg.Message);
			}
			console.log("Participant readied:", msg.Message);
			console.log("Ready player count:", this.readyPlayers.length);

			this.renderHostLobby();
		}
		if (msg.Subject === "lobby-unready") {
			const existing = this.readyPlayers.find((p) => p.Id === msg.Message.Id);
			if (existing) {
				this.readyPlayers.splice(this.readyPlayers.indexOf(existing), 1);
			}
			console.log("Participant unreadied:", msg.Message);
			this.renderHostLobby();
		}

		if (msg.Subject === "player-disconnected") {
			this.readyPlayers = this.readyPlayers.filter(
				(p) => p.Id !== msg.Message?.Id && p.UserId !== msg.Message?.UserId
			);
			console.log("Participant disconnected:", msg.Message);
		}

		if (msg.Subject === "host-disconnected") {
			console.log("Host disconnected:", msg.Message);
		}

		if (msg.Subject === "start-campaign") {
			if (this.isCombatStarted) return;

			this.isCombatStarted = true;
			console.log("Campaign started:", msg.Message);
			this.socket.send(JSON.stringify({
				route: "begin-combat-encounter",
				GameSessionId: this.socketStore.gameSessionId
			}))
		}

		if (msg.Subject == "begin-combat-encounter") {
			this.clearLobbyScene();
			this.prepareCombatScene();
			this.socketStore.encounterId = msg.Message.EncounterId;
			this.loadEncounter(msg.Message.GameState);
		}

		if(msg.Subject === "combat-encounter-ended") {
			if(msg.Message === "players-defeated") {
				this.renderCombatEndOverlay("You Died!", "Retry");
			}
			if(msg.Message === "enemies-defeated") {
				this.renderCombatEndOverlay("Victory!", "Continue");
			}
		}

		if (msg.Subject == "action-handler")
		{
			this.processTurn(msg.Message);
		}
	}

	processTurn(turn:TurnStep[]) 
	{
		let animations:AnimationStep[] = [];

		for (let step of turn) 
		{
			let actionAnimation:AnimationStep | undefined = this.instantiateActionAnimation(step);
			if (actionAnimation) {
				animations.push(actionAnimation);
			}

			let eventAnimation: AnimationStep | undefined = this.instantiateEventAnimation(step);
			if (eventAnimation) {
				animations.push(eventAnimation);
			}
		}

		if (turn.at(-1) != undefined)
			animations.push(new AnimationPause(500));
			//@ts-ignore
			animations.push(this.updateAllHealth(turn.at(-1).GameState))

		let sequence:AnimationSequence = new AnimationSequence(animations);
		sequence.play();
	}

	testAnimation()
	{
		// let anims:AnimationStep[] = []
		// for (const pair of this.playerVisualComponents) {
		// 	const pvc:CharacterVisualComponents = pair[1];

		// 	const enemy:CharacterVisualComponents = (this.enemyVisualComponents.get('0') as CharacterVisualComponents)

		// 	let anim:LeapAnimation = new LeapAnimation({
		// 		leapingNode: pvc.puppet, 
		// 		destination: enemy.puppet, 
		// 		landingAnimation: new FunctionStep(() => { enemy.healthbar.changeHealth(20 - (parseInt(pair[0]) + 1) * 2); })
		// 	});

		// 	anims.push(anim);
		// }

		// let sequence:AnimationSequence = new AnimationSequence(anims);

		// sequence.play();

		
		// let gameState:GameState = {},

		// this.processTurn(sampleSteps);
	}

	instantiateActionAnimation(step:TurnStep)
	{
		let actionName = step.Request.Action;
		switch(actionName) {
			case "Slash":
				return new SlashAnimation(step, this);
			case "Bone Slash":
				return new BoneSlashAnimation(step, this);
			case "Heavy Slam":
				return new HeavySlamAnimation(step, this);
			case "Crushing Swipe":
				return new CrushingSwipeAnimation(step, this);
			case "Block":
				return new BlockAnimation(step, this);
			case "Parry":
				return new ParryAnimation(step, this);
			case "Rattle Guard":
				return new RattleGuardAnimation(step, this);
			case "Whirling Strike":
				return new WhirlingStrikeAnimation(step, this);
			case "Soothing Song":
				return new SongAnimation(step, this, 0);
			case "Inspiring Song":
				return new SongAnimation(step, this, 1);
			case "Frightening Song":
				return new SongAnimation(step, this, 2);
			case "Discord":
				return new DiscordAnimation(step, this);
			case "Mockery":
				return new MockeryAnimation(step, this);
			case "Ice Ray":
				return new IceRayAnimation(step, this);
			case "Fireball":
				return new FireballAnimation(step, this);
			case "Magic Missile":
				return new MagicMissileAnimation(step, this);
			case "Piercing Arrow":
				return new PiercingArrowAnimation(step, this);
			case "Crippling Shot":
				return new CripplingShotAnimation(step, this);
			case "Rotting Aura":
				return new RottingAuraAnimation(step, this);
			case "Dark Bolt":
				return new DarkBoltAnimation(step, this);
			case "Leaf Dart":
				return new LeafDartAnimation(step, this);
			case "Devour Essence":
				return new DevourEssenceAnimation(step, this);
			case "Sonic Squeal":
				return new SonicSquealAnimation(step, this);
			case "Blood Peck":
				return new BloodPeckAnimation(step, this);
			case "Primal Roar":
				return new PrimalRoarAnimation(step, this);
			case "Soul Drain":
				return new SoulDrainAnimation(step, this);
			case "Mirror":
				return new MirrorAnimation(step, this);
			case "Ravenous Growth":
				return new RavenousGrowthAnimation(step, this);
			case "Entangle":
				return new EntangleAnimation(step, this);
			case "Power Cord":
			case "Power Chord":
				return new PowerChordAnimation(step, this);
			case "Summon":
				return new SummonAnimation(step, this);
			default:
				return this.updateAllHealth(step.GameState);
		}
	}

	instantiateEventAnimation(step: TurnStep) {
		if (!step.Events || step.Events.length === 0) return undefined;

		return new FunctionStep(() => {
			for (const combatEvent of step.Events) {
				if (combatEvent.event === "damage") {
					const targetId = combatEvent.params.TargetId;

					const targetHealth =
						step.GameState.PlayerCharacters.find((c: any) => c.Id === targetId)?.Health ??
						step.GameState.EnemyCharacters.find((c: any) => c.Id === targetId)?.Health;

					if (targetHealth === undefined) continue;

					const playerTarget = this.playerVisualComponents.get(targetId);
					const enemyTarget = this.enemyVisualComponents.get(targetId);

					if (playerTarget) {
						playerTarget.healthbar.changeHealth(Math.max(0, targetHealth));
					}

					if (enemyTarget) {
						enemyTarget.healthbar.changeHealth(Math.max(0, targetHealth));
					}
				}

				if (combatEvent.event === "defeated") {
					const targetId = combatEvent.params.TargetId;
					this.markCharacterDefeated(targetId);
				}
			}
		});
	}

	prepareCombatScene() {
		this.backgroundLayer.destroyChildren();
		this.decorLayer.destroyChildren();
		this.lobbyLayer.destroyChildren();
		this.uiLayer.destroyChildren();

		this.backgroundLayer.draw();
		this.decorLayer.draw();
		this.lobbyLayer.draw();
		this.uiLayer.draw();
	}

	getPlayerCombatPosition(index: number, totalPlayers: number) {
		const w = this.w();
		const h = this.h();
		const x = w * 0.18;
		const y = ((index + 1) * h) / (totalPlayers + 1);

		return { x, y };
	}

	getEnemyCombatPosition(index: number, totalEnemies: number) {
		const w = this.w();
		const h = this.h();
		const x = w * 0.82;
		const y = ((index + 1) * h) / (totalEnemies + 1);

		return { x, y };
	}

	renderCombatTimer() {
		const w = this.w();
		const h = this.h();

		const group = new Konva.Group({
			x: w * 0.40,
			y: h * 0.01,
		});

		const box = new Konva.Rect({
			width: w * 0.20,
			height: h * 0.08,
			fill: "#f7f7f7",
			stroke: "#b0b0b0",
			strokeWidth: 2,
			shadowColor: "black",
			shadowBlur: 4,
			shadowOpacity: 0.15,
		});

		const text = new Konva.Text({
			x: 0,
			y: h * 0.015,
			width: w * 0.20,
			align: "center",
			text: "0:40",
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.045,
			fill: "#000",
		});

		group.add(box, text);
		this.uiLayer.add(group);
	}

	renderHostLobby() {
		this.clearLobbyScene();
		this.renderLobbyBackgroundShapes();
		this.renderLobbyDecorations();
		this.renderLobbyBanner();
		this.renderLobbyRoomCodeCard();
		this.renderLobbyPlayerCount();
		this.renderLobbyPlayers();
		this.renderLobbyStartButton();

		this.backgroundLayer.draw();
		this.decorLayer.draw();
		this.lobbyLayer.draw();
		this.uiLayer.draw();
	}

	renderLobbyBackgroundShapes() {
		const w = this.w();
		const h = this.h();

		const horizonY = h * 0.66;
		
		const lowerValleyX = w * 0.42;
		const lowerValleyY = horizonY;

		const sky = new Konva.Line({
			points: [
				0, 0,
				w, 0,
				w, horizonY,
				w * 0.78, horizonY,
				w * 0.60, h * 0.62,
				lowerValleyX, lowerValleyY,
				w * 0.18, h * 0.63,
				0, h * 0.74
			],
			fill: "#dfeaf1",
			closed: true,
			strokeEnabled: false,
			tension: 0.2,
		});

		const path = new Konva.Line({
			points: [
				lowerValleyX - (w * 0.015), lowerValleyY, 
				lowerValleyX + (w * 0.015), lowerValleyY,

				w * 0.68, h * 0.75, 

				w * 0.40, h * 0.85,

				w * 0.70, h,
				w * 0.30, h,

				w * 0.32, h * 0.85,
				w * 0.60, h * 0.75,
			],
			fill: "#ead99b",
			closed: true,
			strokeEnabled: false,
			tension: 0.12,
		});

		this.backgroundLayer.add(sky);
		this.backgroundLayer.add(path);
	}

	createCloud(xRatio: number, yRatio: number, scale = 1, opacity = 1) {
		const w = this.w();
		const h = this.h();

		const group = new Konva.Group({
			x: w * xRatio,
			y: h * yRatio,
			scaleX: scale,
			scaleY: scale,
			opacity,
		});

		const r = Math.min(w, h) * 0.035;

		// back layer
		group.add(new Konva.Ellipse({
			x: 0,
			y: r * 0.50,
			radiusX: r * 1.10,
			radiusY: r * 0.70,
			fill: "#f6f6f6",
			opacity: 0.95,
		}));
		group.add(new Konva.Ellipse({
			x: r * 0.95,
			y: r * 0.18,
			radiusX: r * 1.00,
			radiusY: r * 0.76,
			fill: "#f6f6f6",
			opacity: 0.95,
		}));
		group.add(new Konva.Ellipse({
			x: r * 2.00,
			y: r * 0.35,
			radiusX: r * 1.15,
			radiusY: r * 0.82,
			fill: "#f6f6f6",
			opacity: 0.95,
		}));
		group.add(new Konva.Ellipse({
			x: r * 3.00,
			y: r * 0.48,
			radiusX: r * 1.00,
			radiusY: r * 0.68,
			fill: "#f6f6f6",
			opacity: 0.95,
		}));

		// front puff layer for softer overlap
		group.add(new Konva.Ellipse({
			x: r * 0.55,
			y: r * 0.42,
			radiusX: r * 0.82,
			radiusY: r * 0.54,
			fill: "#fbfbfb",
			opacity: 0.98,
		}));
		group.add(new Konva.Ellipse({
			x: r * 1.55,
			y: r * 0.28,
			radiusX: r * 0.92,
			radiusY: r * 0.58,
			fill: "#fbfbfb",
			opacity: 0.98,
		}));
		group.add(new Konva.Ellipse({
			x: r * 2.55,
			y: r * 0.42,
			radiusX: r * 0.86,
			radiusY: r * 0.54,
			fill: "#fbfbfb",
			opacity: 0.98,
		}));

		return group;
	}

	startCloudParallax() {
		this.stopCloudParallax();

		if (this.cloudGroups.length === 0) return;

		const baseXPositions = this.cloudGroups.map((g) => g.x());
		const amplitudes = this.cloudGroups.map((_, i) => this.w() * (0.004 + i * 0.0015));
		const speeds = this.cloudGroups.map((_, i) => 0.08 + i * 0.03);

		this.cloudAnimation = new Konva.Animation((frame) => {
			if (!frame) return;
			const t = frame.time / 1000;

			this.cloudGroups.forEach((group, i) => {
				group.x(baseXPositions[i] + Math.sin(t * speeds[i]) * amplitudes[i]);
			});
		}, this.decorLayer);

		this.cloudAnimation.start();
	}

	stopCloudParallax() {
		if (this.cloudAnimation) {
			this.cloudAnimation.stop();
			this.cloudAnimation = null;
		}
	}

	createTree(xRatio: number, yRatio: number, scaleRatio = 1, color = "#6aaa3f") {
		const w = this.w();
		const h = this.h();
		const unit = Math.min(w, h) * 0.08;

		const group = new Konva.Group({
			x: w * xRatio,
			y: h * yRatio,
			scaleX: scaleRatio,
			scaleY: scaleRatio,
		});

		const trunk = new Konva.Rect({
			x: -unit * 0.12,
			y: unit * 0.8,
			width: unit * 0.24,
			height: unit * 0.95,
			fill: "#6c5a2b",
		});

		const tri1 = new Konva.Line({
			points: [0, 0, -unit * 0.75, unit * 0.95, unit * 0.75, unit * 0.95],
			fill: color,
			closed: true,
			strokeEnabled: false,
		});

		const tri2 = new Konva.Line({
			points: [0, unit * 0.45, -unit * 0.62, unit * 1.28, unit * 0.62, unit * 1.28],
			fill: color,
			closed: true,
			strokeEnabled: false,
		});

		group.add(trunk, tri1, tri2);
		return group;
	}

	renderLobbyDecorations() {
		this.cloudGroups = [];

		const topLeftCloud = this.createCloud(0.055, 0.20, 2.65, 0.98);
		const midLeftCloud = this.createCloud(0.245, 0.405, 1.35, 0.98);
		const topRightCloud = this.createCloud(0.735, 0.295, 3.35, 0.95);

		this.cloudGroups.push(topLeftCloud, midLeftCloud, topRightCloud);

		this.decorLayer.add(topLeftCloud);
		this.decorLayer.add(midLeftCloud);
		this.decorLayer.add(topRightCloud);

		this.decorLayer.add(this.createTree(0.11, 0.29, 3.60, "#69aa3e"));
		this.decorLayer.add(this.createTree(0.19, 0.39, 2.75, "#4d872c"));
		this.decorLayer.add(this.createTree(0.825, 0.31, 3.45, "#69aa3e"));

		this.startCloudParallax();
	}

	renderLobbyBanner() {
		const w = this.w();
		const h = this.h();

		const bannerWidth = w * 0.52;
		const bannerHeight = h * 0.13;
		const x = (w - bannerWidth) / 2;
		const y = h * 0.05;

		const bannerGroup = new Konva.Group({ x, y });

		const banner = new Konva.Rect({
			width: bannerWidth,
			height: bannerHeight,
			fill: "#edd99b",
			stroke: "#6b5a2d",
			strokeWidth: Math.max(2, w * 0.0018),
			cornerRadius: 12,
			shadowColor: "black",
			shadowBlur: 8,
			shadowOpacity: 0.18,
		});

		const title = new Konva.Text({
			x: 0,
			y: bannerHeight * 0.16,
			width: bannerWidth,
			align: "center",
			text: "Gnome Party",
			fontFamily: "Mystorica",
			fontSize: Math.min(w, h) * 0.07,
			fill: "#000",
		});

		bannerGroup.add(banner, title);
		this.lobbyBannerGroup = bannerGroup;
		this.uiLayer.add(bannerGroup);
	}

	renderLobbyRoomCodeCard() {
		const w = this.w();
		const h = this.h();

		const cardWidth = w * 0.22;
		const cardHeight = h * 0.18;
		const group = new Konva.Group({
			x: (w - cardWidth) / 2,
			y: h * 0.315,
		});

		const roomCodeBox = new Konva.Rect({
			width: cardWidth,
			height: cardHeight,
			fill: "#dfeaf1",
			stroke: "#c7d6de",
			strokeWidth: Math.max(2, w * 0.002),
			shadowColor: "black",
			shadowBlur: 8,
			shadowOpacity: 0.10,
		});

		const roomCodeLabel = new Konva.Text({
			x: 0,
			y: cardHeight * 0.12,
			width: cardWidth,
			align: "center",
			text: "Room Code",
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.022,
			fill: "#111",
		});

		const roomCodeValue = new Konva.Text({
			x: 0,
			y: cardHeight * 0.38,
			width: cardWidth,
			align: "center",
			text: this.inviteCode || "------",
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.055,
			fill: "#000",
		});

		group.add(roomCodeBox, roomCodeLabel, roomCodeValue);
		this.lobbyRoomCodeGroup = group;
		this.uiLayer.add(group);
	}

	renderLobbyPlayerCount() {
		const w = this.w();
		const h = this.h();

		const badgeWidth = w * 0.16;
		const badgeHeight = h * 0.06;

		const group = new Konva.Group({
			x: w * 0.055,
			y: h * 0.48,
		});

		const badge = new Konva.Rect({
			width: badgeWidth,
			height: badgeHeight,
			fill: "#9fc765",
			stroke: "#58752e",
			strokeWidth: 2,
			shadowColor: "black",
			shadowBlur: 10,
			shadowOpacity: 0.5,
		});

		const text = new Konva.Text({
			width: badgeWidth,
			height: badgeHeight,
			text: `Players (${this.readyPlayers.length}/6)`,
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.022,
			fill: "#111",
			align: "center",
			shadowColor: "white",
			shadowBlur: 5,
			shadowOpacity: 0.8,
			verticalAlign: "middle", // Ensures vertical centering
		});

		group.add(badge, text);
		this.lobbyPlayerCountGroup = group;
		this.uiLayer.add(group);
	}

	getLobbyPlayerPositions(count: number) {
		const w = this.w();
		const h = this.h();

		if (count <= 0) {
			return [];
		}

		switch (count) {
			case 1:
				return [{ x: w * 0.50, y: h * 0.94 }];

			case 2:
				return [
					{ x: w * 0.42, y: h * 0.94 },
					{ x: w * 0.58, y: h * 0.94 },
				];

			case 3:
				return [
					{ x: w * 0.34, y: h * 0.94 },
					{ x: w * 0.50, y: h * 0.86 },
					{ x: w * 0.66, y: h * 0.94 },
				];

			default:
				return [
					{ x: w * 0.20, y: h * 0.94 },
					{ x: w * 0.34, y: h * 0.86 },
					{ x: w * 0.46, y: h * 0.94 },
					{ x: w * 0.58, y: h * 0.86 },
					{ x: w * 0.70, y: h * 0.94 },
					{ x: w * 0.82, y: h * 0.86 },
				].slice(0, count);
		}
	}

	renderLobbyPlayers() {
		this.lobbyPlayerPuppets.forEach((puppet) => puppet.destroy());
		this.lobbyPlayerNames.forEach((nameText) => nameText.destroy());
		this.lobbyPlayerShadows.forEach((shadow) => shadow.destroy());

		this.lobbyPlayerPuppets.clear();
		this.lobbyPlayerNames.clear();
		this.lobbyPlayerShadows.clear();

		const positions = this.getLobbyPlayerPositions(this.readyPlayers.length);

		const lobbyPuppetScale = this.h() * 0.00062;

		for (let i = 0; i < positions.length; i++) {
			const player = this.readyPlayers[i];
			const pos = positions[i];

			if (!player || !pos) continue;

			const characterId = player.Id;

			const puppet = new GnomePuppet(player.CharacterType);
			puppet.scale({ x: lobbyPuppetScale, y: lobbyPuppetScale });

			// feet position
			puppet.position({
				x: pos.x,
				y: pos.y,
			});

			const shadow = new Konva.Ellipse({
				x: pos.x,
				y: pos.y - 120 * lobbyPuppetScale,
				radiusX: 170 * lobbyPuppetScale,
				radiusY: 60 * lobbyPuppetScale,
				fill: "#4f8f3a",
				opacity: 0.82,
			});

			const nameWidth = 260 * lobbyPuppetScale;
			const nameText = new Konva.Text({
				x: pos.x - nameWidth / 2,
				y: pos.y - 8,
				width: nameWidth,
				align: "center",
				text: player.Name ?? "Player",	// player.Name is not correct, it currently shows class not the entered name
				fontFamily: "Amasis MT Pro",
				fontSize: 48 * lobbyPuppetScale,
				fill: "#111",
				shadowColor: "white",
				shadowBlur: 10,
				shadowOpacity: 1,
			});

			this.lobbyLayer.add(shadow);
			this.lobbyLayer.add(puppet);
			this.uiLayer.add(nameText);

			this.lobbyPlayerShadows.set(characterId, shadow);
			this.lobbyPlayerPuppets.set(characterId, puppet);
			this.lobbyPlayerNames.set(characterId, nameText);
		}
	}

	renderLobbyStartButton() {
		const w = this.w();
		const h = this.h();

		const buttonWidth = w * 0.12;
		const buttonHeight = h * 0.07;

		const isDisabled = this.hasStartButtonBeenPressed || this.readyPlayers.length < 1;

		const group = new Konva.Group({
			x: w * 0.84,
			y: h * 0.89,
		});

		const button = new Konva.Rect({
			width: buttonWidth,
			height: buttonHeight,
			fill: isDisabled ? "#8a8a8a" : "#9fc765",
			stroke: isDisabled ? "#5a5a5a" : "#58752e",
			strokeWidth: 2,
			shadowColor: "black",
			shadowBlur: 5,
			shadowOpacity: 0.3,
		});

		const text = new Konva.Text({
			width: buttonWidth,
			height: buttonHeight,
			text: this.hasStartButtonBeenPressed ? "STARTING" : "START",
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.035,
			fill: isDisabled ? "#333" : "#111",
			align: "center",
			verticalAlign: "middle",
			shadowColor: "white",
			shadowBlur: isDisabled ? 0 : 5,
			shadowOpacity: isDisabled ? 0 : 0.8,
		});

		group.add(button, text);

		if (!isDisabled) {
			group.on("click", () => {
				if (this.hasStartButtonBeenPressed) return;

				this.hasStartButtonBeenPressed = true;

				this.socket.send(JSON.stringify({
					route: "start-campaign",
				}));

				this.renderHostLobby();
			});

			group.on("mouseenter", () => {
				document.body.style.cursor = "pointer";
			});

			group.on("mouseleave", () => {
				document.body.style.cursor = "default";
			});
		}

		this.lobbyStartButtonGroup = group;
		this.uiLayer.add(group);
	}

	clearLobbyScene() {
		this.stopCloudParallax();
		this.cloudGroups = [];

		this.backgroundLayer.destroyChildren();
		this.decorLayer.destroyChildren();

		this.lobbyPlayerPuppets.forEach((puppet) => puppet.destroy());
		this.lobbyPlayerNames.forEach((nameText) => nameText.destroy());
		this.lobbyPlayerShadows.forEach((shadow) => shadow.destroy());

		this.lobbyPlayerPuppets.clear();
		this.lobbyPlayerNames.clear();
		this.lobbyPlayerShadows.clear();

		this.lobbyLayer.destroyChildren();

		if (this.lobbyRoomCodeGroup) {
			this.lobbyRoomCodeGroup.destroy();
			this.lobbyRoomCodeGroup = null;
		}
		if (this.lobbyPlayerCountGroup) {
			this.lobbyPlayerCountGroup.destroy();
			this.lobbyPlayerCountGroup = null;
		}
		if (this.lobbyBannerGroup) {
			this.lobbyBannerGroup.destroy();
			this.lobbyBannerGroup = null;
		}
		if (this.lobbyStartButtonGroup) {
			this.lobbyStartButtonGroup.destroy();
			this.lobbyStartButtonGroup = null;
		}

		this.uiLayer.destroyChildren();

		this.backgroundLayer.draw();
		this.decorLayer.draw();
		this.lobbyLayer.draw();
		this.uiLayer.draw();
	}

	clearCombatScene() {
		this.playerVisualComponents.forEach((v) => {
			v.puppet.destroy();
			v.healthbar.destroy();
		});
		this.enemyVisualComponents.forEach((v) => {
			v.puppet.destroy();
			v.healthbar.destroy();
		});

		this.playerVisualComponents.clear();
		this.enemyVisualComponents.clear();

		this.combatLayer.destroyChildren();
		this.combatLayer.draw();
		this.uiLayer.draw();
	}
	
	markCharacterDefeated(characterId: string) {
		const enemy = this.enemyVisualComponents.get(characterId);
		const player = this.playerVisualComponents.get(characterId);
		const visual = enemy ?? player;

		if (!visual) return;

		visual.puppet.rotation(90);
		visual.puppet.opacity(0.65);
		visual.puppet.x(visual.puppet.x() - this.w() * 0.03);
	
		visual.healthbar.changeHealth(0);
		visual.healthbar.opacity(0.45);

		this.combatLayer.draw();
		this.uiLayer.draw();
	}

	createEnemyPuppet(type:string) {
		switch (type)
			{
				case "Skeleton":
					return new SkeletonPuppet();
					break;
				case "Goblin Archer":
					return new GoblinArcherPuppet();
					break;
				case "Forest Sprite":
					return new ForestSpritePuppet();
					break;
				case "Cave Bat":
					return new CaveBatPuppet();
					break;
				case "Gnombie Brute":
					return new GnombieBrutePuppet();
					break;
				case "Gnome Eater":
					return new GnomeEaterPuppet();
					break;
				case "Necrognomancer":
					return new NecrognomancerPuppet();
					break;
				default:
					console.log("Default triggered because it was", type);
					return new SkeletonPuppet();
			}
	}

	updateAllHealth(state:GameState) {
		let seq = new AnimationSequence();

		for (let player of state.PlayerCharacters)
		{
			let pvc = this.playerVisualComponents.get(player.Id);
			seq.steps.push(new FunctionStep(() => {pvc?.healthbar.changeHealth(player.Health);}));
		}
		for (let enemy of state.EnemyCharacters)
		{
			let evc = this.enemyVisualComponents.get(enemy.Id);
			seq.steps.push(new FunctionStep(() => {evc?.healthbar.changeHealth(enemy.Health);}));
		}

		seq.steps.push(new AnimationPause(1000));

		return seq;
	}

	renderCombatEndOverlay(titleText: string, buttonText: string) {
		const w = this.w();
		const h = this.h();

		if (this.combatEndOverlayGroup) {
			this.combatEndOverlayGroup.destroy();
			this.combatEndOverlayGroup = null;
		}

		const group = new Konva.Group({
			x: 0,
			y: 0,
		});

		const dimmer = new Konva.Rect({
			x: 0,
			y: 0,
			width: w,
			height: h,
			fill: "black",
			opacity: 0.15,
		});

		const bannerWidth = w * 0.56;
		const bannerHeight = h * 0.25;
		const bannerX = (w - bannerWidth) / 2;
		const bannerY = h * 0.35;

		const banner = new Konva.Rect({
			x: bannerX,
			y: bannerY,
			width: bannerWidth,
			height: bannerHeight,
			fill: "#edd99b",
			stroke: "#6b5a2d",
			strokeWidth: 3,
			cornerRadius: 14,
			shadowColor: "black",
			shadowBlur: 10,
			shadowOpacity: 0.35,
		});

		const title = new Konva.Text({
			x: bannerX,
			y: bannerY + bannerHeight * 0.18,
			width: bannerWidth,
			height: bannerHeight * 0.64,
			text: titleText,
			fontFamily: "Mystorica",
			fontSize: Math.min(w, h) * 0.12,
			fill: "#000",
			align: "center",
			verticalAlign: "middle",
		});

		const buttonWidth = w * 0.14;
		const buttonHeight = h * 0.085;
		const buttonX = (w - buttonWidth) / 2;
		const buttonY = h * 0.76;

		const button = new Konva.Rect({
			x: buttonX,
			y: buttonY,
			width: buttonWidth,
			height: buttonHeight,
			fill: "#9fc765",
			stroke: "#58752e",
			strokeWidth: 3,
			shadowColor: "#6f8f39",
			shadowBlur: 5,
			shadowOpacity: 1,
		});

		const buttonLabel = new Konva.Text({
			x: buttonX,
			y: buttonY,
			width: buttonWidth,
			height: buttonHeight,
			text: buttonText,
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.045,
			fill: "#111",
			align: "center",
			verticalAlign: "middle",
			shadowColor: "white",
			shadowBlur: 5,
			shadowOpacity: 0.85,
		});

		const buttonGroup = new Konva.Group();
		buttonGroup.add(button, buttonLabel);

		buttonGroup.on("mouseenter", () => {
			document.body.style.cursor = "pointer";
		});

		buttonGroup.on("mouseleave", () => {
			document.body.style.cursor = "default";
		});

		buttonGroup.on("click", () => {
			if (buttonText === "Retry") {
				// TODO: send restart campaign message/handle retry logic without reloading the page
				window.location.reload();
				return;
			}

			this.combatEndOverlayGroup?.destroy();
			this.combatEndOverlayGroup = null;
			this.uiLayer.draw();
		});

		group.add(dimmer, banner, title, buttonGroup);

		this.combatEndOverlayGroup = group;
		this.uiLayer.add(group);
		this.uiLayer.draw();
	}
}



export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}