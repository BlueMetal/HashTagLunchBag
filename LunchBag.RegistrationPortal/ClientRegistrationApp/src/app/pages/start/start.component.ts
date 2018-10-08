import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { NavSteps } from '../../models';

@Component({
  selector: "app-start",
  templateUrl: "./start.component.html",
  styleUrls: ["./start.component.scss"]
})
export class StartComponent implements OnInit {
  @Input() donate;
  @Input() step;
  @Output() eventFromPage = new EventEmitter<object>();

  chapter = {
    name: "Bronx Chapter",
    date: "October 28, 2018"
  };

  constructor() {}
  ngOnInit() {}

  eventFromForm(passed) {
    this.step = NavSteps.Register;
    this.eventFromPage.emit({
      step: this.step
    });
  }

  isHidden(): boolean {
    if (this.step === NavSteps.Start) return false;
    else return true;
  }
}
