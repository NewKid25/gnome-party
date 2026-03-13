import Konva from "konva";
import { Vector2d } from "konva/lib/types";
import SimultaneousAnimation from "./SimultaneousAnimation";
import AnimationSequence from "./AnimationSequence";
import AnimationPause from "./AnimationPause";
import TweenFromCurrent from "./TweenFromCurrent";

export default
class HealthBar extends Konva.Group {
	private remainingHealth:number
	private maxHealth:number
	private boundingBox:Konva.Vector2d
	private backBar:Konva.Line
	private fillBar:Konva.Line
	private damageBar:Konva.Line

	static lerp = (a:number, b:number, amount:number) => (1 - amount) * a + amount * b;

	changeHealth(newHealth:number) {
		const DMG_DURATION:number = .1
		const LINGER_DURATION:number = .5
		const DRAIN_DURATION: number = .3

		let prevHealth = this.remainingHealth;
		let prevHealthRatio = prevHealth / this.maxHealth;
		this.remainingHealth = Math.max(newHealth, 0);
		let healthRatio = this.remainingHealth / this.maxHealth;

		let fillBarAnimation = new AnimationSequence([
			new TweenFromCurrent({
				node: this.fillBar,
				points: [0, this.boundingBox.y, 
					HealthBar.lerp(0, this.boundingBox.x * .25, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio), 
					HealthBar.lerp(this.boundingBox.x * .75, this.boundingBox.x, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio), 
					this.boundingBox.x * .75, this.boundingBox.y],
				duration: DMG_DURATION
			}),
			new AnimationPause(LINGER_DURATION * 1000),
			new AnimationPause(DRAIN_DURATION * 1000),
		]);

		let damageBarAnimation = new AnimationSequence([
			new TweenFromCurrent({
				node: this.damageBar,
				// BL, TL, TR, BR
				points: [HealthBar.lerp(0, this.boundingBox.x * .25, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio), 
					HealthBar.lerp(0, this.boundingBox.x * .25, prevHealthRatio), HealthBar.lerp(this.boundingBox.y, 0, prevHealthRatio), 
					HealthBar.lerp(this.boundingBox.x * .75, this.boundingBox.x, prevHealthRatio), HealthBar.lerp(this.boundingBox.y, 0, prevHealthRatio), 
					HealthBar.lerp(this.boundingBox.x * .75, this.boundingBox.x, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio),
				],
				duration: DMG_DURATION
			}),
			new AnimationPause(LINGER_DURATION * 1000),
			new TweenFromCurrent({
				node: this.damageBar,
				// BL, TL, TR, BR
				points: [HealthBar.lerp(0, this.boundingBox.x * .25, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio), 
					HealthBar.lerp(0, this.boundingBox.x * .25, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio), 
					HealthBar.lerp(this.boundingBox.x * .75, this.boundingBox.x, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio),
					HealthBar.lerp(this.boundingBox.x * .75, this.boundingBox.x, healthRatio), HealthBar.lerp(this.boundingBox.y, 0, healthRatio),
				],
				duration: DRAIN_DURATION
			})
		]);

		let chunkAnimation = new SimultaneousAnimation([
			fillBarAnimation,
			damageBarAnimation
		])

		chunkAnimation.play();
	}

	constructor(_maxHealth:number, _boundingBox:Konva.Vector2d) {
		super();
		this.maxHealth = _maxHealth;
		this.remainingHealth = _maxHealth;
		this.boundingBox = _boundingBox;

		this.backBar = new Konva.Line({
			points: [0, this.boundingBox.y, this.boundingBox.x * .25, 0, this.boundingBox.x, 0, this.boundingBox.x * .75, this.boundingBox.y],
			closed: true,
			fill: "#502020"
		});
		this.add(this.backBar);
		
		this.fillBar = new Konva.Line({
			points: [0, this.boundingBox.y, this.boundingBox.x * .25, 0, this.boundingBox.x, 0, this.boundingBox.x * .75, this.boundingBox.y],
			closed: true,
			fill: "#d04040"
		});
		this.add(this.fillBar);

		this.damageBar = new Konva.Line({
			closed: true,
			// BL, TL, TR, BR
			points: [this.boundingBox.x * .25, 0, this.boundingBox.x * .25, 0, this.boundingBox.x, 0, this.boundingBox.x, 0],
			fill: "#f0d040"
		})
		this.add(this.damageBar);
	}
}