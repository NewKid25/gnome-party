import Konva from "konva";
import { Tween } from "konva/lib/Tween";
import AnimationSequence from "./AnimationSequence";
import SimultaneousAnimation from "./SimultaneousAnimation";
import TweenFromCurrent from "./TweenFromCurrent";
import LeapAnimation from "./animations/LeapAnimation";
import AnimationPause from "./AnimationPause";
import GnomePuppet from "./GnomePuppet";
import HealthBar from "./HealthBar";
import FunctionStep from "./FunctionStep";
import AnimationStep from "./interfaces/AnimationStep";
import { TurnStep } from "./interfaces/TurnStep";
import Puppet from "./interfaces/Puppet";
import SlashAnimation from "./animations/SlashAnimation";
import BoneSlashAnimation from "./animations/BoneSlashAnimation";
import { useSocketData } from "../../participant-view/stores/socketData";
import SkeletonPuppet from "./SkeletonPuppet";

export default
class ViewManager {

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

	lobbyPlayerPuppets:Map<string, GnomePuppet> = new Map();
	lobbyPlayerNames:Map<string, Konva.Text> = new Map();
	lobbyPlayerShadows:Map<string, Konva.Ellipse> = new Map();

	lobbyRoomCodeGroup:Konva.Group | null = null;
	lobbyPlayerCountGroup:Konva.Group | null = null;
	lobbyBannerGroup:Konva.Group | null = null;
	lobbyStartButtonGroup:Konva.Group | null = null;

	cloudGroups:Konva.Group[] = [];
	cloudAnimation:Konva.Animation | null = null;

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
		// Load player characters
		/*
		var playerCharacters: Object[] = [
			{},
			{},
			{},
			{},
			{},
			{}
		]
		*/

		var playerCharacters = gameState.PlayerCharacters;

		for (let i = 0; i < playerCharacters.length; i++) {
			// Create GnomePuppet
			let puppet:GnomePuppet = new GnomePuppet();
			var x = this.w() * 0.18;
			var y = (i + 1) * this.stage.height() / (playerCharacters.length + 1);
			// const y = ((i + 1) / (playerCharacters.length + 1)) * this.h() * 0.78 + this.h() * 0.12;

			puppet.scale({x: 0.55, y: 0.55});
			puppet.position({x, y});

			this.combatLayer.add(puppet);

			// Create healthbar
			// let healthbar:HealthBar = new HealthBar(playerCharacters[i].MaxHealth, {x: 30, y: puppet.height() / 2})
			// healthbar.x(puppet.x() - puppet.width() /2 - 50);
			// healthbar.y(puppet.y() - puppet.height() / 3.5);

			let healthbar = new HealthBar(playerCharacters[i].MaxHealth, {x: this.w() * 0.025, y: puppet.height() / 2});

			healthbar.x(puppet.x() - this.w() * 0.09);
			healthbar.y(puppet.y() - this.h() * 0.07);

			this.uiLayer.add(healthbar);

			this.playerVisualComponents.set(playerCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}

		// Load enemy characters
		/*
		var enemyCharacters: Object[] = [
			{}
		]
		*/
		var enemyCharacters = gameState.EnemyCharacters;

		for (let i = 0; i < enemyCharacters.length; i++) {
			// Create puppet of corresponding enemy (using GnomePuppet as placeholder)
			let puppet:SkeletonPuppet = new SkeletonPuppet();

			// puppet.x(this.stage.width() - 300);
			// puppet.y((i + 1) * this.stage.height() / (enemyCharacters.length + 1));
			const x = this.w() * 0.82;
			const y = ((i + 1) / (enemyCharacters.length + 1)) * this.h() * 0.78 + this.h() * 0.12;

			puppet.position({ x, y });

			this.combatLayer.add(puppet);

			// Create healthbar
			// let healthbar:HealthBar = new HealthBar(enemyCharacters[i].MaxHealth, {x: 30, y: puppet.height() / 2})
			// healthbar.x(puppet.x() + puppet.width() /2 + 20);
			// healthbar.y(puppet.y() - puppet.height() / 3.5);


			const healthbar = new HealthBar(enemyCharacters[i].MaxHealth, {
				x: this.w() * 0.025,
				y: puppet.height() / 2,
			});
			healthbar.x(puppet.x() + this.w() * 0.03);
			healthbar.y(puppet.y() - this.h() * 0.07);

			this.uiLayer.add(healthbar);

			this.enemyVisualComponents.set(enemyCharacters[i].Id, {puppet: puppet, healthbar: healthbar});
		}

		this.combatLayer.draw();
		this.uiLayer.draw();
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
			this.isCombatStarted = true;
			console.log("Campaign started:", msg.Message);
			this.socket.send(JSON.stringify({
				route: "begin-combat-encounter",
				GameSessionId: this.socketStore.gameSessionId
			}))
		}

