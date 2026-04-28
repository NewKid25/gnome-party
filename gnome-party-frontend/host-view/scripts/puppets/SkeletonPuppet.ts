import Konva from "konva";
import { Vector2d } from "konva/lib/types";
import Puppet from "../interfaces/Puppet";

export default
class SkeletonPuppet extends Konva.Group implements Puppet {
	
	body:Konva.Image

	static readonly WIDTH = 351;
	static readonly HEIGHT = 449;

	constructor() {
		super();

		const baseX = -SkeletonPuppet.WIDTH / 2;
		const baseY = -SkeletonPuppet.HEIGHT;

		const bodyImg = new Image();
		bodyImg.src = "/img/Skeleton.svg";
		this.body = new Konva.Image({
			x: baseX,
			y: baseY,
			image: bodyImg,
			width: SkeletonPuppet.WIDTH,
			height: SkeletonPuppet.HEIGHT,
		});
		this.add(this.body);

		this.scale({x: 1, y: 1})
		this.position({x: 0, y:0});
	}
}