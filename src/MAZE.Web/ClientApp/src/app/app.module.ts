import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { ApiModule, BASE_PATH } from '@kokitotsos/maze-client-angular';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { GameComponent } from './game/game.component';
import { environment } from '../environments/environment';
import { GameControlComponent } from './game-control/game-control.component';
import { GameWithControlComponent } from './game-with-control/game-with-control.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    GameComponent,
    GameControlComponent,
    GameWithControlComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    ApiModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'game/:id', component: GameComponent },
      { path: 'gameControl/:id', component: GameControlComponent },
      { path: 'gameWithControl/:id', component: GameWithControlComponent }
    ])
  ],
  providers: [{ provide: BASE_PATH, useValue: environment.apiUrl }],
  bootstrap: [AppComponent]
})
export class AppModule { }
