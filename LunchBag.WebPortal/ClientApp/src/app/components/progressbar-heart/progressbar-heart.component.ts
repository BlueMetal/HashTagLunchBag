import { GoalStatusTotal } from '../../models/goal-status-total';
import { Component, OnInit, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'progressbar-heart',
  templateUrl: './progressbar-heart.component.html',
  styleUrls: ['./progressbar-heart.component.scss']
})
export class ProgressbarHeartComponent implements OnInit, OnChanges {
  @Input() heartGoalStatusTotal: GoalStatusTotal;

  scaleMaxVal = 440;
  goalLabelScale = 300;


  constructor() { }

  ngOnInit() {
    // console.log("Heart Goal: ", this.heartGoalStatusTotal);
  }

  ngOnChanges() {
    // console.log("Heart Goal On Change: ", this.heartGoalStatusTotal ,"| Masked Value:",this.getMaskedValue());
  }

  getMaskedValue(): number {
    if (!this.heartGoalStatusTotal.goalStatus) { return this.scaleMaxVal; }

    let filledValue = (this.heartGoalStatusTotal.goalStatus / this.heartGoalStatusTotal.goal) * this.scaleMaxVal;
    return Math.round((this.scaleMaxVal - filledValue));
  }

  getGoalLabelScaledValue(): number {
    let retValue = 0;
    if (!this.heartGoalStatusTotal.goalStatus) {
      retValue = 0;
    } else {
      retValue = Math.round(((this.goalLabelScale / 2) - (this.heartGoalStatusTotal.goalStatus / this.heartGoalStatusTotal.goal) * this.goalLabelScale));
      //  console.log("getGoalLabelScaledValue: ", retValue);
    }

    return retValue;
  }


}
