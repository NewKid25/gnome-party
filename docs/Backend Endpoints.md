# Endpoints
To have the backend perform certain logic, you need to provide information to the Websocket API. You need to specify what logic you want it to perform as well as any parameters to that logic. To do your desired logic, the API must route your request to the correct lambda function. For out API you must do this by specifying the "route" key in a JSON object, this will direct the API to invoke the lamdba at your desired endpoint. 

## join-game
- tries to get a GameSession from the game table with an invite code equal to 0, if now such GameSession exists, then it creates a new one
- adds the requesting connection as a player to the GameSession, saves the GameSession to the game table 
- sends the game session to the requesting connection
- format:
    {"route":"join-game"}

## begin-combat-encounter
- creates a new ActiveCombatEncounter from the first Encounter of the Gampaign in the GameSession with the provided ID, save the ActiveCombatEncounter to the ActiveEncounterTable
- sends the ActiveCombatEncounter to the requesting connection
- format:
    {
        "route":"begin-combat-encounter",
        "GameSessionId" : string
    }

## player-action
- applies the action of the name provided from the source character to the target character 
- you must specify the encounter where this action is happening
- format:
    {
        "route": "player-action",
        "EncounterId": string, 
        "TargetCharacterId": string, 
        "SourceCharacterId": string, 
        "Action": string (name of an action)
    }


### automatic endpoints
To hit this endpoints, you do not need to specify a route. They occur naturally in the course of using the WebSocket API.
## $connect
- triggered when a connection request is made to the websocket. 
- this stores the a connection ID to the connection table
## $disconnect
- triggered when a client disconnects from the websocket. This can be intentionally, or after a period of no communication (10mins I believe)
- this removes a connection ID from the connection table

