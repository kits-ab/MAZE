import * as signalR from '@microsoft/signalr';
import { GameId } from './game.service';
import { Observable, Subject } from 'rxjs';

export class GameEventService {
  private readonly worldUpdates = new Subject<IWorldUpdated>();

  constructor(gameId: GameId) {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:44396/gameEvents?gameId=${gameId}`)
      .build();

    connection.on('WorldUpdated', (worldUpdated: IWorldUpdated) => {
      this.worldUpdates.next(worldUpdated);
    });

    connection.start();
  }

  getWorldUpdates(): Observable<IWorldUpdated> {
    return this.worldUpdates;
  }
}

interface IWorldUpdated {
  potentiallyChangedResources: string[];
}
