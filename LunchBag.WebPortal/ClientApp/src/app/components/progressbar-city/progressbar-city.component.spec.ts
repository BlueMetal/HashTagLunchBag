import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProgressbarCityComponent } from './progressbar-city.component';

describe('ProgressbarCityComponent', () => {
  let component: ProgressbarCityComponent;
  let fixture: ComponentFixture<ProgressbarCityComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProgressbarCityComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProgressbarCityComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
