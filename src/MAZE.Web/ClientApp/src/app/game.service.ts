import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { GamesService, Location } from 'maze-client-angular';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  static readonly tileSize = 32;

  constructor(private readonly gamesApi: GamesService) {
  }

  getTiles(gameId: GameId): Observable<ITile[]> {
    return this.gamesApi.getLocations(gameId).pipe(map(locations => {
      return locations.map(this.convert);
    }));
  }

  private convert(location: Location): ITile {
    return { locationId: location.id, x: 0, y: location.id * GameService.tileSize, image: '/assets/castle/left.png' };
  }
}

type GameId = string;

export interface ITile {
  locationId: number;
  x: number;
  y: number;
  image: string;
}
