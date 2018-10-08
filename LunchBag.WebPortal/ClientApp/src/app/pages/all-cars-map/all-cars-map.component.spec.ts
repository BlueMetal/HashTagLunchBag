import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllCarsMapComponent } from './all-cars-map.component';

describe('AllCarsMapComponent', () => {
  let component: AllCarsMapComponent;
  let fixture: ComponentFixture<AllCarsMapComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllCarsMapComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllCarsMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
