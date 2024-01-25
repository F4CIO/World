import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GoProPageComponent } from './go-pro.component';

describe('GoProComponent', () => {
  let component: GoProPageComponent;
  let fixture: ComponentFixture<GoProPageComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GoProPageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GoProPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
