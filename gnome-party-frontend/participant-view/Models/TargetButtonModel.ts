import { CharacterImageModel } from "./CharacterImageModel"
import { HealthBarModel } from "./HealthBarModel"

export type { TargetButtonModel }

interface TargetButtonModel {
    selected:boolean
    targetName:string
    characterImage: CharacterImageModel
    healthbar: HealthBarModel
    targetId:string
}