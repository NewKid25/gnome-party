import { PlayerImageModel } from "./PlayerImageModel";
import { HealthBarModel } from "./HealthBarModel";

export { PlayerStatusModel }

class PlayerStatusModel {
    public playerImage: PlayerImageModel
    public healthBar: HealthBarModel

    constructor(
        playerImage: PlayerImageModel,
        healthBar: HealthBarModel,
    ) {
        this.playerImage = playerImage
        this.healthBar = healthBar
        
    }
}
