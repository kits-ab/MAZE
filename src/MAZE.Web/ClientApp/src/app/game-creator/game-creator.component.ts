import { Component, OnInit } from '@angular/core';
import { GamesService, Game } from '@kokitotsos/maze-client-angular';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-game-creator',
  templateUrl: './game-creator.component.html',
  styleUrls: ['./game-creator.component.scss']
})
export class GameCreatorComponent implements OnInit {

  constructor(
    private readonly gameApi: GamesService,
    private readonly activatedRoute: ActivatedRoute,
    private readonly router: Router) {
  }

  ngOnInit() {
    const newGame: Game = {
      world: 'Castle'
    };
    this.gameApi.createGame(newGame).subscribe(createdGame => {
      if (this.activatedRoute.snapshot.params.withControl) {
        this.router.navigate(['/gameWithControl', createdGame.id, 'FallenMaster']);
      }
      else {
        this.router.navigate(['/game', createdGame.id ]);
      }
    });
  }
}
