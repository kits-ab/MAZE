import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as d3 from 'd3';
import { GameService, ITile, ICharacter, IPlayer } from '../game.service';
import { GamesService } from '@kokitotsos/maze-client-angular';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class GameComponent implements OnInit {
  private static readonly characterSize = 20;
  private readonly gameService = new GameService(this.activatedRoute.snapshot.params.id, this.gamesApi);
  readonly joinGameQrUrl = `/Games/${this.activatedRoute.snapshot.params.id}/Join/QR`;

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gamesApi: GamesService) {
  }

  ngOnInit() {
    const game$ = this.gameService.getGame();
    game$.subscribe(game => {
      const svg = d3.select<SVGElement, null>('svg');

      const svgElement = svg.node();

      if (svgElement == null) {
        throw new Error('No SVG element found');
      }

      const viewWidth = svgElement.getBoundingClientRect().width;
      const viewHeight = svgElement.getBoundingClientRect().height;

      const scale = Math.min(viewWidth / game.world.width, viewHeight / game.world.height);
      const offsetX = (viewWidth - game.world.width * scale) / 2 - game.world.x * scale;
      const offsetY = (viewHeight - game.world.height * scale) / 2 - game.world.y * scale;

      svg.select<SVGGElement>('.game')
        .style('transform', `translate(${offsetX}px, ${offsetY}px) scale(${scale})`);

      const tiles = svg.select<SVGGElement>('.world')
        .selectAll<SVGRectElement, ITile>('rect')
        .data(game.world.tiles, tile => `${tile.type} ${tile.x} ${tile.y} ${tile.width} ${tile.height}`)
            
      tiles.enter()
        .append('rect')
        .attr('x', tile => tile.x)
        .attr('y', tile => tile.y)
        .attr('width', tile => tile.width)
        .attr('height', tile => tile.height)
        .attr('fill', location => `url(#${location.type})`);

      tiles.exit()
        .remove();

      const characters = svg.select<SVGGElement>('.characters')
        .selectAll<SVGImageElement, ICharacter>('image')
        .data(game.characters, character => character.id.toString());

      characters
        .attr('x', character => game.world.locationPositions.get(character.location)!.x - GameComponent.characterSize / 2)
        .attr('y', character => game.world.locationPositions.get(character.location)!.y - GameComponent.characterSize / 2)

      characters
        .enter()
        .append('image')
        .attr('class', 'character')
        .attr('x', character => game.world.locationPositions.get(character.location)!.x - GameComponent.characterSize / 2)
        .attr('y', character => game.world.locationPositions.get(character.location)!.y - GameComponent.characterSize / 2)
        .attr('width', GameComponent.characterSize)
        .attr('height', GameComponent.characterSize)
        .attr('href', character => `/assets/characters/${character.characterClass}.png`)
        .exit()
        .remove();

      const players = d3.select<HTMLDivElement, null>('.players')
        .selectAll<HTMLParagraphElement, IPlayer>('p')
        .data(game.players, player => player.id.toString());

      players
        .enter()
        .append('p')
        .html(player => `${player.name} (${player.id})`);

      players
        .exit()
        .remove();
    });
  }
}

