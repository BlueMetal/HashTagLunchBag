import { ConstantData } from '../../models';
import { PbarCity } from '../../models/pbar-city';
import { element, browser } from 'protractor';
import { EventLocation } from '../../models/event-location';
import { GoalStatusTotal } from '../../models/goal-status-total';
import { Events } from '../../models/events';
import { Component, OnInit, OnChanges, HostListener  } from '@angular/core';
import { HttpClientService } from '../../services/http-client.service';
import { SignalrClientService } from '../../services/signalr-client.service';
import { trigger, state, style, transition, animate, keyframes } from '@angular/animations';
import { HubConnection } from '@aspnet/signalr';
import * as signalR from '@aspnet/signalr';
import { Http } from '@angular/http';

@Component({
  selector: 'app-all-cities-status',
  templateUrl: './all-cities-status.component.html',
  styleUrls: ['./all-cities-status.component.scss']
})
export class AllCitiesStatusComponent implements OnInit {

  private currentEvent = ConstantData.EventCityeName;
  eventsInfo: Events;
  pbarCitySize: PbarCity = {height: 0, width: 550};
  pbarSizeRatio = 3.5;
  goalStatusTotal: GoalStatusTotal;
  pBarPosition = ['right', 'middle', 'left'];
  pbarImageArr = ['group1.jpg', 'group2.jpg', 'group3.jpg'];
  pbarClass = ['pbar-area-left', 'pbar-area-mid', 'pbar-area-right'];


  constructor(private _httpClient: HttpClientService,
    public _signalRClient: SignalrClientService) {
      this.onResize();
  }

  @HostListener('window:resize', ['$event'])
    onResize(event?) {
      this.pbarCitySize.height = Math.round(window.innerHeight / this.pbarSizeRatio);
      this.pbarCitySize.width = Math.round (window.innerWidth / this.pbarSizeRatio);
  }


  ngOnInit() {
    this._httpClient.getEventDetails(this.currentEvent).subscribe(response => {
      this.eventsInfo = response.json();
      this.goalStatusTotal = this.getTotalGoalStatus(this.eventsInfo.eventLocations);
      this._signalRClient.eventLocationInfoArr = this.eventsInfo.eventLocations;
      this._signalRClient.connectSignalR();
    });

  }

  getTotalGoalStatus(argEventLocation: EventLocation[]): GoalStatusTotal {
    let retFinalGoalStatus: GoalStatusTotal = {goal: 0, goalStatus: 0};
    let totalAchivedGoalStatus = 0;
    let finalGoalStatus = 0;

    argEventLocation.forEach(item => {
      totalAchivedGoalStatus += item.goalStatus;
      finalGoalStatus += item.goal;
    });

    retFinalGoalStatus.goal = finalGoalStatus;
    retFinalGoalStatus.goalStatus = totalAchivedGoalStatus;
    return retFinalGoalStatus;

  }


}
