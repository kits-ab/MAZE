import { TestBed } from '@angular/core/testing';

import { GameEventService } from './game-event.service';

describe('GameEventService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: GameEventService = TestBed.get(GameEventService);
    expect(service).toBeTruthy();
  });
});
