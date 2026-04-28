import Konva from "konva";
import SimultaneousAnimation from "./SimultaneousAnimation";
import AnimationSequence from "./AnimationSequence";
import AnimationPause from "./AnimationPause";
import TweenFromCurrent from "./TweenFromCurrent";

export default
class HealthBar extends Konva.Group {
	private remainingHealth:number
	private maxHealth:number
	private boundingBox:Konva.Vector2d
	private backBar:Konva.Rect
	private fillBar:Konva.Rect
	private damageBar:Konva.Rect
	private border:Konva.Rect

	getHealth() {
		return this.remainingHealth;
	}

	private getBarWidth(health: number) {
		const ratio = Math.max(0, Math.min(1, health / this.maxHealth));
		return this.boundingBox.x * ratio;
	}

	changeHealth(newHealth:number) {
		const DMG_DURATION:number = .1
		const LINGER_DURATION:number = .5
		const DRAIN_DURATION: number = .3

		let prevHealth = this.remainingHealth;
		this.remainingHealth = Math.max(newHealth, 0);
		let newFillWidth = this.getBarWidth(this.remainingHealth);
		let prevFillWidth = this.getBarWidth(prevHealth);

		this.damageBar.width(prevFillWidth);

		let fillBarAnimation = new AnimationSequence([
			new TweenFromCurrent({
				node: this.fillBar,
				width: newFillWidth,
				duration: DMG_DURATION
			}),
			new AnimationPause(LINGER_DURATION * 1000),
			new AnimationPause(DRAIN_DURATION * 1000),
		]);

		let damageBarAnimation = new AnimationSequence([
			new AnimationPause(DMG_DURATION * 1000),
			new AnimationPause(LINGER_DURATION * 1000),
			new TweenFromCurrent({
				node: this.damageBar,
				width: newFillWidth,
				duration: DRAIN_DURATION
			})
		]);

		let chunkAnimation = new SimultaneousAnimation([
			fillBarAnimation,
			damageBarAnimation
		]).play();
	}

	constructor(_maxHealth:number, _boundingBox:Konva.Vector2d) {
		super();
		this.maxHealth = _maxHealth;
		this.remainingHealth = _maxHealth;
		this.boundingBox = _boundingBox;

		const cornerRadius = Math.min(this.boundingBox.y * 0.25, 4);

		this.backBar = new Konva.Rect({
			x: 0,
			y: 0,
			width: this.boundingBox.x,
			height: this.boundingBox.y,
			fill: "#502020",
			cornerRadius,
		});
		this.add(this.backBar);

		this.damageBar = new Konva.Rect({
			x: 0,
			y: 0,
			width: this.boundingBox.x,
			height: this.boundingBox.y,
			fill: "#f0d040",
			cornerRadius,
		});
		this.add(this.damageBar);

		this.fillBar = new Konva.Rect({
			x: 0,
			y: 0,
			width: this.boundingBox.x,
			height: this.boundingBox.y,
			fill: "#d96b35",
			cornerRadius,
		});
		this.add(this.fillBar);

		this.border = new Konva.Rect({
			x: 0,
			y: 0,
			width: this.boundingBox.x,
			height: this.boundingBox.y,
			stroke: "#111",
			strokeWidth: 2,
			cornerRadius,
		});
		this.add(this.border);
	}
}