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
            const movementActions = new Array<IMovementAction>(GameControlComponent.maxNumberOfSteps * 4 + 1);
            const directions: Direction[] = ['west', 'east', 'north', 'south'];
            directions.forEach((direction, directionIndex) => {
              for (let movementSteps = 1; movementSteps <= GameControlComponent.maxNumberOfSteps; movementSteps++) {
                const availableMovement = newCharacter.availableMovements.find(movement => movement.numberOfPathsToTravel == movementSteps && movement.type == direction);
                movementActions[directionIndex * GameControlComponent.maxNumberOfSteps + movementSteps - 1] = {
                  name: this.getActionName(direction),
                  description: `${movementSteps} step${movementSteps > 1 ? 's' : ''} ${direction}`,
                  isAvailable: availableMovement ? true : false,
                  locationId: availableMovement ? availableMovement.location : null
                };
              }
            });

            const availablePortal = newCharacter.availableMovements.find(movement => movement.type == 'portal');
            movementActions[movementActions.length - 1] = {
              name: 'use-portal',
              description: 'Use portal',
              isAvailable: availablePortal ? true : false,
              locationId: availablePortal ? availablePortal.location : null
            }

            const character: ICharacter = {
              id: newCharacter.id,
              characterClass: newCharacter.characterClass,
              actions: movementActions
            };
            return character;
          });

          this.awaitingNewControls = false;
        });
      }
    });
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
    this.move(characterId, action.locationId);
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
  locationId: LocationId;
}

type DirectionMovementActionName = 'move-west' | 'move-east' | 'move-north' | 'move-south';

type MovementActionName = DirectionMovementActionName | 'use-portal';

type ActionName = MovementActionName | 'use-portal' | 'clear-obstacle';

type Direction = 'west' | 'east' | 'north' | 'south';
