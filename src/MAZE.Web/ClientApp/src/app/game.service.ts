import { Injectable } from '@angular/core';
import { GamesService, Location, Path } from '@kokitotsos/maze-client-angular';
import { Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private static readonly tileSize = 30;
  private static readonly wallsPerTile = 6;

  constructor(private readonly gamesApi: GamesService) {
  }

  getTiles(gameId: GameId): Observable<ITile[]> {
    const locations$ = this.gamesApi.getLocations(gameId);
    const paths$ = this.gamesApi.getPaths(gameId);
    return combineLatest(locations$, paths$).pipe(map(([locations, paths]) => this.buildWorld(locations, paths)));
  }

  private buildWorld(locations: Location[], paths: Path[]): ITile[] {
    const originLocationId = locations[0].id;
    const locationData = new Map<LocationId, ILocationData>(locations.map<[LocationId, ILocationData]>(location => [
      location.id,
      {
        locationId: location.id,
        x: undefined,
        y: undefined,
        visited: false,
        hasPathWest: false,
        locationWest: undefined,
        hasPathEast: false,
        locationEast: undefined,
        hasPathNorth: false,
        locationNorth: undefined,
        hasPathSouth: false,
        locationSouth: undefined
      }]));
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
        const connectedLocation = locationData.get(path.to);

        switch (path.type) {
          case 'West':
            location.hasPathWest = true;
            location.locationWest = connectedLocation;
            break;

          case 'East':
            location.hasPathEast = true;
            location.locationEast = connectedLocation;
            break;

          case 'North':
            location.hasPathNorth = true;
            location.locationNorth = connectedLocation;
            break;

          case 'South':
            location.hasPathSouth = true;
            location.locationSouth = connectedLocation;
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

      const hasInnerCornerRightBottom = !location.hasPathWest && !location.hasPathNorth;
      if (hasInnerCornerRightBottom) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerRightBottom.png'
        });
      }

      const hasInnerCornerLeftBottom = !location.hasPathEast && !location.hasPathNorth;
      if (hasInnerCornerLeftBottom) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerLeftBottom.png'
        });
      }

      const hasInnerCornerRightTop = !location.hasPathWest && !location.hasPathSouth;
      if (hasInnerCornerRightTop) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerRightTop.png'
        });
      }

      const hasInnerCornerLeftTop = !location.hasPathEast && !location.hasPathSouth;
      if (hasInnerCornerLeftTop) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/InnerCornerLeftTop.png'
        });
      }

      if (!location.hasPathWest) {
        for (let y = hasInnerCornerRightBottom ? 1 : 0; y < GameService.wallsPerTile - (hasInnerCornerRightTop ? 1 : 0); y++) {
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
        for (let y = hasInnerCornerLeftBottom ? 1 : 0; y < GameService.wallsPerTile - (hasInnerCornerLeftTop ? 1 : 0); y++) {
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
        for (let x = hasInnerCornerRightBottom ? 1 : 0; x < GameService.wallsPerTile - (hasInnerCornerLeftBottom ? 1 : 0); x++) {
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
        for (let x = hasInnerCornerRightTop ? 1 : 0; x < GameService.wallsPerTile - (hasInnerCornerLeftTop ? 1 : 0); x++) {
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

      if (location.hasPathNorth && location.hasPathEast && (location.locationNorth != null && !location.locationNorth.hasPathEast || location.locationEast != null && !location.locationEast.hasPathNorth)) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/OuterCornerRightTop.png'
        });
      }

      if (location.hasPathNorth && location.hasPathWest && (location.locationNorth != null && !location.locationNorth.hasPathWest || location.locationWest != null && !location.locationWest.hasPathNorth)) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/OuterCornerLeftTop.png'
        });
      }

      if (location.hasPathSouth && location.hasPathEast && (location.locationSouth != null && !location.locationSouth.hasPathEast || location.locationEast != null && !location.locationEast.hasPathSouth)) {
        tiles.push({
          locationId: location.locationId,
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/OuterCornerRightBottom.png'
        });
      }

      if (location.hasPathSouth && location.hasPathWest && (location.locationSouth != null && !location.locationSouth.hasPathWest || location.locationWest != null && !location.locationWest.hasPathSouth)) {
        tiles.push({
          locationId: location.locationId,
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          image: '/assets/castle/OuterCornerLeftBottom.png'
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
  locationWest: ILocationData;

  hasPathEast: boolean;
  locationEast: ILocationData;

  hasPathNorth: boolean;
  locationNorth: ILocationData;

  hasPathSouth: boolean;
  locationSouth: ILocationData;
}

export interface IWorld {
  width: number;
  height: number;
  tiles: ITile[];
}

export interface ITile {
  locationId: LocationId;
  x: number;
  y: number;
  width: number;
  height: number;
  image: string;
}
