import { CharacterImageModel } from "./CharacterImageModel";
import { HealthBarModel } from "./HealthBarModel";

export { PlayerStatusModel }

class PlayerStatusModel {
    public characterImage: CharacterImageModel
    public healthBar: HealthBarModel

    constructor(
        characterImage: CharacterImageModel,
        healthBar: HealthBarModel,
    ) {
        this.characterImage = characterImage
        this.healthBar = healthBar
        
    }
}
