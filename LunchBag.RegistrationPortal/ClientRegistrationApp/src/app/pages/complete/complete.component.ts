import { Component, OnInit, Input } from "@angular/core";
import { NavSteps } from '../../models';

@Component({
  selector: "app-complete",
  templateUrl: "./complete.component.html",
  styleUrls: ["./complete.component.scss"]
})
export class CompleteComponent implements OnInit {
  @Input() step;

  constructor() {}
  ngOnInit() {}

  onSubmit() {
    location.reload();
  }

  isHidden(): boolean {
    if (this.step === NavSteps.Complete) return false;
    else return true;
  }
}
