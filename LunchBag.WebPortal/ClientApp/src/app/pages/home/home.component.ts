
import { Component, OnInit } from '@angular/core';
import { HttpClientService } from '../../services/http-client.service';
import { ConstantData, Events, NavPages } from '../../models';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {

  private currentEvent = ConstantData.EventCityeName;
  private eventsInfo: Events;
  viewId = '';
  isEventActive = false;
  counter = 0;
  currentState = NavPages.Give;
  isResetNavFlag = false;
  cyclingInterval = 30; // default 30 Sec


  constructor(private _httpClient: HttpClientService,
    private activeRoute: ActivatedRoute,
    private router: Router) { }

  ngOnInit() {

    this.activeRoute.params.subscribe(params => {
      console.log('Params: ', params);
      this.viewId = params['viewId'];

      this._httpClient.getEventDetails(this.currentEvent, this.viewId).subscribe(response => {
        this.eventsInfo = response.json();
        this.isEventActive = this.eventsInfo.isEventActive;

        if (this.viewId !== undefined && this.viewId.length > 1) {
          if (this.eventsInfo.eventView) {
            this.cyclingInterval = this.eventsInfo.eventView.cyclingInterval;
            this.autoNavigate();
          }
        }
      });
    }
    );
  }

  isMobileDevice(): boolean {
    if (window.innerWidth < 600) return true;
    else return false;
  }

  donateNow() {
    window.location.href = 'https://donate.ready.gives/';
  }

  autoNavigate() {
    setInterval(() => {
      this._httpClient.getEventDetails(this.currentEvent).subscribe(response => {
        this.eventsInfo = response.json();
        if (this.eventsInfo.isEventActive && !this.isMobileDevice()) {
          this.isResetNavFlag = true;
          if (this.currentState === NavPages.Give) {
            this.router.navigate([NavPages.Prepare]);
            this.currentState = NavPages.Prepare;
          } else {
            this.router.navigate([NavPages.Give]);
            this.currentState = NavPages.Give;
          }
        } else {
          if (this.isResetNavFlag) {
            this.router.navigate([NavPages.Organize]);
            this.isResetNavFlag = false;
          }
        }
      });
    }, this.cyclingInterval * 1000);
  }

}
