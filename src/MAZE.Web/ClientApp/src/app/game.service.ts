import { GamesService, Location, Path, Character, CharacterClass, Player, Obstacle } from '@kokitotsos/maze-client-angular';
import { Observable, combineLatest, of } from 'rxjs';
import { map, flatMap, filter } from 'rxjs/operators';
import { GameEventService } from './game-event.service';

export class GameService {
  private static readonly tileSize = 30;
  private static readonly wallsPerTile = 6;

  private readonly gameEventService = new GameEventService(this.gameId);
  private originLocationId: LocationId | null = null;

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

    const players$ = worldUpdates$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('players')))
      .pipe(flatMap(_ => this.gamesApi.getPlayers(this.gameId)));

    const obstacles$ = worldUpdates$
      .pipe(filter(worldUpdate => worldUpdate.potentiallyChangedResources.includes('obstacles')))
      .pipe(flatMap(_ => this.gamesApi.getObstacles(this.gameId)));

    const world$ = combineLatest(locationsAndPaths$, obstacles$)
      .pipe(map(([[locations, paths], obstacles]) => {
        latestLocations = locations;
        latestPaths = paths;
        const locationData = this.buildLocationData(locations, paths, obstacles);
        const locationPositions = this.buildLocationPositions(locationData);
        const tiles = this.buildTiles(locationData);
        const world = this.createWorld(tiles, locationPositions);
        return world;
      }));

    const game$ = combineLatest(world$, characters$, players$)
      .pipe(map(([world, characterDtos, playerDtos]) => {
        const characters: ICharacter[] = characterDtos
          .filter(character => world.locationPositions.get(character.location) != null)
          .map(this.convertCharacter);

        const players: IPlayer[] = playerDtos.map(this.convertPlayer);

        const game: IGame = {
          world: world,
          characters: characters,
          players: players
        };

        return game;
      }));

    return game$;
  }

  private convertCharacter(character: Character): ICharacter {
    return {
      id: character.id,
      location: character.location,
      characterClass: character.characterClass
    };
  }

  private convertPlayer(player: Player): IPlayer {
    return {
      id: player.id,
      name: player.name
    };
  }

  private buildLocationData(locations: Location[], paths: Path[], obstacles: Obstacle[]): ILocationData[] {
    if (this.originLocationId === null) {
      this.originLocationId = locations[0].id;
    }
    const originLocationId: LocationId = this.originLocationId;
    const locationData = new Map<LocationId, ILocationData>(locations.map<[LocationId, ILocationData]>(location => [
      location.id,
      {
        locationId: location.id,
        x: undefined,
        y: undefined,
        visited: false,
        pathWest: null,
        pathEast: null,
        pathNorth: null,
        pathSouth: null,
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
          const blockingObstacle = obstacles.find(obstacle => obstacle.blockedPaths.includes(path.id));
          const pathState: PathState = blockingObstacle == null ? 'open' : this.convertToBlockade(blockingObstacle.type);

          switch (path.type) {
            case 'west':
              location.pathWest = {
                state: pathState,
                location: neighborLocation
              };
              break;

            case 'east':
              location.pathEast = {
                state: pathState,
                location: neighborLocation
              };
              break;

            case 'north':
              location.pathNorth = {
                state: pathState,
                location: neighborLocation
              };
              break;

            case 'south':
              location.pathSouth = {
                state: pathState,
                location: neighborLocation
              };
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

  private convertToBlockade(obstacleType: Obstacle.TypeEnum): Blockade {
    switch (obstacleType) {
      case 'forceField':
        return 'force-wall';

      case 'lock':
        return 'lock';

      case 'stone':
        return 'stone';

      case 'ghost':
        return 'spirits';
    }
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

      const hasInnerCornerRightBottom = location.pathWest === null && location.pathNorth === null;
      if (hasInnerCornerRightBottom) {
        tiles.push({
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-right-bottom'
        });
      }

      const hasInnerCornerLeftBottom = location.pathEast === null && location.pathNorth === null;
      if (hasInnerCornerLeftBottom) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-left-bottom'
        });
      }

      const hasInnerCornerRightTop = location.pathWest === null && location.pathSouth === null;
      if (hasInnerCornerRightTop) {
        tiles.push({
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-right-top'
        });
      }

      const hasInnerCornerLeftTop = location.pathEast === null && location.pathSouth === null;
      if (hasInnerCornerLeftTop) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-inner-left-top'
        });
      }

      const hasOuterCornerRightTop = location.pathNorth !== null && location.pathEast !== null && (location.pathNorth.location !== null && location.pathNorth.location.pathEast === null || location.pathEast.location !== null && location.pathEast.location.pathNorth === null);
      if (hasOuterCornerRightTop) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-right-top'
        });
      }

      const hasOuterCornerLeftTop = location.pathNorth !== null && location.pathWest !== null && (location.pathNorth.location !== null && location.pathNorth.location.pathWest === null || location.pathWest.location !== null && location.pathWest.location.pathNorth === null);
      if (hasOuterCornerLeftTop) {
        tiles.push({
          x: location.x,
          y: location.y,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-left-top'
        });
      }

      const hasOuterCornerRightBottom = location.pathSouth !== null && location.pathEast !== null && (location.pathSouth.location !== null && location.pathSouth.location.pathEast === null || location.pathEast.location !== null && location.pathEast.location.pathSouth === null);
      if (hasOuterCornerRightBottom) {
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-right-bottom'
        });
      }

      const hasOuterCornerLeftBottom = location.pathSouth !== null && location.pathWest !== null && (location.pathSouth.location !== null && location.pathSouth.location.pathWest === null || location.pathWest.location !== null && location.pathWest.location.pathSouth === null);
      if (hasOuterCornerLeftBottom) {
        tiles.push({
          x: location.x,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: 'wall-corner-outer-left-bottom'
        });
      }

      const isTopLeftCornerOccupied = hasInnerCornerRightBottom || hasOuterCornerLeftTop;
      const isTopRightCornerOccupied = hasInnerCornerLeftBottom || hasOuterCornerRightTop;
      const isBottomLeftCornerOccupied = hasInnerCornerRightTop || hasOuterCornerLeftBottom;
      const isBottomRightCornerOccupied = hasInnerCornerLeftTop || hasOuterCornerRightBottom;

      if (location.pathWest === null || location.pathWest.state !== 'open') {
        const isExpansionNorthBlocked = isTopLeftCornerOccupied || location.pathNorth === null;
        const isExpansionSouthBlocked = isBottomLeftCornerOccupied || location.pathSouth === null;
        tiles.push({
          x: location.x,
          y: location.y + (isExpansionNorthBlocked ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize - ((isExpansionNorthBlocked ? 1 : 0) + (isExpansionSouthBlocked ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
          type: location.pathWest !== null && location.pathWest.state !== 'open' ? location.pathWest.state : 'wall-left'
        });
      }

      if (location.pathEast === null || location.pathEast.state !== 'open') {
        const isExpansionNorthBlocked = isTopRightCornerOccupied || location.pathNorth === null;
        const isExpansionSouthBlocked = isBottomRightCornerOccupied || location.pathSouth === null;
        tiles.push({
          x: location.x + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          y: location.y + (isExpansionNorthBlocked ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
          width: GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize - ((isExpansionNorthBlocked ? 1 : 0) + (isExpansionSouthBlocked ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
          type: location.pathEast !== null && location.pathEast.state !== 'open' ? location.pathEast.state : 'wall-right'
        });
      }

      if (location.pathNorth === null || location.pathNorth.state !== 'open') {
        const isExpansionWestBlocked = isTopLeftCornerOccupied || location.pathWest === null;
        const isExpansionEastBlocked = isTopRightCornerOccupied || location.pathEast === null;
        tiles.push({
          x: location.x + (isExpansionWestBlocked ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
          y: location.y,
          width: GameService.tileSize - ((isExpansionWestBlocked ? 1 : 0) + (isExpansionEastBlocked ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: location.pathNorth !== null && location.pathNorth.state !== 'open' ? location.pathNorth.state : 'wall-top'
        });
      }

      if (location.pathSouth === null || location.pathSouth.state !== 'open') {
        const isExpansionWestBlocked = isBottomLeftCornerOccupied || location.pathWest === null;
        const isExpansionEastBlocked = isBottomRightCornerOccupied || location.pathEast === null;
        tiles.push({
          x: location.x + (isExpansionWestBlocked ? 1 : 0) * GameService.tileSize / GameService.wallsPerTile,
          y: location.y + (1 - 1 / GameService.wallsPerTile) * GameService.tileSize,
          width: GameService.tileSize - ((isExpansionWestBlocked ? 1 : 0) + (isExpansionEastBlocked ? 1 : 0)) * GameService.tileSize / GameService.wallsPerTile,
          height: GameService.tileSize / GameService.wallsPerTile,
          type: location.pathSouth !== null && location.pathSouth.state !== 'open' ? location.pathSouth.state : 'wall-bottom'
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

  pathWest: IPathData | null;
  pathEast: IPathData | null;
  pathNorth: IPathData | null;
  pathSouth: IPathData | null;
}

interface IPathData {
  state: PathState;
  location: ILocationData | null;
}

type Blockade = 'force-wall' | 'lock' | 'stone' | 'spirits';

type PathState = 'open' | Blockade;

export interface IGame {
  world: IWorld;
  characters: ICharacter[];
  players: IPlayer[];
}

export interface ICharacter {
  id: CharacterId;
  location: LocationId;
  characterClass: CharacterClass;
}

export interface IPlayer {
  id: PlayerId;
  name: string;
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
  'wall-corner-outer-left-bottom' | 'wall-corner-outer-left-top' | 'wall-corner-outer-right-bottom' | 'wall-corner-outer-right-top' |
  Blockade;

type PathType = 'west' | 'east' | 'north' | 'south' | 'portal';
