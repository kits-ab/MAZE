import { Injectable } from '@angular/core';
import { GamesService, Location, Path } from '@kokitotsos/maze-client-angular';
import { Observable, combineLatest, of } from 'rxjs';
import { map, flatMap } from 'rxjs/operators';
import { GameEventService } from './game-event.service';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private static readonly tileSize = 30;
  private static readonly wallsPerTile = 6;

  constructor(private readonly gamesApi: GamesService) {
  }

  getWorld(gameId: GameId): Observable<IWorld> {
    let latestLocations: Location[] = null;
    let latestPaths: Path[] = null;

    const gameEventService = new GameEventService(gameId);
    const worldUpdates$ = gameEventService.getWorldUpdates();

    return worldUpdates$.pipe(flatMap(worldUpdate => {
      const locations$ = worldUpdate.potentiallyChangedResources.includes('locations') ? this.gamesApi.getLocations(gameId) : of(latestLocations);
      const paths$ = worldUpdate.potentiallyChangedResources.includes('paths') ? this.gamesApi.getPaths(gameId) : of(latestPaths);
      return combineLatest(locations$, paths$)
        .pipe(map(([locations, paths]) => {
          latestLocations = locations;
          latestPaths = paths;
          return this.buildWorld(latestLocations, latestPaths);
        }))
        .pipe(map(tiles => this.createWorld(tiles)));
    }));
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
        if (location != null) {
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
      }
    });

    const originPathConnection = pathConnections.get(originLocationId);

    originPathConnection.forEach(([originNeighborLocationId, directionFromParent]) => {
      this.traversePaths(originNeighborLocationId, originLocationId, directionFromParent, locationData, pathConnections);
    });

    const tiles: ITile[] = [];

    locationData.forEach(location => {
      tiles.push({
        x: location.x,
        y: location.y,
        width: GameService.tileSize,
        height: GameService.tileSize,
        type: 'floor'
      });

      const hasInnerCornerRightBottom = !location.hasPathWest && !location.hasPathNorth;
      if (hasInnerCornerRightBottom) {
        tiles.push({
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-right-bottom'
        });
      }

      const hasInnerCornerLeftBottom = !location.hasPathEast && !location.hasPathNorth;
      if (hasInnerCornerLeftBottom) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-left-bottom'
        });
      }

      const hasInnerCornerRightTop = !location.hasPathWest && !location.hasPathSouth;
      if (hasInnerCornerRightTop) {
        tiles.push({
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-right-top'
        });
      }

      const hasInnerCornerLeftTop = !location.hasPathEast && !location.hasPathSouth;
      if (hasInnerCornerLeftTop) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-left-top'
        });
      }

      if (!location.hasPathWest) {
        tiles.push({
            x: location.x,
            y: location.y + (hasInnerCornerRightBottom ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize - ((hasInnerCornerRightBottom ? 1 : 0) + (hasInnerCornerRightTop ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
            type: 'wall-left'
          });
      }

      if (!location.hasPathEast) {
        tiles.push({
            x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
            y: location.y + (hasInnerCornerLeftBottom ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
            width: GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize - ((hasInnerCornerLeftBottom ? 1 : 0) + (hasInnerCornerLeftTop ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
            type: 'wall-right'
          });
      }

      if (!location.hasPathNorth) {
        tiles.push({
            x: location.x + (hasInnerCornerRightBottom ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
            y: location.y,
            width: GameService.tileSize - ((hasInnerCornerRightBottom ? 1 : 0) + (hasInnerCornerLeftBottom ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            type: 'wall-top'
          });
        }

      if (!location.hasPathSouth) {
        tiles.push({
            x: location.x + (hasInnerCornerRightTop ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
            y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
            width: GameService.tileSize - ((hasInnerCornerRightTop ? 1 : 0) + (hasInnerCornerLeftTop ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
            height: GameService.tileSize / GameService.wallsPerTile,
            type: 'wall-bottom'
          });
      }

      if (location.hasPathNorth && location.hasPathEast && (location.locationNorth != null && !location.locationNorth.hasPathEast || location.locationEast != null && !location.locationEast.hasPathNorth)) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-right-top'
        });
      }

      if (location.hasPathNorth && location.hasPathWest && (location.locationNorth != null && !location.locationNorth.hasPathWest || location.locationWest != null && !location.locationWest.hasPathNorth)) {
        tiles.push({
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-left-top'
        });
      }

      if (location.hasPathSouth && location.hasPathEast && (location.locationSouth != null && !location.locationSouth.hasPathEast || location.locationEast != null && !location.locationEast.hasPathSouth)) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-right-bottom'
        });
      }

      if (location.hasPathSouth && location.hasPathWest && (location.locationSouth != null && !location.locationSouth.hasPathWest || location.locationWest != null && !location.locationWest.hasPathSouth)) {
        tiles.push({
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-left-bottom'
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
    if (pathConnection != null) {
      pathConnection.forEach(([neighborLocationId, neighborDirectionFromParent]) => {
        const neighborLocation = locationData.get(neighborLocationId);
        if (neighborLocation != null && !neighborLocation.visited) {
          this.traversePaths(
            neighborLocationId,
            locationId,
            neighborDirectionFromParent,
            locationData,
            pathConnections);
        }
      });
    }
  }

  private createWorld(tiles: ITile[]): IWorld {
    const leftValues = tiles.map(tile => tile.x);
    const rightValues = tiles.map(tile => tile.x + tile.width);
    const topValues = tiles.map(tile => tile.y);
    const bottomValues = tiles.map(tile => tile.y + tile.height);
    const minX = Math.min(...leftValues);
    const minY = Math.min(...topValues);
    const maxX = Math.max(...rightValues);
    const maxY = Math.max(...bottomValues);

    return {
      x: minX,
      y: minY,
      width: maxX - minX,
      height: maxY - minY,
      tiles: tiles
    }
  }
}

export type GameId = string;

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
  x: number;
  y: number;
  width: number;
  height: number;
  tiles: ITile[];
}

export interface ITile {
  x: number;
  y: number;
  width: number;
  height: number;
  type: TileType;
}

export type TileType =
  'floor' |
  'wall-left' | 'wall-right' | 'wall-top' | 'wall-bottom' |
  'wall-corner-inner-left-bottom' | 'wall-corner-inner-left-top' | 'wall-corner-inner-right-bottom' | 'wall-corner-inner-right-top' |
  'wall-corner-outer-left-bottom' | 'wall-corner-outer-left-top' | 'wall-corner-outer-right-bottom' | 'wall-corner-outer-right-top';
