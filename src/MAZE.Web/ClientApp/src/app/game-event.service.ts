import * as signalR from '@microsoft/signalr';
import { GameId } from './game.service';
import { Observable, Subject } from 'rxjs';
import { environment } from '../environments/environment';

export class GameEventService {
  private readonly worldUpdates = new Subject<IWorldUpdated>();

  constructor(gameId: GameId, playerName?: string) {
    let connectionUrl = `${environment.apiUrl}/gameEvents?gameId=${gameId}`;

    if (playerName != null) {
      connectionUrl += `&playerName=${encodeURIComponent(playerName)}`
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(connectionUrl)
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
