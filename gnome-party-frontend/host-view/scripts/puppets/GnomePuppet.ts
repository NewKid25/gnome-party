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
	
	constructor() {
		super();

		// Why are these the numbers I need? I don't know. This was trial and error
		const imgOffset:Vector2d = {x: 437 / 2.9, y: 2.05 * 595 / 5}
	
		const bodyImg = new Image();
		bodyImg.src = "/img/Body-Blue.svg";
		this.body = new Konva.Image({
			x: 0,
			y: 0,
			// offset: imgOffset,
			image: bodyImg,
		});
		this.add(this.body);

		const headImg = new Image();
		headImg.src = "/img/Head-Tone1.svg";
		this.head = new Konva.Image({
			x: 0,
			y: 0,
			// offset: imgOffset,
			image: headImg,
		});

		this.add(this.head);

		const beardImg = new Image();
		beardImg.src = "/img/Beard.svg";
		this.beard = new Konva.Image({
			x: 0,
			y: 0,
			// offset: imgOffset,
			image: beardImg,
		});
		this.add(this.beard);

		const hatImg = new Image();
		hatImg.src = "/img/Hat-PointyPink.svg";
		this.hat = new Konva.Image({
			x: 0,
			y: 0,
			// offset: imgOffset,
			image: hatImg,
		});
		this.add(this.hat);

		const noseImg = new Image();
		noseImg.src = "/img/Nose-Tone1.svg";
		this.nose = new Konva.Image({
			x: 0,
			y: 0,
			// offset: imgOffset,
			image: noseImg,
		});
		this.add(this.nose);
		
		// this.offset({x: 437 / 2, y: 595 / 2})
		
		this.width(437/ 2);
		this.height(595 / 2);
		this.scale({x: 0.5, y: 0.5})
		this.offset(imgOffset);
		this.position({x: 0, y:0});

		console.log(this.offset());
	}
}