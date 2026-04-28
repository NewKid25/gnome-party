import { Group } from "konva/lib/Group";
import FunctionStep from "../FunctionStep";
import GnomePuppet from "../puppets/GnomePuppet";
import HealthBar from "../HealthBar";
import AnimationStep from "../interfaces/AnimationStep";
import Puppet from "../interfaces/Puppet";
import { GameState, TurnStep } from "../interfaces/TurnStep";
import ViewManager, { CharacterVisualComponents } from "../ViewManager";
import LeapAnimation from "../animations/LeapAnimation";
import { convertToObject } from "typescript";
import SimultaneousAnimation from "../SimultaneousAnimation";
import ProjectileAnimation from "../animations/ProjectileAnimation";
import Konva from "konva";
import TextPopupAnimation from "../animations/TextPopupAnimation";
import AnimationSequence from "../AnimationSequence";
import { Vector2d } from "konva/lib/types";
import SpawnIconAnimation from "../animations/SpawnIconAnimation";
import AnimationPause from "../AnimationPause";

enum SONG_TYPE {
	Soothing,
	Inspiring,
	Frightening
}

export default
class PowerChordAnimation implements AnimationStep
{
	anim:SimultaneousAnimation
	play() 
	{
		this.anim.onFinish = this.onFinish
		this.anim.play();
	}
	onFinish: Function | undefined;

	constructor(step:TurnStep, vm:ViewManager) 
	{
		this.anim = new SimultaneousAnimation();

		const msgToSongType = {
			"Soothing Song": SONG_TYPE.Soothing,
			"Inspiring Song": SONG_TYPE.Inspiring,
			"Frightening Song": SONG_TYPE.Frightening,
		}

		console.log(step.Request.SourceCharacterId, step.Request.TargetCharacterId)

		// @ts-ignore
		let songType:int = msgToSongType[step.Events.filter(e => e.event == "power_cord_used")[0].params["AmplifiedSong"]]

		// @ts-ignore
		let popupText: string = ({
				0: "HEALED",
				1: "DMG UP",
				2: "STUNNED",
			}[songType]);

		let player:Puppet | undefined = vm.playerVisualComponents.get(step.Request.SourceCharacterId)?.puppet;

		let targets:CharacterVisualComponents[] = []

		if (songType == SONG_TYPE.Frightening) {
			vm.enemyVisualComponents.forEach((evc, id) => {
				let targetPuppet = evc.puppet;
				let targetHealthBar = evc.healthbar;
				let newHealth = step.GameState.EnemyCharacters.find((v, i, o) => v.Id == id)?.Health ?? 0;
				
				const noteImg = new Image();
				switch (songType){
					case SONG_TYPE.Soothing:
						noteImg.src = "/img/SoothingSong.svg";
						break;
					case SONG_TYPE.Inspiring:
						noteImg.src = "/img/InspiringSong.svg";
						break;
					case SONG_TYPE.Frightening:
						noteImg.src = "/img/FrighteningSong.svg";
						break;
				}
				let noteIcon = new Konva.Image({
					image: noteImg,
					scale: {x: 0.2, y: 0.2},
					opacity: 0.0
				});

				noteIcon.position({x: targetPuppet.position().x, y: targetPuppet.position().y})

				this.anim.steps.push(new AnimationSequence([
					new AnimationPause(0.1),
					new SimultaneousAnimation([
						new SpawnIconAnimation({node: noteIcon,  position: {x: () => targetPuppet.position().x, y: () => targetPuppet.position().y - noteIcon.getHeight() * 0.4 / 2}, layer: vm.uiLayer}),
						new TextPopupAnimation({text: popupText,  position: targetPuppet.position(), layer: vm.uiLayer}),
						new FunctionStep(() => {targetHealthBar.changeHealth(newHealth)}),
					])
				]))
			})
		} else {
			vm.playerVisualComponents.forEach((pvc, id) => {
				let targetPuppet = pvc.puppet;
				let targetHealthBar = pvc.healthbar;
				let newHealth = step.GameState.PlayerCharacters.find((v, i, o) => v.Id == id)?.Health ?? 0;
				
				const noteImg = new Image();
				switch (songType){
					case SONG_TYPE.Soothing:
						noteImg.src = "/img/SoothingSong.svg";
						break;
					case SONG_TYPE.Inspiring:
						noteImg.src = "/img/InspiringSong.svg";
						break;
					case SONG_TYPE.Frightening:
						noteImg.src = "/img/FrighteningSong.svg";
						break;
				}
				let noteIcon = new Konva.Image({
					image: noteImg,
					scale: {x: 0.2, y: 0.2},
					opacity: 0.0,
				});

				console.log("Height: ", noteImg.height)

				noteIcon.position({x: targetPuppet.position().x, y: targetPuppet.position().y})

				this.anim.steps.push(new AnimationSequence([
					new AnimationPause(0.1),
					new SimultaneousAnimation([
						new SpawnIconAnimation({node: noteIcon,  position: {x: () => targetPuppet.position().x, y: () => targetPuppet.position().y - noteIcon.getHeight() * 0.4 / 2}, layer: vm.uiLayer}),
						new TextPopupAnimation({text: popupText,  position: targetPuppet.position(), layer: vm.uiLayer}),
						new FunctionStep(() => {targetHealthBar.changeHealth(newHealth)}),
					])
				]))
			})
		}
	}
}