import { Injectable } from '@angular/core';
import { GamesService, Location, Path } from '@kokitotsos/maze-client-angular';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private static readonly tileSize = 32;
  private static readonly wallsPerTile = 4;

  constructor(private readonly gamesApi: GamesService) {
  }

  getTiles(gameId: GameId): Observable<ITile[]> {
    const locations$ = this.gamesApi.getLocations(gameId);
    const paths$ = this.gamesApi.getPaths(gameId);
    return combineLatest(locations$, paths$).pipe(map(([locations, paths]) => this.buildWorld(locations, paths)));
  }

  private buildWorld(locations: Location[], paths: Path[]): ITile[] {
    const originLocationId = locations[0].id;
    const locationData = new Map<LocationId, ILocationData>(locations.map<[LocationId, ILocationData]>(location => [location.id, { locationId: location.id, x: undefined, y: undefined, visited: false, hasPathWest: false, hasPathEast: false, hasPathNorth: false, hasPathSouth: false}]));
    const originLocationPosition = locationData.get(originLocationId);
    originLocationPosition.x = 0;
    originLocationPosition.y = 0;
    originLocationPosition.visited = true;

    const pathConnections = new Map<LocationId, [LocationId, Path.TypeEnum][]>();
    paths.forEach(path => {
      if (path.type === 'West' || path.type === 'East' || path.type === 'North' || path.type === 'South') {
        if (!pathConnections.has(path.from)) {
          pathConnections.set(path.from, [[path.to, path.type]]);
        } else {
          pathConnections.get(path.from).push([path.to, path.type]);
        }

        const location = locationData.get(path.from);

        switch (path.type) {
          case 'West':
            location.hasPathWest = true;
            break;

          case 'East':
            location.hasPathEast = true;
            break;

          case 'North':
            location.hasPathNorth = true;
            break;

          case 'South':
            location.hasPathSouth = true;
            break;
        }
      }
    });

    const originPathConnection = pathConnections.get(originLocationId);

    originPathConnection.forEach(([originNeighborLocationId, directionFromParent]) => {
      this.traversePaths(originNeighborLocationId, originLocationId, directionFromParent, locationData, pathConnections);
    });

    const tiles: ITile[] = [];

    locationData.forEach(location => {
      tiles.push({
        locationId: location.locationId,
        x: location.x,
        y: location.y,
        width: GameService.tileSize,
        height: GameService.tileSize,
        image: '/assets/castle/Floor.png'
      });

      if (!location.hasPathWest) {
        for (let y = 1; y < GameService.wallsPerTile - 1; y++) {
          tiles.push({
            locationId: location.locationId,
            x: location.x,
            y: location.y + y * GameService.tileSize / GameService.wallsPerTile,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            image: '/assets/castle/Left.png'
          });
        }
      }

      if (!location.hasPathEast) {
        for (let y = 1; y < GameService.wallsPerTile - 1; y++) {
          tiles.push({
            locationId: location.locationId,
            x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
            y: location.y + y * GameService.tileSize / GameService.wallsPerTile,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            image: '/assets/castle/Right.png'
          });
        }
      }

      if (!location.hasPathNorth) {
        for (let x = 1; x < GameService.wallsPerTile - 1; x++) {
          tiles.push({
            locationId: location.locationId,
            x: location.x + x * GameService.tileSize / GameService.wallsPerTile,
            y: location.y,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            image: '/assets/castle/Top.png'
          });
        }
      }

      if (!location.hasPathSouth) {
        for (let x = 1; x < GameService.wallsPerTile - 1; x++) {
          tiles.push({
            locationId: location.locationId,
            x: location.x + x * GameService.tileSize / GameService.wallsPerTile,
            y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            image: '/assets/castle/Bottom.png'
          });
        }
      }

      if (!location.hasPathWest && !location.hasPathNorth) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerRightBottom.png'
        });
      }

      if (!location.hasPathEast && !location.hasPathNorth) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerLeftBottom.png'
        });
      }

      if (!location.hasPathWest && !location.hasPathSouth) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerRightTop.png'
        });
      }

      if (!location.hasPathEast && !location.hasPathSouth) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerLeftTop.png'
        });
      }

      if (!location.hasPathWest && !location.hasPathNorth) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerRightBottom.png'
        });
      }
    });

    return tiles;
  }

  private traversePaths(locationId: LocationId, parentLocationId: LocationId, directionFromParent: Path.TypeEnum, locationData: Map<LocationId, ILocationData>, pathConnections: Map<LocationId, [LocationId, Path.TypeEnum][]>): void {
    const location = locationData.get(locationId);
    const parentLocation = locationData.get(parentLocationId);
    location.visited = true;
    switch (directionFromParent) {
      case Path.TypeEnum.West:
        location.x = parentLocation.x - GameService.tileSize;
        location.y = parentLocation.y;
        break;

      case Path.TypeEnum.East:
        location.x = parentLocation.x + GameService.tileSize;
        location.y = parentLocation.y;
        break;

      case Path.TypeEnum.North:
        location.x = parentLocation.x;
        location.y = parentLocation.y - GameService.tileSize;
        break;

      case Path.TypeEnum.South:
        location.x = parentLocation.x;
        location.y = parentLocation.y + GameService.tileSize;
        break;
    }

    const pathConnection = pathConnections.get(locationId);
    pathConnection.forEach(([neighborLocationId, directionFromParent]) => {
      if (!locationData.get(neighborLocationId).visited) {
        this.traversePaths(neighborLocationId, locationId, directionFromParent, locationData, pathConnections);
      }
    });
  }
}

type GameId = string;

type LocationId = number;

interface ILocationData {
  locationId: LocationId;
  visited: boolean;
  x: number;
  y: number;
  hasPathWest: boolean;
  hasPathEast: boolean;
  hasPathNorth: boolean;
  hasPathSouth: boolean;
}

export interface ITile {
  locationId: LocationId;
  x: number;
  y: number;
  width: number;
  height: number;
  image: string;
}
