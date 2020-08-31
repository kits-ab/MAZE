import { Component, OnInit } from '@angular/core';
import { GamesService, CharacterClass, PatchOperation, Move, ClearObstacle, UsePortal, Player } from '@kokitotsos/maze-client-angular';
import { CharacterId, GameId, ObstacleId, PathId } from '../game.service';
import { ActivatedRoute } from '@angular/router';
import { GameEventService } from '../game-event.service';
import { combineLatest } from 'rxjs';
import { filter, flatMap, map } from 'rxjs/operators';

@Component({
  selector: 'app-game-control',
  templateUrl: './game-control.component.html',
  styleUrls: ['./game-control.component.scss']
})
export class GameControlComponent implements OnInit {
  private static readonly maxNumberOfSteps = 4;
  private readonly gameId: GameId = this.activatedRoute.snapshot.params.id;
  private readonly gameEventService = new GameEventService(this.gameId, this.activatedRoute.snapshot.params.playerName);

  characters: ICharacter[] = [];

  controlledActionNames: ActionName[] = [];

  awaitingNewControls = true;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gameApi: GamesService) { }

  ngOnInit() {
    const currentPlayer$ = this.gameEventService.getPlayer();

    currentPlayer$.subscribe(player => {
      this.gameApi.defaultHeaders = this.gameApi.defaultHeaders.set('Authorization', `Bearer ${player.token}`);
    });

    const worldUpdate$ = this.gameEventService.getWorldUpdates();

    const players$ = worldUpdate$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('players')))
      .pipe(flatMap(_ => {
        return this.gameApi.getPlayers(this.gameId)
      }));

    const player$ = combineLatest(players$, currentPlayer$)
      .pipe(map(([players, currentPlayer]) => players.find(player => player.id == currentPlayer.id)))
      .pipe(filter<Player>(player => player != null));

    player$.subscribe(player => {
      this.controlledActionNames = player.actions.filter(this.isSupportedActionName);
    });

    const characters$ = worldUpdate$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('characters')))
      .pipe(flatMap(_ => this.gameApi.getCharacters(this.gameId)));

    characters$.subscribe(characters => {
      this.characters = characters.map(character => {
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

        character.availableActions.forEach(action => {
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

        const newCharacter: ICharacter = {
          id: character.id,
          characterClass: character.characterClass,
          actions: actions
        };
        return newCharacter;
      });

      this.awaitingNewControls = false;
    });
  }

  private isSupportedActionName(actionName: string): actionName is ActionName {
    switch (actionName) {
      case 'moveWest':
      case 'moveEast':
      case 'moveNorth':
      case 'moveSouth':
      case 'usePortal':
      case 'clearObstacle':
        const typedActionName: ActionName = actionName;
        return typedActionName != null;

      default:
        return false;
    }
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
