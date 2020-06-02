import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import * as d3 from 'd3';
import { GameService, ITile } from '../game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss']
})
export class GameComponent implements OnInit {
  

  constructor(readonly activatedRoute: ActivatedRoute, readonly gameService: GameService) {
  }

  ngOnInit() {

    const gameId = this.activatedRoute.snapshot.params.id;

    const world$ = this.gameService.getWorld(gameId);
    world$.subscribe(world => {
      d3
        .select('svg')
        .attr('viewBox', `${world.x} ${world.y} ${world.width} ${world.height}`)
        .selectAll<SVGImageElement, ITile>('image')
        .data<ITile>(world.tiles, tile => `${tile.locationId} ${tile.x} ${tile.y} ${tile.width} ${tile.height}`)
        .enter()
        .append('image')
        .attr('x', tile => tile.x)
        .attr('y', tile => tile.y)
        .attr('width', tile => tile.width)
        .attr('height', tile => tile.height)
        .attr('href', location => location.image)
        .exit()
        .remove();
    });
  }
}

