export interface TurnStep {
    Request:   ActionRequest;
    GameState: GameState;
    Events: TurnEvent[];
}

export interface GameState {
    PlayerCharacters: Character[];
    EnemyCharacters:  Character[];
}

export interface Character {
    CharacterType?:      string;
    Id:                  string;
    Name:                string;
    Health:              number;
    MaxHealth:           number;
    ActionsDescriptions: ActionsDescription[];
    StatusEffects?:      object[];
}

export interface ActionsDescription {
    Name:        string;
    Description: string;
}

export interface ActionRequest {
    EncounterId:       string;
    TargetCharacterId: string;
    SourceCharacterId: string;
    Action:            string;
}

export interface TurnEvent {
    event: string;
    params: any;
}