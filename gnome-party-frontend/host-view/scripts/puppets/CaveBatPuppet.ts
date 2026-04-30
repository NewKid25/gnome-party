import Konva from "konva";
import { Vector2d } from "konva/lib/types";
import Puppet from "../interfaces/Puppet";

export default
class CaveBatPuppet extends Konva.Group implements Puppet {
	
	body:Konva.Image

	static readonly WIDTH = 425;
	static readonly HEIGHT = 170;
	
	constructor() {
		super();

		// Why are these the numbers I need? I don't know. This was trial and error
		const imgOffset:Vector2d = {x: 351 / 2.9, y: 2.05 * 460 / 5}
	
		const bodyImg = new Image();
		bodyImg.src = "/img/Cave Bat.svg";
		this.body = new Konva.Image({
			x: 0,
			y: 0,
			width: CaveBatPuppet.WIDTH,
			height: CaveBatPuppet.HEIGHT,
			// offset: imgOffset,
			image: bodyImg,
		});
		this.add(this.body);

		
		this.width(203/ 2);
		this.height(300 / 2);
		this.scale({x: 0.5, y: 0.5})
		this.offset(imgOffset);
		this.position({x: 0, y:0});

		console.log(this.offset());
	}
}