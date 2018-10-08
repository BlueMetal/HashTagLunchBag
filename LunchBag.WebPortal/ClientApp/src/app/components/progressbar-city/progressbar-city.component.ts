import { PbarCity } from '../../models/pbar-city';
import { EventLocation } from '../../models/event-location';
import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { SignalrClientService } from '../../services/signalr-client.service';
import { flyingHeart } from '../../utils/custom-animation';


@Component({
  selector: 'progressbar-city',
  templateUrl: './progressbar-city.component.html',
  styleUrls: ['./progressbar-city.component.scss'],
  animations: [
    flyingHeart
  ]
})
export class ProgressbarCityComponent implements OnInit, OnChanges {

  @Input() eventLocationInfo: EventLocation;
  @Input() cityPBarSize: PbarCity;
  @Input() pBarPosition: string;
  @Input() imageName = '';

  progressBarHeight;

  heartArray: string[] = [];
  heartCounter = 0;

  constructor(public _signalRClient: SignalrClientService) {

   }

   ngOnChanges() {
     console.log('Received changes for :' + this.eventLocationInfo);
     this.addHeart();
   }

  addHeart() {
    let heartId = 'Heart' + this.heartCounter++;
    this.heartArray.push(heartId);

    setTimeout(() => {
      this.heartArray.shift();
    }, 2000 );

  }

  ngOnInit() {
    this.progressBarHeight = this.cityPBarSize.height;
    this._signalRClient.observableGoalStatusMsg.subscribe(item => {
        if (item.locationId === this.eventLocationInfo.id) {
          console.log('Observable Reveived Data: ', item);
          this.addHeart();
          setTimeout(() => {
            this.addHeart();
            setTimeout(() => {
              this.addHeart();
            }, 300);
          }, 1000);
        }
    });
  }

  getAchievedGoal(): number {
    let goalStautsInPercentage = (this.eventLocationInfo.goalStatus / this.eventLocationInfo.goal) * 100;
    return parseFloat (Math.round(goalStautsInPercentage).toFixed(1));
  }

  getRemainingGoalHeight(): string {
    let remainingGoal = 100 - this.getAchievedGoal();
    let remainingGoalHeight = Math.floor((remainingGoal / 100) * this.progressBarHeight);
    return remainingGoalHeight + 'px';
  }

  getImagePath(): string {
    return '../../../assets/images/group/' + this.imageName;
  }

  getAchievedGoalHeight(): string {
    let achievedGoalHeight = Math.ceil((this.getAchievedGoal() / 100) * this.progressBarHeight) - 1;
    return achievedGoalHeight + 'px';
  }

}
