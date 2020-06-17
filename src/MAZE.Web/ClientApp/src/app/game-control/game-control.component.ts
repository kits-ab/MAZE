import { Component, OnInit } from '@angular/core';
import { GamesService, CharacterClass, PatchOperation, Movement } from '@kokitotsos/maze-client-angular';
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
  actionControls = new Map<Action, IActionControl>();
  awaitingNewControls = true;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gameApi: GamesService) { }

  ngOnInit() {
    this.actionControls = new Map<Action, IActionControl>();
    this.addActionControl('north');
    this.addActionControl('west');
    this.addActionControl('east');    
    this.addActionControl('south');
    this.addActionControl('portal');

    this.gameEventService.getToken().subscribe(newToken => {
      this.gameApi.defaultHeaders = this.gameApi.defaultHeaders.set('Authorization', `Bearer ${newToken}`);
    });

    this.gameEventService.getWorldUpdates().subscribe(worldUpdate => {
      if (worldUpdate.potentiallyChangedResources.includes('characters')) {
        this.gameApi.getCharacters(this.gameId).subscribe(characters => {
          // Reset movements
          this.actionControls.forEach(actionControl => {
            actionControl.characterControls.forEach(characterControl => {
              for (let i = 0; i < characterControl.movements.length; i++) {
                characterControl.movements[i] = null;
              }
            });
          });

          characters.forEach(character => {
            if (!this.actionControls.get('west').characterControls.has(character.id)) {
              this.addCharacterControls(character.id, character.characterClass);
            }
            
            character.availableMovements
              .filter(movement => movement.numberOfPathsToTravel <= GameControlComponent.maxNumberOfSteps)
              .forEach(movement => {
                const actionControl = this.actionControls.get(movement.type);
                const characterControl = actionControl.characterControls.get(character.id);
                characterControl.movements[movement.numberOfPathsToTravel - 1] = movement.location;
              });

            this.awaitingNewControls = false;
          });
        });
      }
    });
  }

  private addActionControl(action: Action): void {
    const newActionControl: IActionControl = {
      action: action,
      characterControls: new Map<CharacterId, ICharacterControl>()
    };
    this.actionControls.set(newActionControl.action, newActionControl);
  }

  private addCharacterControls(characterId: CharacterId, characterClass: CharacterClass): void {
    this.addCharacterControl(characterId, characterClass, 'west');
    this.addCharacterControl(characterId, characterClass, 'east');
    this.addCharacterControl(characterId, characterClass, 'north');
    this.addCharacterControl(characterId, characterClass, 'south');
    this.addCharacterControl(characterId, characterClass, 'portal');
  }

  private addCharacterControl(characterId: CharacterId, characterClass: CharacterClass, action: Action): void {
    const newCharacterControl: ICharacterControl = {
      characterId: characterId,
      characterClass: characterClass,
      movements: new Array<LocationId>(action == 'portal' ? 1 : GameControlComponent.maxNumberOfSteps)
    };
    this.actionControls.get(action).characterControls.set(newCharacterControl.characterId, newCharacterControl);
  }

  actionsCanBeUsed(): boolean {
    return this.gameApi.defaultHeaders.has('Authorization') && !this.awaitingNewControls;
  }

  getActionControls(): IActionControl[] {
    return [...this.actionControls.values()];
  }

  getCharacterControls(actionControl: IActionControl): ICharacterControl[] {
    return [...actionControl.characterControls.values()];
  }

  getLocationIds(characterControl: ICharacterControl): LocationId[] {
    return characterControl.movements;
  }

  move(characterId: CharacterId, newLocationId: LocationId): void {
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

interface IActionControl {
  action: Action;
  characterControls: Map<CharacterId, ICharacterControl>;
}

interface ICharacterControl {
  characterId: CharacterId;
  characterClass: CharacterClass;
  movements: LocationId[];
}

type Action = 'west' | 'east' | 'north' | 'south' | 'portal';
