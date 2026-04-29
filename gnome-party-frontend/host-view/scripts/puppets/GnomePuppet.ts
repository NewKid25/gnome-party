import Konva from "konva";
import { Vector2d } from "konva/lib/types";
import Puppet from "../interfaces/Puppet";

export default
class GnomePuppet extends Konva.Group implements Puppet {
	
	body:Konva.Image
	head:Konva.Image
	beard:Konva.Image
	hat:Konva.Image
	nose:Konva.Image
	weapon:Konva.Image
	
	static readonly WIDTH = 437;
	static readonly HEIGHT = 595;

	constructor(charClass:string = "Warrior") {
		super();

		const baseX = -GnomePuppet.WIDTH / 2;
		const baseY = -GnomePuppet.HEIGHT;

		const bodyImg = new Image();
		bodyImg.src = "/img/Body-Blue.svg";
		this.body = new Konva.Image({
			x: baseX,
			y: baseY,
			image: bodyImg,
			width: GnomePuppet.WIDTH,
			height: GnomePuppet.HEIGHT,
		});
		this.add(this.body);

		const headImg = new Image();
		headImg.src = "/img/Head-Tone1.svg";
		this.head = new Konva.Image({
			x: baseX,
			y: baseY,
			image: headImg,
			width: GnomePuppet.WIDTH,
			height: GnomePuppet.HEIGHT,
		});
		this.add(this.head);

		const beardImg = new Image();
		beardImg.src = "/img/Beard.svg";
		this.beard = new Konva.Image({
			x: baseX,
			y: baseY,
			image: beardImg,
			width: GnomePuppet.WIDTH,
			height: GnomePuppet.HEIGHT,
		});
		this.add(this.beard);

		const hatImg = new Image();
		hatImg.src = "/img/Hat-PointyPink.svg";
		this.hat = new Konva.Image({
			x: baseX,
			y: baseY,
			image: hatImg,
			width: GnomePuppet.WIDTH,
			height: GnomePuppet.HEIGHT,
		});
		this.add(this.hat);

		const noseImg = new Image();
		noseImg.src = "/img/Nose-Tone1.svg";
		this.nose = new Konva.Image({
			x: baseX,
			y: baseY,
			image: noseImg,
			width: GnomePuppet.WIDTH,
			height: GnomePuppet.HEIGHT,
		});
		this.add(this.nose);

		const weaponImg = new Image();
		switch (charClass) {
			case "Warrior":
				weaponImg.src = "/img/Warrior Weapon.svg";
				break;
			case "Mage":
				weaponImg.src = "/img/Mage Weapon.svg";
				break;
			case "Bard":
				weaponImg.src = "/img/Bard Weapon.svg";
				break;
		}

		this.weapon = new Konva.Image({
			x: baseX + GnomePuppet.WIDTH / 1.75,
			y: baseY + GnomePuppet.HEIGHT / 4 ,
			image: weaponImg,
			width: GnomePuppet.WIDTH * .5,
			height: GnomePuppet.HEIGHT * .5,
		});
		this.add(this.weapon);

		this.scale({ x: 1, y: 1 });
		this.position({ x: 0, y: 0 });
	}
}