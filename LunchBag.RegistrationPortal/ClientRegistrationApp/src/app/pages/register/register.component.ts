

import { Component, OnInit, Input, Output, EventEmitter } from "@angular/core";
import { Donate, NavSteps } from '../../models';
@Component({
  selector: "app-register",
  templateUrl: "./register.component.html",
  styleUrls: ["./register.component.scss"]
})
export class RegisterComponent implements OnInit {
  @Input() donate: Donate;
  @Input() step;
  @Output() eventFromPage = new EventEmitter<object>();

  constructor() {}
  ngOnInit() {}

  goBack() {
    this.step = NavSteps.Start;
    this.eventFromPage.emit({
      step: this.step
    });
  }

  eventFromForm(passed) {
    if (this.donate.type !== "creditcard") {
      location.reload();
    } else {
      this.step = NavSteps.Complete;
      this.eventFromPage.emit({
        step: this.step
      });
    }
  }

  isHidden(): boolean {
    if (this.step === NavSteps.Register) return false;
    else return true;
  }

}
