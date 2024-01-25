import { TestBed } from '@angular/core/testing';

import { NoGuard } from './no.guard';

describe('NoGuard', () => {
  let guard: NoGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(NoGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
