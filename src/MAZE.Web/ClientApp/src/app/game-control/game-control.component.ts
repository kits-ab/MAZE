import { Component, OnInit } from '@angular/core';
import { GamesService, CharacterClass, PatchOperation, Move, ClearObstacle } from '@kokitotsos/maze-client-angular';
import { LocationId, CharacterId, GameId, ObstacleId } from '../game.service';
import { ActivatedRoute } from '@angular/router';
import { GameEventService } from '../game-event.service';

@Component({
  selector: 'app-game-control',
  templateUrl: './game-control.component.html',
  styleUrls: ['./game-control.component.scss']
})
export class GameControlComponent implements OnInit {
  private static readonly maxNumberOfSteps = 4;
  private readonly gameId: GameId = this.activatedRoute.snapshot.params.id;
  private readonly gameEventService = new GameEventService(this.gameId, this.activatedRoute.snapshot.params.playerName);

  actionNames: ActionName[] = [
    'move-west',
    'move-east',
    'move-north',
    'move-south',
    'use-portal',
    'clear-obstacle'
  ];

  characters: ICharacter[] = [];

  awaitingNewControls = true;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gameApi: GamesService) { }

  ngOnInit() {
    this.gameEventService.getToken().subscribe(newToken => {
      this.gameApi.defaultHeaders = this.gameApi.defaultHeaders.set('Authorization', `Bearer ${newToken}`);
    });

    this.gameEventService.getWorldUpdates().subscribe(worldUpdate => {
      if (worldUpdate.potentiallyChangedResources.includes('characters')) {
        this.gameApi.getCharacters(this.gameId).subscribe(newCharacters => {
          this.characters = newCharacters.map(newCharacter => {
            const movementActions = new Map<string, IMovementAction>();

            const portalAction: IMovementAction = {
              name: 'use-portal',
              description: 'Use portal',
              isAvailable: false,
              locationId: null
            };

            const clearObstacleAction: IClearObstacleAction = {
              name: 'clear-obstacle',
              description: 'Clear obstacle',
              isAvailable: false,
              obstacleId: null
            }
            
            newCharacter.availableActions.forEach(action => {
              if (action.actionName == 'move' && action.numberOfPathsToTravel <= GameControlComponent.maxNumberOfSteps) {
                if (action.type == 'portal') {
                  portalAction.isAvailable = true;
                  portalAction.locationId = action.location;
                }
                else {
                  movementActions.set(action.numberOfPathsToTravel + action.type, this.createMovementAction(action.type, action.numberOfPathsToTravel, action.location));
                }
              }
              else if (action.actionName == 'clearObstacle') {
                clearObstacleAction.isAvailable = true;
                clearObstacleAction.obstacleId = action.obstacle;
              }
            });

            const finalMovementActions = new Array<IMovementAction>(GameControlComponent.maxNumberOfSteps * 4);
            const directions: Direction[] = ['west', 'east', 'north', 'south'];
            directions.forEach((direction, directionIndex) => {
              for (let movementSteps = 1; movementSteps <= GameControlComponent.maxNumberOfSteps; movementSteps++) {
                const existingMovementAction = movementActions.get(movementSteps + direction);
                const movementActionIndex = directionIndex * GameControlComponent.maxNumberOfSteps + movementSteps - 1;
                if (existingMovementAction != null) {
                  finalMovementActions[movementActionIndex] = existingMovementAction;
                }
                else {
                  finalMovementActions[movementActionIndex] = this.createMovementAction(direction, movementSteps);
                }
              }
            });

            const actions: Action[] = finalMovementActions;
            actions.push(portalAction);
            actions.push(clearObstacleAction);

            const character: ICharacter = {
              id: newCharacter.id,
              characterClass: newCharacter.characterClass,
              actions: actions
            };
            return character;
          });

          this.awaitingNewControls = false;
        });
      }
    });
  }

  private createPortalAction(locationId?: LocationId): IMovementAction {
    return {
      name: 'use-portal',
      description: 'Use portal',
      isAvailable: locationId != null,
      locationId: locationId === undefined ? null : locationId
    };
  }

  private createMovementAction(direction: Direction, numberOfSteps: number, locationId?: LocationId): IMovementAction {
    return {
      name: this.getActionName(direction),
      description: `${numberOfSteps} step${numberOfSteps > 1 ? 's' : ''} ${direction}`,
      isAvailable: locationId != null,
      locationId: locationId === undefined ? null : locationId
    };
  }

  private getActionName(direction: Direction): DirectionMovementActionName {
    switch (direction) {
      case 'west':
        return 'move-west';
      case 'east':
        return 'move-east';
      case 'north':
        return 'move-north';
      case 'south':
        return 'move-south';
    }
  }

  actionsCanBeUsed(): boolean {
    return this.gameApi.defaultHeaders.has('Authorization') && !this.awaitingNewControls;
  }

  getActions(character: ICharacter, actionName: ActionName): Action[] {
    return character.actions.filter(action => action.name == actionName);
  }

  executeAction(characterId: CharacterId, action: Action): void {

    let actionToExecute: ClearObstacle | Move | undefined;

    if (action.name == "clear-obstacle") {
      if (action.obstacleId == null) {
        throw new Error('Cannot execute a obstacle removal without an obstacle');
      }
      const clearObstacle: ClearObstacle = {
        actionName: 'clearObstacle',
        obstacle: action.obstacleId
      };
      actionToExecute = clearObstacle;
    }
    else {
      if (action.locationId == null) {
        throw new Error('Cannot execute a movement without a location');
      }
      const move: Move = {
        actionName: 'move',
        location: action.locationId
      };
      actionToExecute = move;
    }

    if (actionToExecute == null) {
      throw new Error(`${action.name} is an unsupported action `);
    }

    const patchOperation: PatchOperation = {
      op: 'add',
      path: 'executedActions',
      value: actionToExecute
    };
    this.awaitingNewControls = true;
    this.gameApi.updateCharacter(this.gameId, characterId, [patchOperation])
      .subscribe(
        _ => {
          // Expect to get new event with available actions on success
        },
        _ => {
          // Expect to get new event with available actions on conflict
        });
  }
}

interface ICharacter {
  id: CharacterId;
  characterClass: CharacterClass;
  actions: Array<IMovementAction | IClearObstacleAction>;
}

interface IAction {
  description: string;
  isAvailable: boolean;
}

interface IMovementAction extends IAction {
  name: MovementActionName;
  locationId: LocationId | null;
}

interface IClearObstacleAction extends IAction {
  name: 'clear-obstacle';
  obstacleId: ObstacleId | null;
}

type DirectionMovementActionName = 'move-west' | 'move-east' | 'move-north' | 'move-south';

type MovementActionName = DirectionMovementActionName | 'use-portal';

type ActionName = MovementActionName | 'clear-obstacle';

type Direction = 'west' | 'east' | 'north' | 'south';

type Action = IMovementAction | IClearObstacleAction;
