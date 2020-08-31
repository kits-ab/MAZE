import * as signalR from '@microsoft/signalr';
import { GameId, PlayerId } from './game.service';
import { Observable, Subject, ReplaySubject } from 'rxjs';
import { environment } from '../environments/environment';

export class GameEventService {
  private readonly worldUpdates = new Subject<IWorldUpdated>();
  private readonly player = new ReplaySubject<IPlayer>(1);

  constructor(gameId: GameId, playerName?: string) {
    let connectionUrl = `${environment.apiUrl}/gameEvents?gameId=${gameId}`;

    if (playerName != null) {
      connectionUrl += `&playerName=${encodeURIComponent(playerName)}`
    }

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(connectionUrl)
      .build();

    connection.on('NewPlayer', (newPlayer: INewPlayer) => {
      const player: IPlayer = {
        id: newPlayer.playerId,
        token: newPlayer.token
      }
      this.player.next(player);
    });

    connection.on('WorldUpdated', (worldUpdated: IWorldUpdated) => {
      this.worldUpdates.next(worldUpdated);
    });

    connection.start();
  }

  getWorldUpdates(): Observable<IWorldUpdated> {
    return this.worldUpdates;
  }

  getPlayer(): Observable<IPlayer> {
    return this.player;
  }
}

interface IWorldUpdated {
  potentiallyChangedResources: string[];
}

interface INewPlayer {
  playerId: PlayerId;
  token: string;
}

export interface IPlayer {
  id: PlayerId;
  token: string;
}
