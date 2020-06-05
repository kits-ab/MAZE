import { Component, OnInit } from '@angular/core';
import { Path, GamesService, CharacterClass, Character, PatchOperation } from '@kokitotsos/maze-client-angular';
import { LocationId, CharacterId, GameId } from '../game.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-game-control',
  templateUrl: './game-control.component.html',
  styleUrls: ['./game-control.component.scss']
})
export class GameControlComponent implements OnInit {
  readonly gameId: GameId = this.activatedRoute.snapshot.params.id;
  characters: ICharacter[];
  movements = new Map<CharacterId, Map<Path.TypeEnum, IMovement[]>>();

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gameApi: GamesService) { }

  ngOnInit() {
    this.gameApi.getCharacters(this.gameId).subscribe(characters => {
      this.characters = characters.map(character => ({
        id: character.id,
        characterClass: character.characterClass
      }));

      characters.forEach(character => {
        const eastMovements = new Array<IMovement>(5);

        for (let i = 0; i < 5; i++) {
          eastMovements[i] = {
            numberOfSteps: i + 1,
            locationId: i != 4 ? i : null
          };
        }

        const movements = new Map<Path.TypeEnum, IMovement[]>();
        movements.set('east', eastMovements);
        this.movements.set(character.id, movements);
      });
    });
  }

  getMovements(characterId: CharacterId, direction: Path.TypeEnum): IMovement[] {
    return this.movements.get(characterId).get(direction);
  }

  move(characterId: CharacterId, newLocationId: LocationId): void {
    //const patchOperation: PatchOperation = {
    //  op: 'replace',
    //  path: 'location',
    //  value: newLocationId
    //};
    //this.gameApi.updateCharacter(this.gameId, characterId, [patchOperation]);
  }
}

interface ICharacter {
  id: CharacterId;
  characterClass: CharacterClass;
}

interface IMovement {
  numberOfSteps: number;
  locationId: LocationId;
}
