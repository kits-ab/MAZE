import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class GameEventService {

  constructor() {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:44396/gameEvents')
      .build();

    connection.on('WorldUpdated', (worldUpdated: IWorldUpdated) => {
      console.log(worldUpdated.potentiallyChangedResources.join(',') + ' might needs update');
    });

    connection.start();
  }
}

interface IWorldUpdated {
  potentiallyChangedResources: string[];
}
