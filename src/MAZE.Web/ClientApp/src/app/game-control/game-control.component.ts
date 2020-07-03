import { Component, OnInit } from '@angular/core';
import { GamesService, CharacterClass, PatchOperation } from '@kokitotsos/maze-client-angular';
import { LocationId, CharacterId, GameId } from '../game.service';
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
            let portalAction: IMovementAction | null = null;
            
            newCharacter.availableActions.forEach(action => {
              if (action.actionName == 'move' && action.numberOfPathsToTravel <= GameControlComponent.maxNumberOfSteps) {
                if (action.type == 'portal') {
                  portalAction = this.createPortalAction(action.location);
                }
                else {
                  movementActions.set(action.numberOfPathsToTravel + action.type, this.createMovementAction(action.type, action.numberOfPathsToTravel, action.location));
                }
              }
            });

            const finalMovementAction = new Array<IMovementAction>(GameControlComponent.maxNumberOfSteps * 4 + 1);
            const directions: Direction[] = ['west', 'east', 'north', 'south'];
            directions.forEach((direction, directionIndex) => {
              for (let movementSteps = 1; movementSteps <= GameControlComponent.maxNumberOfSteps; movementSteps++) {
                const existingMovementAction = movementActions.get(movementSteps + direction);
                const movementActionIndex = directionIndex * GameControlComponent.maxNumberOfSteps + movementSteps - 1;
                if (existingMovementAction != null) {
                  finalMovementAction[movementActionIndex] = existingMovementAction;
                }
                else {
                  finalMovementAction[movementActionIndex] = this.createMovementAction(direction, movementSteps);
                }
              }
            });

            const portalActionIndex = finalMovementAction.length - 1;
            if (portalAction != null) {
              finalMovementAction[portalActionIndex] = portalAction;
            }
            else {
              finalMovementAction[portalActionIndex] = this.createPortalAction();
            }

            const actions = finalMovementAction;

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

  getActions(character: ICharacter, actionName: ActionName): IMovementAction[] {
    return character.actions.filter(action => action.name == actionName);
  }

  executeAction(characterId: CharacterId, action: IMovementAction): void {
    if (action.locationId == null) {
      throw new Error('Cannot execute a movement without a location');
    }
    else {
      this.move(characterId, action.locationId);
    }
  }

  private move(characterId: CharacterId, newLocationId: LocationId): void {
    const patchOperation: PatchOperation = {
      op: 'replace',
      path: 'location',
      value: newLocationId
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
  actions: IMovementAction[];
}

interface IMovementAction {
  name: MovementActionName;
  description: string;
  isAvailable: boolean;
  locationId: LocationId | null;
}

type DirectionMovementActionName = 'move-west' | 'move-east' | 'move-north' | 'move-south';

type MovementActionName = DirectionMovementActionName | 'use-portal';

type ActionName = MovementActionName | 'use-portal' | 'clear-obstacle';

type Direction = 'west' | 'east' | 'north' | 'south';
