import { TargetButtonModel } from "./TargetButtonModel";

export { TargetListModel }

class TargetListModel {
    public get selected() : TargetButtonModel | undefined {
        return this.targets.find((target:TargetButtonModel) => {return target.selected})
    }
    public targets:Array<TargetButtonModel>

    constructor(_targets:Array<TargetButtonModel>) {
        this.targets = _targets
    }
}