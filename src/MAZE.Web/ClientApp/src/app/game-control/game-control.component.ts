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
  private static readonly maxNumberOfSteps = 5;
  readonly gameId: GameId = this.activatedRoute.snapshot.params.id;
  private readonly gameEventService = new GameEventService(this.gameId);
  characterControls = new Map<CharacterId, ICharacterControl>();

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gameApi: GamesService) { }

  ngOnInit() {
    this.gameEventService.getWorldUpdates().subscribe(worldUpdate => {
      if (worldUpdate.potentiallyChangedResources.includes('characters')) {
        this.gameApi.getCharacters(this.gameId).subscribe(characters => {
          characters.forEach(character => {
            if (!this.characterControls.has(character.id)) {
              const newMovements = new Map<MovementType, LocationId[]>();
              newMovements.set('west', new Array<LocationId>(GameControlComponent.maxNumberOfSteps));
              newMovements.set('east', new Array<LocationId>(GameControlComponent.maxNumberOfSteps));
              newMovements.set('north', new Array<LocationId>(GameControlComponent.maxNumberOfSteps));
              newMovements.set('south', new Array<LocationId>(GameControlComponent.maxNumberOfSteps));
              newMovements.set('portal', new Array<LocationId>(1));
              const newCharacterControl: ICharacterControl = {
                characterId: character.id,
                characterClass: character.characterClass,
                isAvailable: true,
                movements: newMovements
              };
              this.characterControls.set(newCharacterControl.characterId, newCharacterControl);
            }
            const characterControl = this.characterControls.get(character.id);
            characterControl.movements.forEach(movements => {
              for (let i = 0; i < movements.length; i++) {
                movements[i] = null;
              }
            })
            character.availableMovements
              .filter(movement => movement.numberOfPathsToTravel <= GameControlComponent.maxNumberOfSteps)
              .forEach(movement => {
                characterControl.movements.get(movement.type)[movement.numberOfPathsToTravel - 1] = movement.location;
              });

            characterControl.isAvailable = true;
          });
        });
      }
    });
  }

  getCharacterControls(): ICharacterControl[] {
    return [...this.characterControls.values()];
  }

  getLocationIds(characterControl: ICharacterControl, movementType: MovementType): LocationId[] {
    return characterControl.movements.get(movementType);
  }

  move(characterId: CharacterId, newLocationId: LocationId): void {
    const patchOperation: PatchOperation = {
      op: 'replace',
      path: 'location',
      value: newLocationId
    };
    this.characterControls.get(characterId).isAvailable = false;
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

interface ICharacterControl {
  characterId: CharacterId;
  characterClass: CharacterClass;
  isAvailable: boolean;
  movements: Map<MovementType, LocationId[]>;
}

type MovementType = 'west' | 'east' | 'north' | 'south' | 'portal';
