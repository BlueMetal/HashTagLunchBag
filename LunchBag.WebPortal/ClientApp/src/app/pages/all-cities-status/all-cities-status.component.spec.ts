import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AllCitiesStatusComponent } from './all-cities-status.component';

describe('AllCitiesStatusComponent', () => {
  let component: AllCitiesStatusComponent;
  let fixture: ComponentFixture<AllCitiesStatusComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllCitiesStatusComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllCitiesStatusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
