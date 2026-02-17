import { ActionButtonModel } from "./ActionButtonModel"

export { ActionListModel }

class ActionListModel {
	public get selected() : ActionButtonModel | undefined {
		return this.actions.find((action:ActionButtonModel) => {return action.selected})
	}
	public actions:Array<ActionButtonModel>

	constructor(_actions:Array<ActionButtonModel>) {
		this.actions = _actions
	}
}