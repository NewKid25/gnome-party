import { PlayerImageModel } from "./PlayerImageModel";
import { HealthBarModel } from "./HealthBarModel";

export { PlayerStatusModel }

class PlayerStatusModel {
    public playerImage: PlayerImageModel
    public healthBar: HealthBarModel

    constructor(
        healthBar: HealthBarModel,
        playerImage: PlayerImageModel,
    ) {
        this.healthBar = healthBar
        this.playerImage = playerImage
    }
}
