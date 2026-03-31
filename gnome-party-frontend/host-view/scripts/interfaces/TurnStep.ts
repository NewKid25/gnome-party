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
    Id:                  string;
    Name:                string;
    Health:              number;
    MaxHealth:           number;
    ActionsDescriptions: ActionsDescription[];
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