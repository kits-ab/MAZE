import { Component, OnInit } from '@angular/core';
import * as d3 from 'd3';
import { GameService, ITile } from '../game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss']
})
export class GameComponent implements OnInit {
  

  constructor(readonly gameService: GameService) {
  }

  ngOnInit() {
    const tiles$ = this.gameService.getTiles('0');
    tiles$.subscribe(tiles => {
      d3
        .select('svg')
        .selectAll<SVGImageElement, ITile>('image')
        .data<ITile>(tiles, tile => tile.locationId.toString())
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

