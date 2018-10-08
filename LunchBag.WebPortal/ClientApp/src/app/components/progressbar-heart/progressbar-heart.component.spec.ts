import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProgressbarHeartComponent } from './progressbar-heart.component';

describe('ProgressbarHeartComponent', () => {
  let component: ProgressbarHeartComponent;
  let fixture: ComponentFixture<ProgressbarHeartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ProgressbarHeartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProgressbarHeartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
