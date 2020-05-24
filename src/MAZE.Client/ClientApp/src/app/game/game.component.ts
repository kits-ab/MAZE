import { Component, OnInit } from '@angular/core';
import * as d3 from 'd3';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss']
})
export class GameComponent implements OnInit {
  private static readonly tileSize = 32;

  ngOnInit() {
    const tiles: ITile[] = [
      { x: 0, y: 0, image: '/assets/castle/left.png' },
      { x: 0, y: GameComponent.tileSize, image: '/assets/castle/left.png' }
    ];

    const svg = d3.select('svg');
    svg
      .selectAll('image')
      .data<ITile>(tiles)
      .enter()
      .append('image')
      .attr('x', location => location.x)
      .attr('y', location => location.y)
      .attr('width', GameComponent.tileSize)
      .attr('height', GameComponent.tileSize)
      .attr('href', location => location.image);
  }

}

export interface ITile {
  x: number;
  y: number;
  image: string;
}
