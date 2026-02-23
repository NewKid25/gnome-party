import Konva from 'konva'

export type { HealthBarModel }

interface HealthBarModel {
    value:number
    maxValue:number
    sprite:Konva.Group
}