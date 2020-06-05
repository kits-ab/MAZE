import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GameWithControlComponent } from './game-with-control.component';

describe('GameWithControlComponent', () => {
  let component: GameWithControlComponent;
  let fixture: ComponentFixture<GameWithControlComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GameWithControlComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GameWithControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
