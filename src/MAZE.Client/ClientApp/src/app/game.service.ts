import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  static readonly tileSize = 32;

  getTiles(gameId: GameId): Observable<ITile[]> {
    return from([[
      { locationId: 0, x: 0, y: 0, image: '/assets/castle/left.png' },
      { locationId: 1, x: 0, y: GameService.tileSize, image: '/assets/castle/left.png' }
    ]]);
  }
}

type GameId = string;

export interface ITile {
  locationId: number;
  x: number;
  y: number;
  image: string;
}
