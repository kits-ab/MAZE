import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as d3 from 'd3';
import { GameService, ITile } from '../game.service';
import { GamesService, Character } from '@kokitotsos/maze-client-angular';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class GameComponent implements OnInit {
  private static readonly characterSize = 20;
  private readonly gameService = new GameService(this.activatedRoute.snapshot.params.id, this.gamesApi);

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly gamesApi: GamesService) {
  }

  ngOnInit() {

    const game$ = this.gameService.getGame();
    game$.subscribe(game => {
      const svg = d3.select('svg');
      svg.attr('viewBox', `${game.world.x} ${game.world.y} ${game.world.width} ${game.world.height}`);

      svg.select<SVGGElement>('.world')
        .selectAll<SVGRectElement, ITile>('rect')
        .data<ITile>(game.world.tiles, tile => `${tile.type} ${tile.x} ${tile.y} ${tile.width} ${tile.height}`)
        .enter()
        .append('rect')
        .attr('x', tile => tile.x)
        .attr('y', tile => tile.y)
        .attr('width', tile => tile.width)
        .attr('height', tile => tile.height)
        .attr('fill', location => `url(#${location.type})`)
        .exit()
        .remove();

      const characters = svg.select<SVGGElement>('.characters')
        .selectAll<SVGImageElement, any>('image')
        .data<any>(game.characters, character => character.id.toString());

      characters
        .attr('x', character => game.world.locationPositions.get(character.location).x - GameComponent.characterSize / 2)
        .attr('y', character => game.world.locationPositions.get(character.location).y - GameComponent.characterSize / 2)

      characters
        .enter()
        .append('image')
        .attr('class', 'character')
        .attr('x', character => game.world.locationPositions.get(character.location).x - GameComponent.characterSize / 2)
        .attr('y', character => game.world.locationPositions.get(character.location).y - GameComponent.characterSize / 2)
        .attr('width', GameComponent.characterSize)
        .attr('height', GameComponent.characterSize)
        .attr('href', character => `/assets/characters/${character.characterClass}.png`)
        .exit()
        .remove();
    });
  }
}

