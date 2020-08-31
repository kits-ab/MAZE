import { GamesService, Location, Path, Character } from '@kokitotsos/maze-client-angular';
import { Observable, combineLatest, of } from 'rxjs';
import { map, flatMap, filter } from 'rxjs/operators';
import { GameEventService } from './game-event.service';

export class GameService {
  private static readonly tileSize = 30;
  private static readonly wallsPerTile = 6;

  private readonly gameEventService = new GameEventService(this.gameId);

  constructor(private readonly gameId: GameId, private readonly gamesApi: GamesService) {
  }

  getGame(): Observable<IGame> {
    const worldUpdates$ = this.gameEventService.getWorldUpdates();

    let latestLocations: Location[] = [];
    let latestPaths: Path[] = [];

    const locationsAndPaths$ = worldUpdates$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('locations') || worldUpdate.potentiallyChangedResources.includes('paths')))
      .pipe(flatMap(worldUpdate => {
        const locations$ = worldUpdate.potentiallyChangedResources.includes('locations') ? this.gamesApi.getLocations(this.gameId) : of(latestLocations);
        const paths$ = worldUpdate.potentiallyChangedResources.includes('paths') ? this.gamesApi.getPaths(this.gameId) : of(latestPaths);

        return combineLatest(locations$, paths$);
      }));

    const characters$ = worldUpdates$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('characters')))
      .pipe(flatMap(_ => this.gamesApi.getCharacters(this.gameId)));

    const world$ = locationsAndPaths$
      .pipe(map(([locations, paths]) => {
        latestLocations = locations;
        latestPaths = paths;
        const locationData = this.buildLocationData(locations, paths);
        const locationPositions = this.buildLocationPositions(locationData);
        const tiles = this.buildTiles(locationData);
        const world = this.createWorld(tiles, locationPositions);
        return world;
      }));

    const game$ = combineLatest(world$, characters$)
      .pipe(map(([world, characters]) => {
        const game: IGame = {
          world: world,
          characters: characters.filter(character => world.locationPositions.get(character.location) != null)
        };

        return game;
      }));

    return game$;
  }

  private buildLocationData(locations: Location[], paths: Path[]): ILocationData[] {
    const originLocationId = locations[0].id;
    const locationData = new Map<LocationId, ILocationData>(locations.map<[LocationId, ILocationData]>(location => [
      location.id,
      {
        locationId: location.id,
        x: undefined,
        y: undefined,
        visited: false,
        hasPathWest: false,
        locationWest: null,
        hasPathEast: false,
        locationEast: null,
        hasPathNorth: false,
        locationNorth: null,
        hasPathSouth: false,
        locationSouth: null
      }]));
    const originLocationPosition = locationData.get(originLocationId);
    if (originLocationPosition == null) {
      throw new Error('An origin location is expected to exist')
    }
    originLocationPosition.x = 0;
    originLocationPosition.y = 0;
    originLocationPosition.visited = true;

    const pathConnections = new Map<LocationId, [LocationId, PathType][]>();
    paths.forEach(path => {
      if (path.type === 'west' || path.type === 'east' || path.type === 'north' || path.type === 'south') {
        const pathConnectionsFrom = pathConnections.get(path.from);
        if (pathConnectionsFrom == null) {
          pathConnections.set(path.from, [[path.to, path.type]]);
        } else {
          pathConnectionsFrom.push([path.to, path.type]);
        }

        const location = locationData.get(path.from);
        if (location != null) {
          const connectedLocation = locationData.get(path.to);
          const neighborLocation = connectedLocation === undefined ? null : connectedLocation;

          switch (path.type) {
            case 'west':
              location.hasPathWest = true;
              location.locationWest = neighborLocation;
              break;

            case 'east':
              location.hasPathEast = true;
              location.locationEast = neighborLocation;
              break;

            case 'north':
              location.hasPathNorth = true;
              location.locationNorth = neighborLocation;
              break;

            case 'south':
              location.hasPathSouth = true;
              location.locationSouth = neighborLocation;
              break;
          }
        }
      }
    });

    const originPathConnection = pathConnections.get(originLocationId);

    if (originPathConnection == null) {
      throw new Error('Origin path connection is expected to exist')
    }

    originPathConnection.forEach(([originNeighborLocationId, directionFromParent]) => {
      this.traversePaths(originNeighborLocationId, originLocationId, directionFromParent, locationData, pathConnections);
    });

    return [...locationData.values()]
      .filter(locationData => locationData.x != null && locationData.y != null);
  }

  private buildLocationPositions(locationData: ILocationData[]): Map<LocationId, IPosition> {
    const locations = locationData.map(this.convertToLocationEntry);
    return new Map<LocationId, IPosition>(locations);
  }

  private convertToLocationEntry(location: ILocationData): [LocationId, IPosition] {
    if (location.x == null || location.y == null) {
      throw new Error('Cannot convert a location data without position');
    }
    return [location.locationId, { x: location.x + GameService.tileSize / 2, y: location.y + GameService.tileSize / 2 }];
  }

  private buildTiles(locationData: ILocationData[]): ITile[] {
    const tiles: ITile[] = [];

    locationData.forEach(location => {
      if (location.x == null || location.y == null) {
        throw new Error('All locations should have positions at this point');
      }

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

  private traversePaths(locationId: LocationId, parentLocationId: LocationId, directionFromParent: PathType, locationData: Map<LocationId, ILocationData>, pathConnections: Map<LocationId, [LocationId, PathType][]>): void {
    const location = locationData.get(locationId);
    if (location == null) {
      throw new Error('Location is expected to exist')
    }

    const parentLocation = locationData.get(parentLocationId);
    if (parentLocation == null || parentLocation.x == null || parentLocation.y == null) {
      throw new Error('A parent location is expected to exist with a position')
    }

    location.visited = true;
    switch (directionFromParent) {
      case 'west':
      location.x = parentLocation.x - GameService.tileSize;
      location.y = parentLocation.y;
      break;

      case 'east':
      location.x = parentLocation.x + GameService.tileSize;
      location.y = parentLocation.y;
      break;

      case 'north':
      location.x = parentLocation.x;
      location.y = parentLocation.y - GameService.tileSize;
      break;

      case 'south':
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

  private createWorld(tiles: ITile[], locationPositions: Map<LocationId, IPosition>): IWorld {
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
      tiles: tiles,
      locationPositions: locationPositions
    }
  }
}

export type GameId = string;

export type LocationId = number;

export type ObstacleId = number;

export type PathId = number;

export type CharacterId = number;

export type PlayerId = number;

interface ILocationData {
  locationId: LocationId;
  visited: boolean;

  x: number | undefined;
  y: number | undefined;

  hasPathWest: boolean;
  locationWest: ILocationData | null;

  hasPathEast: boolean;
  locationEast: ILocationData | null;

  hasPathNorth: boolean;
  locationNorth: ILocationData | null;

  hasPathSouth: boolean;
  locationSouth: ILocationData | null;
}

export interface IGame {
  world: IWorld;
  characters: Character[];
}

export interface IWorld {
  x: number;
  y: number;
  width: number;
  height: number;
  tiles: ITile[];
  locationPositions: Map<LocationId, IPosition>;
}

export interface ITile {
  x: number;
  y: number;
  width: number;
  height: number;
  type: TileType;
}

export interface IPosition {
  x: number;
  y: number;
}

export type TileType =
  'floor' |
  'wall-left' | 'wall-right' | 'wall-top' | 'wall-bottom' |
  'wall-corner-inner-left-bottom' | 'wall-corner-inner-left-top' | 'wall-corner-inner-right-bottom' | 'wall-corner-inner-right-top' |
  'wall-corner-outer-left-bottom' | 'wall-corner-outer-left-top' | 'wall-corner-outer-right-bottom' | 'wall-corner-outer-right-top';

type PathType = 'west' | 'east' | 'north' | 'south' | 'portal';
