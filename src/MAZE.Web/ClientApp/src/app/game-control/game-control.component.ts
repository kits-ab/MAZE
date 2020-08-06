import { Component, OnInit } from '@angular/core';
import { GamesService, CharacterClass, PatchOperation, Move, ClearObstacle, UsePortal } from '@kokitotsos/maze-client-angular';
import { LocationId, CharacterId, GameId, ObstacleId, PathId } from '../game.service';
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
    'moveWest',
    'moveEast',
    'moveNorth',
    'moveSouth',
    'usePortal',
    'clearObstacle'
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

            const portalAction: IUsePortalAction = {
              name: 'usePortal',
              description: 'Use portal',
              isAvailable: false,
              portalPathId: null
            };

            const clearObstacleAction: IClearObstacleAction = {
              name: 'clearObstacle',
              description: 'Clear obstacle',
              isAvailable: false,
              obstacleId: null
            }
            
            newCharacter.availableActions.forEach(action => {
              if (action.actionName == 'clearObstacle') {
                clearObstacleAction.isAvailable = true;
                clearObstacleAction.obstacleId = action.obstacle;
              }
              else if (action.actionName == 'usePortal') {
                portalAction.isAvailable = true;
                portalAction.portalPathId = action.portalPath;
              }
              else if (action.actionName == 'disarm' || action.actionName == 'heal' || action.actionName == 'smash' || action.actionName == 'teleport') {
                // Non-implemented actions
              }
              else if (action.numberOfPathsToTravel <= GameControlComponent.maxNumberOfSteps) {
                movementActions.set(action.numberOfPathsToTravel + action.actionName, this.createMovementAction(action.actionName, action.numberOfPathsToTravel));
              }
            });

            const finalMovementActions = new Array<IMovementAction>(GameControlComponent.maxNumberOfSteps * 4);
            const movementActionNames: MovementActionName[] = ['moveWest', 'moveEast', 'moveNorth', 'moveSouth'];
            movementActionNames.forEach((movementActionName, directionIndex) => {
              for (let movementSteps = 1; movementSteps <= GameControlComponent.maxNumberOfSteps; movementSteps++) {
                const existingMovementAction = movementActions.get(movementSteps + movementActionName);
                const movementActionIndex = directionIndex * GameControlComponent.maxNumberOfSteps + movementSteps - 1;
                if (existingMovementAction != null) {
                  finalMovementActions[movementActionIndex] = existingMovementAction;
                }
                else {
                  finalMovementActions[movementActionIndex] = this.createMovementAction(movementActionName, movementSteps, false);
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

  private createMovementAction(actionName: MovementActionName, numberOfSteps: number, isAvailable: boolean = true): IMovementAction {
    return {
      name: actionName,
      description: `${numberOfSteps} step${numberOfSteps > 1 ? 's' : ''} ${this.getDirection(actionName)}`,
      isAvailable: isAvailable,
      numberOfPathsToTravel: numberOfSteps
    };
  }

  private getDirection(actionName: MovementActionName): Direction {
    switch (actionName) {
      case 'moveWest':
        return 'west';
      case 'moveEast':
        return 'east';
      case 'moveNorth':
        return 'north';
      case 'moveSouth':
        return 'south';
    }
  }

  actionsCanBeUsed(): boolean {
    return this.gameApi.defaultHeaders.has('Authorization') && !this.awaitingNewControls;
  }

  getActions(character: ICharacter, actionName: ActionName): Action[] {
    return character.actions.filter(action => action.name == actionName);
  }

  executeAction(characterId: CharacterId, action: Action): void {

    let actionToExecute: ClearObstacle | Move | UsePortal | undefined;

    if (action.name == 'clearObstacle') {
      if (action.obstacleId == null) {
        throw new Error('Cannot execute a obstacle removal without an obstacle');
      }
      const clearObstacle: ClearObstacle = {
        actionName: action.name,
        obstacle: action.obstacleId
      };
      actionToExecute = clearObstacle;
    }
    else if (action.name == 'usePortal') {
      if (action.portalPathId == null) {
        throw new Error('Cannot execute a use portal without specifying which path to travel');
      }
      const usePortal: UsePortal = {
        actionName: action.name,
        portalPath: action.portalPathId
      };
      actionToExecute = usePortal;
    }
    else {
      if (action.numberOfPathsToTravel == null) {
        throw new Error('Cannot execute a movement without number of steps to travel');
      }
      const move: Move = {
        actionName: action.name,
        numberOfPathsToTravel: action.numberOfPathsToTravel
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
  actions: Array<Action>;
}

interface IAction {
  description: string;
  isAvailable: boolean;
}

interface IMovementAction extends IAction {
  name: MovementActionName;
  numberOfPathsToTravel: number;
}

interface IUsePortalAction extends IAction {
  name: 'usePortal';
  portalPathId: PathId | null;
}

interface IClearObstacleAction extends IAction {
  name: 'clearObstacle';
  obstacleId: ObstacleId | null;
}

type MovementActionName = 'moveWest' | 'moveEast' | 'moveNorth' | 'moveSouth';

type ActionName = MovementActionName | 'usePortal' | 'clearObstacle';

type Direction = 'west' | 'east' | 'north' | 'south';

type Action = IMovementAction | IUsePortalAction | IClearObstacleAction;