		if (msg.Subject == "begin-combat-encounter")
		{
			this.clearLobbyScene();
			this.socketStore.encounterId = msg.Message.EncounterId;
			this.loadEncounter(msg.Message.GameState);
		}
		if (msg.Subject == "action-handler")
		{
			this.processTurn(msg.Message);
		}
	}

	processTurn(turn:TurnStep[])
	{
		let animations:AnimationStep[] = [];
		// let finalStep:TurnStep|undefined;
		for (let step of turn)
		{
			let animation:AnimationStep | undefined = this.instantiateActionAnimation(step);
			if (animation) animations.push(animation);
			// finalStep = step;
		}

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

		let sampleSteps:TurnStep[] = 
		[
			{
				"Request": {
				"EncounterId": "50b8c0cf-e032-4625-ba07-dad08231081b",
				"TargetCharacterId": "test-enemy",
				"SourceCharacterId": "0",
				"Action": "Slash"
				},
				"GameState": {
				"PlayerCharacters": [
					{
					"Id": "0",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					},
					{
					"Id": "1",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					}
				],
				"EnemyCharacters": [
					{
					"Id": "test-enemy",
					"Name": "skeleton",
					"Health": 12,
					"MaxHealth": 10,
					"ActionsDescriptions": [
						{
						"Name": "punch",
						"Description": "A weak punch"
						}
					]
					}
				]
				},
				"Events": []

			},
			{
				"Request": {
				"EncounterId": "50b8c0cf-e032-4625-ba07-dad08231081b",
				"TargetCharacterId": "test-enemy",
				"SourceCharacterId": "1",
				"Action": "Slash"
				},
				"GameState": {
				"PlayerCharacters": [
					{
					"Id": "0",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					},
					{
					"Id": "1",
					"Name": "Default Name",
					"Health": 1,
					"MaxHealth": 1,
					"ActionsDescriptions": [
						{
						"Name": "Slash",
						"Description": "default_action_description"
						},
						{
						"Name": "Block",
						"Description": "default_action_description"
						}
					]
					}
				],
				"EnemyCharacters": [
					{
					"Id": "test-enemy",
					"Name": "skeleton",
					"Health": 8,
					"MaxHealth": 10,
					"ActionsDescriptions": [
						{
						"Name": "punch",
						"Description": "A weak punch"
						}
					]
					}
				]
				},
				"Events": []
			}
		];

		this.processTurn(sampleSteps);
	}

	instantiateActionAnimation(step:TurnStep)
	{
		let actionName = step.Request.Action;
		switch(actionName) {
			case "Slash":
				return new SlashAnimation(step, this);
			case "Bone Slash":
				return new BoneSlashAnimation(step, this);
		}
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

		const sky = new Konva.Line({
			points: [
				0, 0,
				w, 0,
				w, h * 0.66,
				w * 0.78, h * 0.66,
				w * 0.60, h * 0.62,
				w * 0.42, h * 0.66,
				w * 0.18, h * 0.63,
				0, h * 0.74
			],
			fill: "#dfeaf1",
			closed: true,
			strokeEnabled: false,
		});

		const path = new Konva.Line({
			points: [
				w * 0.52, h * 0.66,   // start at horizon
				w * 0.60, h * 0.72,
				w * 0.53, h * 0.80,
				w * 0.58, h,
				w * 0.42, h,
				w * 0.45, h * 0.84,
				w * 0.40, h * 0.76,
				w * 0.46, h * 0.70
			],
			fill: "#ead99b",
			closed: true,
			strokeEnabled: false,
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

		// Clouds positioned to match the mockup
		const topLeftCloud = this.createCloud(0.06, 0.055, 1.45, 0.98);
		const midLeftCloud = this.createCloud(0.245, 0.175, 0.72, 0.98);
		const topRightCloud = this.createCloud(0.73, 0.045, 2.15, 0.98);

		this.cloudGroups.push(topLeftCloud, midLeftCloud, topRightCloud);

		this.decorLayer.add(topLeftCloud);
		this.decorLayer.add(midLeftCloud);
		this.decorLayer.add(topRightCloud);

		// Trees positioned to match mockup more closely
		this.decorLayer.add(this.createTree(0.105, 0.49, 1.95, "#69aa3e"));
		this.decorLayer.add(this.createTree(0.185, 0.585, 1.35, "#4d872c"));
		this.decorLayer.add(this.createTree(0.825, 0.565, 1.80, "#69aa3e"));

		this.startCloudParallax();
	}

	renderLobbyBanner() {
		const w = this.w();
		const h = this.h();

		const bannerWidth = w * 0.52;
		const bannerHeight = h * 0.13;
		const x = (w - bannerWidth) / 2;
		const y = h * 0.02;

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
			y: h * 0.24,
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
			fill: "#8eb63e",
			cornerRadius: 4,
			shadowColor: "white",
			shadowBlur: 10,
			shadowOpacity: 0.75,
		});

		const text = new Konva.Text({
			x: 0,
			y: badgeHeight * 0.18,
			width: badgeWidth,
			align: "center",
			text: `Players (${this.readyPlayers.length}/6)`,
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.022,
			fill: "#111",
		});

		group.add(badge, text);
		this.lobbyPlayerCountGroup = group;
		this.uiLayer.add(group);
	}

	getLobbyPlayerPositions(count: number) {
		const w = this.w();
		const h = this.h();

		switch (count) {
			case 1:
				return [
					{ x: w * 0.50, y: h * 0.84 },
				];

			case 2:
				return [
					{ x: w * 0.42, y: h * 0.84 },
					{ x: w * 0.58, y: h * 0.84 },
				];

			case 3:
				return [
					{ x: w * 0.34, y: h * 0.84 },
					{ x: w * 0.50, y: h * 0.76 },
					{ x: w * 0.66, y: h * 0.84 },
				];

			case 4:
				return [
					{ x: w * 0.28, y: h * 0.84 },
					{ x: w * 0.43, y: h * 0.76 },
					{ x: w * 0.57, y: h * 0.76 },
					{ x: w * 0.72, y: h * 0.84 },
				];

			case 5:
				return [
					{ x: w * 0.22, y: h * 0.84 },
					{ x: w * 0.36, y: h * 0.76 },
					{ x: w * 0.50, y: h * 0.84 },
					{ x: w * 0.64, y: h * 0.76 },
					{ x: w * 0.78, y: h * 0.84 },
				];

			default:
				return [
					{ x: w * 0.20, y: h * 0.84 },
					{ x: w * 0.34, y: h * 0.76 },
					{ x: w * 0.46, y: h * 0.84 },
					{ x: w * 0.58, y: h * 0.76 },
					{ x: w * 0.70, y: h * 0.84 },
					{ x: w * 0.82, y: h * 0.76 },
				];
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

	const puppetScale = this.h() * 0.00045;

	for (let i = 0; i < positions.length; i++) {
		const player = this.readyPlayers[i];
		const pos = positions[i];
		const characterId = player.Id;

		const puppet = new GnomePuppet();
		puppet.scale({ x: puppetScale, y: puppetScale });

		const FOOT_OFFSET = 45 * puppetScale;

		puppet.position({
			x: pos.x,
			y: pos.y + FOOT_OFFSET
		});

		const shadow = new Konva.Ellipse({
			x: puppet.x(),
			y: puppet.y() + 8 * puppetScale,
			radiusX: 95 * puppetScale,
			radiusY: 32 * puppetScale,
			fill: "#4f8f3a",
			opacity: 0.82,
		});

		const nameWidth = 220 * puppetScale;
		const nameText = new Konva.Text({
			x: puppet.x() - nameWidth / 2,
			y: puppet.y() + 28 * puppetScale,
			width: nameWidth,
			align: "center",
			text: player.Name ?? "Player",
			fontFamily: "Amasis MT Pro",
			fontSize: 42 * puppetScale,
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

		const group = new Konva.Group({
			x: w * 0.84,
			y: h * 0.89,
		});

		const button = new Konva.Rect({
			width: buttonWidth,
			height: buttonHeight,
			fill: "#9fc765",
			stroke: "#58752e",
			strokeWidth: Math.max(2, w * 0.002),
			shadowColor: "#58752e",
			shadowOffset: { x: buttonWidth * 0.07, y: buttonHeight * 0.18 },
			shadowOpacity: 0.45,
		});

		const text = new Konva.Text({
			x: 0,
			y: buttonHeight * 0.18,
			width: buttonWidth,
			align: "center",
			text: "START",
			fontFamily: "Amasis MT Pro",
			fontSize: Math.min(w, h) * 0.03,
			fill: "#111",
		});

		group.add(button, text);

		group.on("click", () => {
			this.socket.send(JSON.stringify({
				route: "start-campaign",
			}));
		});

		group.on("mouseenter", () => {
			document.body.style.cursor = "pointer";
		});

		group.on("mouseleave", () => {
			document.body.style.cursor = "default";
		});

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

}



export interface CharacterVisualComponents {
	healthbar:HealthBar
	puppet:Puppet
}