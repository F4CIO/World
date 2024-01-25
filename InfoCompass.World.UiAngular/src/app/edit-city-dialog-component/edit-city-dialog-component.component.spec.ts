import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCityDialogComponentComponent } from './edit-city-dialog-component.component';

describe('EditCityDialogComponentComponent', () => {
  let component: EditCityDialogComponentComponent;
  let fixture: ComponentFixture<EditCityDialogComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ EditCityDialogComponentComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCityDialogComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
